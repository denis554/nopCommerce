﻿using System.Collections.Generic;
using System.Linq;
using Moq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Services.Tests.Orders
{
    [TestFixture]
    public class CheckoutAttributeParserAndFormatterTests : ServiceTest
    {
        private Mock<IRepository<CheckoutAttribute>> _checkoutAttributeRepo;
        private Mock<IRepository<CheckoutAttributeValue>> _checkoutAttributeValueRepo;
        private Mock<IEventPublisher> _eventPublisher;
        private Mock<IStoreMappingService> _storeMappingService;
        private ICheckoutAttributeService _checkoutAttributeService;
        private ICheckoutAttributeParser _checkoutAttributeParser;
        private Mock<IWorkContext> _workContext;
        private Mock<ICurrencyService> _currencyService;
        private Mock<ITaxService> _taxService;
        private Mock<IPriceFormatter> _priceFormatter;
        private Mock<IDownloadService> _downloadService;
        private Mock<IWebHelper> _webHelper;
        private ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private Mock<IProductAttributeParser> _productAttributeParser;
        private Mock<IProductService> _productService;

        private CheckoutAttribute ca1, ca2, ca3;
        private CheckoutAttributeValue cav1_1, cav1_2, cav2_1, cav2_2;
        
        [SetUp]
        public new void SetUp()
        {
            #region Test data

            //color (dropdownlist)
            ca1 = new CheckoutAttribute
            {
                Id = 1,
                Name= "Color",
                TextPrompt = "Select color:",
                IsRequired = true,
                AttributeControlType = AttributeControlType.DropdownList,
                DisplayOrder = 1
            };
            cav1_1 = new CheckoutAttributeValue
            {
                Id = 11,
                Name = "Green",
                DisplayOrder = 1,
                CheckoutAttribute = ca1,
                CheckoutAttributeId = ca1.Id
            };
            cav1_2 = new CheckoutAttributeValue
            {
                Id = 12,
                Name = "Red",
                DisplayOrder = 2,
                CheckoutAttribute = ca1,
                CheckoutAttributeId = ca1.Id
            };
            ca1.CheckoutAttributeValues.Add(cav1_1);
            ca1.CheckoutAttributeValues.Add(cav1_2);

            //custom option (checkboxes)
            ca2 = new CheckoutAttribute
            {
                Id = 2,
                Name = "Custom option",
                TextPrompt = "Select custom option:",
                IsRequired = true,
                AttributeControlType = AttributeControlType.Checkboxes,
                DisplayOrder = 2
            };
            cav2_1 = new CheckoutAttributeValue
            {
                Id = 21,
                Name = "Option 1",
                DisplayOrder = 1,
                CheckoutAttribute = ca2,
                CheckoutAttributeId = ca2.Id
            };
            cav2_2 = new CheckoutAttributeValue
            {
                Id = 22,
                Name = "Option 2",
                DisplayOrder = 2,
                CheckoutAttribute = ca2,
                CheckoutAttributeId = ca2.Id
            };
            ca2.CheckoutAttributeValues.Add(cav2_1);
            ca2.CheckoutAttributeValues.Add(cav2_2);

            //custom text
            ca3 = new CheckoutAttribute
            {
                Id = 3,
                Name = "Custom text",
                TextPrompt = "Enter custom text:",
                IsRequired = true,
                AttributeControlType = AttributeControlType.MultilineTextbox,
                DisplayOrder = 3
            };


            #endregion
            
            _checkoutAttributeRepo = new Mock<IRepository<CheckoutAttribute>>();
            _checkoutAttributeRepo.Setup(x => x.Table).Returns(new List<CheckoutAttribute> { ca1, ca2, ca3 }.AsQueryable());
            _checkoutAttributeRepo.Setup(x => x.GetById(ca1.Id)).Returns(ca1);
            _checkoutAttributeRepo.Setup(x => x.GetById(ca2.Id)).Returns(ca2);
            _checkoutAttributeRepo.Setup(x => x.GetById(ca3.Id)).Returns(ca3);

            _checkoutAttributeValueRepo = new Mock<IRepository<CheckoutAttributeValue>>();
            _checkoutAttributeValueRepo.Setup(x => x.Table).Returns(new List<CheckoutAttributeValue> { cav1_1, cav1_2, cav2_1, cav2_2 }.AsQueryable());
            _checkoutAttributeValueRepo.Setup(x => x.GetById(cav1_1.Id)).Returns(cav1_1);
            _checkoutAttributeValueRepo.Setup(x => x.GetById(cav1_2.Id)).Returns(cav1_2);
            _checkoutAttributeValueRepo.Setup(x => x.GetById(cav2_1.Id)).Returns(cav2_1);
            _checkoutAttributeValueRepo.Setup(x => x.GetById(cav2_2.Id)).Returns(cav2_2);

            var cacheManager = new NopNullCache();

            _storeMappingService = new Mock<IStoreMappingService>();

            _eventPublisher = new Mock<IEventPublisher>();
            _eventPublisher.Setup(x => x.Publish(It.IsAny<object>()));

            _checkoutAttributeService = new CheckoutAttributeService(cacheManager,
                _checkoutAttributeRepo.Object,
                _checkoutAttributeValueRepo.Object,
                _storeMappingService.Object,
                _eventPublisher.Object);

            _productService = new Mock<IProductService>();
            _productAttributeParser = new Mock<IProductAttributeParser>();
            _checkoutAttributeParser = new CheckoutAttributeParser(_checkoutAttributeService, _productAttributeParser.Object, _productService.Object);

            var workingLanguage = new Language();
            _workContext = new Mock<IWorkContext>();
            _workContext.Setup(x => x.WorkingLanguage).Returns(workingLanguage);
            _currencyService = new Mock<ICurrencyService>();
            _taxService = new Mock<ITaxService>();
            _priceFormatter = new Mock<IPriceFormatter>();
            _downloadService = new Mock<IDownloadService>();
            _webHelper = new Mock<IWebHelper>();

            _checkoutAttributeFormatter = new CheckoutAttributeFormatter(_workContext.Object,
                _checkoutAttributeService,
                _checkoutAttributeParser,
                _currencyService.Object,
                _taxService.Object,
                _priceFormatter.Object,
                _downloadService.Object,
                _webHelper.Object);
        }
        
        [Test]
        public void Can_add_and_parse_checkoutAttributes()
        {
            var attributes = "";
            //color: green
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca1, cav1_1.Id.ToString());
            //custom option: option 1, option 2
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca2, cav2_1.Id.ToString());
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca2, cav2_2.Id.ToString());
            //custom text
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca3, "Some custom text goes here");

            var parsed_attributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(attributes);
            parsed_attributeValues.Contains(cav1_1).ShouldEqual(true);
            parsed_attributeValues.Contains(cav1_2).ShouldEqual(false);
            parsed_attributeValues.Contains(cav2_1).ShouldEqual(true);
            parsed_attributeValues.Contains(cav2_2).ShouldEqual(true);
            parsed_attributeValues.Contains(cav2_2).ShouldEqual(true);

            var parsedValues = _checkoutAttributeParser.ParseValues(attributes, ca3.Id);
            parsedValues.Count.ShouldEqual(1);
            parsedValues.Contains("Some custom text goes here").ShouldEqual(true);
            parsedValues.Contains("Some other custom text").ShouldEqual(false);
        }

        [Test]
        public void Can_add_render_attributes_withoutPrices()
        {
            var attributes = "";
            //color: green
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca1, cav1_1.Id.ToString());
            //custom option: option 1, option 2
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca2, cav2_1.Id.ToString());
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca2, cav2_2.Id.ToString());
            //custom text
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca3, "Some custom text goes here");


            var customer = new Customer();
            var formattedAttributes = _checkoutAttributeFormatter.FormatAttributes(attributes, customer, "<br />", false, false);
            formattedAttributes.ShouldEqual("Color: Green<br />Custom option: Option 1<br />Custom option: Option 2<br />Custom text: Some custom text goes here");
        }

        [Test]
        public void Can_add_and_remove_checkoutAttributes()
        {
            var attributes = "";
            //color: green
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca1, cav1_1.Id.ToString());
            //custom option: option 1, option 2
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca2, cav2_1.Id.ToString());
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca2, cav2_2.Id.ToString());
            //custom text
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca3, "Some custom text goes here");
            //delete some of them
            attributes = _checkoutAttributeParser.RemoveCheckoutAttribute(attributes, ca2);
            attributes = _checkoutAttributeParser.RemoveCheckoutAttribute(attributes, ca3);

            var parsed_attributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(attributes);
            parsed_attributeValues.Contains(cav1_1).ShouldEqual(true);
            parsed_attributeValues.Contains(cav1_2).ShouldEqual(false);
            parsed_attributeValues.Contains(cav2_1).ShouldEqual(false);
            parsed_attributeValues.Contains(cav2_2).ShouldEqual(false);
            parsed_attributeValues.Contains(cav2_2).ShouldEqual(false);

            var parsedValues = _checkoutAttributeParser.ParseValues(attributes, ca3.Id);
            parsedValues.Count.ShouldEqual(0);
        }
    }
}
