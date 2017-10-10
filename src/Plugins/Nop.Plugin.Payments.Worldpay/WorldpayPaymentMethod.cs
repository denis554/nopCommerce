using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Worldpay.Domain;
using Nop.Plugin.Payments.Worldpay.Domain.Enums;
using Nop.Plugin.Payments.Worldpay.Domain.Models;
using Nop.Plugin.Payments.Worldpay.Domain.Requests;
using Nop.Plugin.Payments.Worldpay.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;

namespace Nop.Plugin.Payments.Worldpay
{
    /// <summary>
    /// Represents Worldpay payment method
    /// </summary>
    public class WorldpayPaymentMethod : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWebHelper _webHelper;
        private readonly WorldpayPaymentManager _worldpayPaymentManager;
        private readonly WorldpayPaymentSettings _worldpayPaymentSettings;

        #endregion

        #region Ctor

        public WorldpayPaymentMethod(ICurrencyService currencyService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderTotalCalculationService orderTotalCalculationService,
            ISettingService settingService,
            IStoreService storeService,
            IWebHelper webHelper,
            WorldpayPaymentManager worldpayPaymentManager,
            WorldpayPaymentSettings worldpayPaymentSettings)
        {
            this._currencyService = currencyService;
            this._customerService = customerService;
            this._localizationService = localizationService;
            this._logger = logger;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._settingService = settingService;
            this._storeService = storeService;
            this._webHelper = webHelper;
            this._worldpayPaymentManager = worldpayPaymentManager;
            this._worldpayPaymentSettings = worldpayPaymentSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Process a regular or recurring payment
        /// </summary>
        /// <param name="paymentRequest">Payment request parameters</param>
        /// <param name="isRecurringPayment">Whether it is a recurring payment</param>
        /// <returns>Process payment result</returns>
        private ProcessPaymentResult ProcessPayment(ProcessPaymentRequest paymentRequest, bool isRecurringPayment)
        {
            //create request parameters
            var request = CreateChargeRequest(paymentRequest, isRecurringPayment);

            //charge transaction
            var transaction = _worldpayPaymentSettings.TransactionMode == TransactionMode.Authorize
                ? _worldpayPaymentManager.Authorize(new AuthorizeRequest(request)) : _worldpayPaymentManager.Charge(request)
                ?? throw new NopException("An error occurred while processing. Error details in the log");

            //save card identifier to payment custom values for further purchasing
            if (isRecurringPayment && !string.IsNullOrEmpty(transaction.VaultData?.Token?.PaymentMethodId))
                paymentRequest.CustomValues.Add(_localizationService.GetResource("Plugins.Payments.Worldpay.Fields.StoredCard.Key"), transaction.VaultData.Token.PaymentMethodId);

            //return result
            var result = new ProcessPaymentResult
            {
                AvsResult = $"{transaction.AvsResult}. Code: {transaction.AvsCode}",
                Cvv2Result = $"{transaction.CvvResult}. Code: {transaction.CvvCode}",
                AuthorizationTransactionCode = transaction.AuthorizationCode
            };

            if (_worldpayPaymentSettings.TransactionMode == TransactionMode.Authorize)
            {
                result.AuthorizationTransactionId = transaction.TransactionId.ToString();
                result.AuthorizationTransactionResult = transaction.ResponseText;
                result.NewPaymentStatus = PaymentStatus.Authorized;
            }

            if (_worldpayPaymentSettings.TransactionMode == TransactionMode.Charge)
            {
                result.CaptureTransactionId = transaction.TransactionId.ToString();
                result.CaptureTransactionResult = transaction.ResponseText;
                result.NewPaymentStatus = PaymentStatus.Paid;
            }

            return result;
        }

        /// <summary>
        /// Create request parameters to charge transaction
        /// </summary>
        /// <param name="paymentRequest">Payment request parameters</param>
        /// <param name="isRecurringPayment">Whether it is a recurring payment</param>
        /// <returns>Charge request parameters</returns>
        private ChargeRequest CreateChargeRequest(ProcessPaymentRequest paymentRequest,bool isRecurringPayment)
        {
            //get customer
            var customer = _customerService.GetCustomerById(paymentRequest.CustomerId);
            if (customer == null)
                throw new NopException("Customer cannot be loaded");

            //whether USD is available
            var usdCurrency = _currencyService.GetCurrencyByCode("USD");
            if (usdCurrency == null)
                throw new NopException("USD currency cannot be loaded");

            //create common charge request parameters
            var request = new ChargeRequest
            {
                OrderId = CommonHelper.EnsureMaximumLength(paymentRequest.OrderGuid.ToString(), 25),
                TransactionDuplicateCheckType = TransactionDuplicateCheckType.NoCheck,
                ExtendedInformation = new ExtendedInformation
                {
                    InvoiceNumber = paymentRequest.OrderGuid.ToString(),
                    InvoiceDescription = $"Order from the '{_storeService.GetStoreById(paymentRequest.StoreId)?.Name}'"
                },
                PaymentVaultToken = new VaultToken
                {
                    PublicKey = _worldpayPaymentSettings.PublicKey
                }
            };

            //set amount in USD
            var amount = _currencyService.ConvertFromPrimaryStoreCurrency(paymentRequest.OrderTotal, usdCurrency);
            request.Amount = Math.Round(amount, 2);

            //get current shopping cart
            var shoppingCart = customer.ShoppingCartItems
                .Where(shoppingCartItem => shoppingCartItem.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(paymentRequest.StoreId).ToList();

            //whether there are non-downloadable items in the shopping cart
            var nonDownloadable = shoppingCart.Any(item => !item.Product.IsDownload);
            request.ExtendedInformation.GoodsType = nonDownloadable ? GoodsType.Physical : GoodsType.Digital;

            //try to get previously stored card details
            var storedCardKey = _localizationService.GetResource("Plugins.Payments.Worldpay.Fields.StoredCard.Key");
            if (paymentRequest.CustomValues.TryGetValue(storedCardKey, out object storedCardId) && !storedCardId.ToString().Equals(Guid.Empty.ToString()))
            {
                //check whether customer exists in Vault
                var vaultCustomer = _worldpayPaymentManager.GetCustomer(customer.CustomerGuid.ToString()) 
                    ?? throw new NopException("Failed to retrieve customer");

                //use previously stored card to charge
                request.PaymentVaultToken.CustomerId = vaultCustomer.CustomerId;
                request.PaymentVaultToken.PaymentMethodId = storedCardId.ToString();

                return request;
            }

            //or try to get the card token
            var cardTokenKey = _localizationService.GetResource("Plugins.Payments.Worldpay.Fields.Token.Key");
            if (!paymentRequest.CustomValues.TryGetValue(cardTokenKey, out object token) || string.IsNullOrEmpty(token?.ToString()))
                throw new NopException("Failed to get the card token");

            //remove the card token from payment custom values, since it is no longer needed
            paymentRequest.CustomValues.Remove(cardTokenKey);

            //whether to save card details for the future purchasing
            var saveCardKey = _localizationService.GetResource("Plugins.Payments.Worldpay.Fields.SaveCard.Key");
            if (paymentRequest.CustomValues.TryGetValue(saveCardKey, out object saveCardValue) && saveCardValue is bool saveCard && saveCard && !customer.IsGuest())
            {
                //remove the value from payment custom values, since it is no longer needed
                paymentRequest.CustomValues.Remove(saveCardKey);

                try
                {
                    //check whether customer exists and try to create the new one, if not exists
                    var vaultCustomer = _worldpayPaymentManager.GetCustomer(customer.CustomerGuid.ToString())
                        ?? _worldpayPaymentManager.CreateCustomer(new CreateCustomerRequest
                        {
                            CustomerId = customer.CustomerGuid.ToString(),
                            CustomerDuplicateCheckType = CustomerDuplicateCheckType.Ignore,
                            EmailReceiptEnabled = !string.IsNullOrEmpty(customer.Email),
                            Email = customer.Email,
                            FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                            LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                            Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company),
                            Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                            BillingAddress = new Address
                            {
                                Line1 = customer.BillingAddress?.Address1,
                                City = customer.BillingAddress?.City,
                                State = customer.BillingAddress?.StateProvince?.Abbreviation,
                                Country = customer.BillingAddress?.Country?.TwoLetterIsoCode,
                                Zip = customer.BillingAddress?.ZipPostalCode,
                                Company = customer.BillingAddress?.Company,
                                Phone = customer.BillingAddress?.PhoneNumber
                            }
                        }) ?? throw new NopException("Failed to create customer. Error details in the log");

                    //add card to the Vault after charge
                    request.AddToVault = true;
                    request.PaymentVaultToken.CustomerId = vaultCustomer.CustomerId;
                }
                catch (Exception exception)
                {
                    _logger.Warning(exception.Message, exception, customer);
                    if (isRecurringPayment)
                        throw new NopException("For recurring payments you need to save the card details");
                }
            } else if (isRecurringPayment)
                throw new NopException("For recurring payments you need to save the card details");

            //use card token to charge
            request.PaymentVaultToken.PaymentMethodId = token.ToString();

            return request;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentException(nameof(processPaymentRequest));

            return ProcessPayment(processPaymentRequest, false);
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //do nothing
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _worldpayPaymentSettings.AdditionalFee, _worldpayPaymentSettings.AdditionalFeePercentage);

            return result;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            if (capturePaymentRequest == null)
                throw new ArgumentException(nameof(capturePaymentRequest));

            //capture transaction
            var transaction = _worldpayPaymentManager.CaptureTransaction(new CaptureRequest
            {
                TransactionId = capturePaymentRequest.Order.AuthorizationTransactionId
            }) ?? throw new NopException("An error occurred while processing. Error details in the log");
            
            //sucessfully captured
            return new CapturePaymentResult
            {
                NewPaymentStatus = PaymentStatus.Paid,
                CaptureTransactionId = transaction.TransactionId.ToString(),
                CaptureTransactionResult = transaction.ResponseText
            };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            if (refundPaymentRequest == null)
                throw new ArgumentException(nameof(refundPaymentRequest));

            //whether USD is available
            var usdCurrency = _currencyService.GetCurrencyByCode("USD");
            if (usdCurrency == null)
                throw new NopException("USD currency cannot be loaded");

            //set amount in USD
            var amount = _currencyService.ConvertCurrency(refundPaymentRequest.AmountToRefund, refundPaymentRequest.Order.CurrencyRate);

            var transaction = _worldpayPaymentManager.Refund(new RefundRequest
            {
                TransactionId = refundPaymentRequest.Order.CaptureTransactionId,
                Amount = Math.Round(amount, 2),
                OrderId = CommonHelper.EnsureMaximumLength(Guid.NewGuid().ToString(), 25)
            }) ?? throw new NopException("An error occurred while processing. Error details in the log");
            
            //sucessfully refunded
            return new RefundPaymentResult
            {
                NewPaymentStatus = PaymentStatus.PartiallyRefunded
            };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            if (voidPaymentRequest == null)
                throw new ArgumentException(nameof(voidPaymentRequest));

            //void transaction
            var transaction = _worldpayPaymentManager.VoidTransaction(new VoidRequest
            {
                TransactionId = voidPaymentRequest.Order.AuthorizationTransactionId,
                OrderId = CommonHelper.EnsureMaximumLength(Guid.NewGuid().ToString(), 25),
                VoidType = VoidType.MerchantGenerated
            }) ?? throw new NopException("An error occurred while processing. Error details in the log");
            
            //sucessfully voided
            return new VoidPaymentResult
            {
                NewPaymentStatus = PaymentStatus.Voided
            };
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentException(nameof(processPaymentRequest));

            return ProcessPayment(processPaymentRequest, true);
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            if (cancelPaymentRequest == null)
                throw new ArgumentException(nameof(cancelPaymentRequest));

            //always success
            return new CancelRecurringPaymentResult();
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //it's not a redirection payment method. So we always return false
            return false;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentException(nameof(form));
            
            //try to get errors
            if (form.TryGetValue("Errors", out StringValues errorsString) && !StringValues.IsNullOrEmpty(errorsString))
                return new[] { errorsString.ToString() }.ToList();

            return new List<string>();
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentException(nameof(form));

            var paymentRequest = new ProcessPaymentRequest();

            //pass custom values to payment method
            if (form.TryGetValue("Token", out StringValues token) && !StringValues.IsNullOrEmpty(token))
                paymentRequest.CustomValues.Add(_localizationService.GetResource("Plugins.Payments.Worldpay.Fields.Token.Key"), token.ToString());

            if (form.TryGetValue("StoredCardId", out StringValues storedCardId) && !StringValues.IsNullOrEmpty(storedCardId) && !storedCardId.Equals(Guid.Empty.ToString()))
                paymentRequest.CustomValues.Add(_localizationService.GetResource("Plugins.Payments.Worldpay.Fields.StoredCard.Key"), storedCardId.ToString());

            if (form.TryGetValue("SaveCard", out StringValues saveCardValue) && !StringValues.IsNullOrEmpty(saveCardValue) && bool.TryParse(saveCardValue[0], out bool saveCard) && saveCard)
                paymentRequest.CustomValues.Add(_localizationService.GetResource("Plugins.Payments.Worldpay.Fields.SaveCard.Key"), saveCard);

            return paymentRequest;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentWorldpay/Configure";
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <param name="viewComponentName">View component name</param>
        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "PaymentWorldpay";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new WorldpayPaymentSettings
            {
                //default sandbox values
                DeveloperId = "12345678",
                DeveloperVersion = "1.2",
                UseSandbox = true,

                TransactionMode = TransactionMode.Charge
            });

            //locales
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Payments.Worldpay.Domain.TransactionMode.Authorize", "Authorize only");
            this.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Payments.Worldpay.Domain.TransactionMode.Charge", "Charge (authorize and capture)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.DeveloperId", "Developer ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.DeveloperId.Hint", "Specify developer ID of integrator as assigned by Worldpay (available after certification).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.DeveloperVersion", "Application version");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.DeveloperVersion.Hint", "Specify version number of the integrator's application (available after certification).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.PublicKey", "Public key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.PublicKey.Hint", "Specify the Public key. It will be sent to the email address that you signed up with during the sandbox sign-up process.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.SaveCard", "Save the card data for future purchasing");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.SaveCard.Key", "Save card details");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.SecureKey", "Secure key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.SecureKey.Hint", "Specify the Secure key. You can obtain the Secure Key by signing into the Virtual Terminal with the login credentials that you were emailed to you during the sign-up process. You will then need to navigate to Settings and click on the Key Management link.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.SecureNetId", "SecureNet ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.SecureNetId.Hint", "Specify the SecureNet ID. You will get this in an email shortly after signing up for your account.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.StoredCard", "Use a previously saved card");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.StoredCard.Key", "Pay using stored card token");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.StoredCard.SelectCard", "Select a card");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.Token.Key", "Pay using card token");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.TransactionMode", "Transaction mode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.TransactionMode.Hint", "Choose the transaction mode.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.UseSandbox", "Use sandbox");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.UseSandbox.Hint", "Determine whether to enable sandbox (testing environment).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.ValidateAddress", "Validate address");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Fields.ValidateAddress.Hint", "Determine whether to validate customers' billing addresses on processing payments.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.Instructions", @"
                <p>
                    For plugin configuration follow these steps:<br />
                    <br />
                    1. Sign up for a <a href=""http://www.worldpay.com/us/developers/developer-resources/developer-sandbox-signup"" target=""_blank"">sandbox account</a><br />
                    2. The SecureNet ID can be obtained from the email that you received during the sign-up process.<br />
                    3. You can obtain the Secure Key by signing into the Virtual Terminal with the login credentials that you were also emailed to you. 
                       You will then need to navigate to Settings and click on the Key Management link.<br /> 
                    4. A public key will be sent to the email address that you signed up with during the sandbox sign-up process.<br />
                    5. Then you should send the source code to Worldpay to pass the <a href=""https://www.worldpay.com/us/forms/get-certified"" target=""_blank"">plugin code certification</a>. 
                       After the certification you will get certified credentials that will replace the testing credentials you obtained when signing up for your sandbox and also unique Developer ID and Application Version.
                       You cannot go live with the application until Worldpay technical team has certified your application and you receive the confirmation letter with the credentials. 
                       This process is needed to ensure that you are using the Worldpay Total platform correctly and obtaining valid results.<br />
                       <em>Note: For testing purposes, use the sandbox and set your Developer Id to '12345678' and Application version to '1.2'</em><br />
                    6. Fill in the remaining fields and save to complete the configuration<br />
                    <br />
                    <em>Note: The Worldpay Total platform supports only USD currency, ensure that you have correctly configured exchange rate from your primary store currency to the USD currency.</em>
                    <br />
                </p>");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Worldpay.PaymentMethodDescription", "Pay by credit card using Worldpay");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<WorldpayPaymentSettings>();

            //locales
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Payments.Worldpay.Domain.TransactionMode.Authorize");
            this.DeletePluginLocaleResource("Enums.Nop.Plugin.Payments.Worldpay.Domain.TransactionMode.Charge");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.AdditionalFeePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.DeveloperId");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.DeveloperId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.DeveloperVersion");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.DeveloperVersion.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.PublicKey");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.PublicKey.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.SaveCard");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.SaveCard.Key");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.SecureKey");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.SecureKey.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.SecureNetId");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.SecureNetId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.StoredCard");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.StoredCard.Key");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.StoredCard.SelectCard");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.Token.Key");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.TransactionMode");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.TransactionMode.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.UseSandbox");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.UseSandbox.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.ValidateAddress");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Fields.ValidateAddress.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.Instructions");
            this.DeletePluginLocaleResource("Plugins.Payments.Worldpay.PaymentMethodDescription");

            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get { return RecurringPaymentType.Manual; }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public Nop.Services.Payments.PaymentMethodType PaymentMethodType
        {
            get { return Nop.Services.Payments.PaymentMethodType.Standard; }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayPal site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.Worldpay.PaymentMethodDescription"); }
        }

        #endregion
    }
}