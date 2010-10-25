//------------------------------------------------------------------------------
// The contents of this file are subject to the nopCommerce Public License Version 1.0 ("License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at  http://www.nopCommerce.com/License.aspx. 
// 
// Software distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
// See the License for the specific language governing rights and limitations under the License.
// 
// The Original Code is nopCommerce.
// The Initial Developer of the Original Code is NopSolutions.
// All Rights Reserved.
// 
// Contributor(s): _______. 
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using NopSolutions.NopCommerce.BusinessLogic.Configuration.Settings;
using NopSolutions.NopCommerce.BusinessLogic.Payment;
using NopSolutions.NopCommerce.Common.Utils;
using NopSolutions.NopCommerce.Payment.Methods.Manual;
using NopSolutions.NopCommerce.Web.Templates.Payment;
using NopSolutions.NopCommerce.BusinessLogic.IoC;

namespace NopSolutions.NopCommerce.Web.Administration.Payment.SagePay
{
    public partial class ConfigurePaymentMethod : BaseNopAdministrationUserControl, IConfigurePaymentMethodModule
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BindData();
            }
        }

        private void BindData()
        {
            cbUseSandbox.Checked = IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("PaymentMethod.SagePay.UseSandbox", false);
            txtPartnerId.Text = IoCFactory.Resolve<ISettingManager>().GetSettingValue("PaymentMethod.SagePay.PartnerId");
            txtVendorName.Text = IoCFactory.Resolve<ISettingManager>().GetSettingValue("PaymentMethod.SagePay.VendorName");
            txtVendorDescription.Text = IoCFactory.Resolve<ISettingManager>().GetSettingValue("PaymentMethod.SagePay.VendorDescription");
            cbSendEmails.Checked = IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("PaymentMethod.SagePay.SendEmails", false);
            txtThanksMessage.Text = IoCFactory.Resolve<ISettingManager>().GetSettingValue("PaymentMethod.SagePay.EmailThanksMessage");
            cbApplyCVS.Checked = IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("PaymentMethod.SagePay.ApplyCVS", true);
            cbApply3DS.Checked = IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("PaymentMethod.SagePay.Apply3DS", true);
            txtEncyptionPassword.Text = IoCFactory.Resolve<ISettingManager>().GetSettingValue("PaymentMethod.SagePay.EncryptionPassword");
            txtProtocolNumber.Text = IoCFactory.Resolve<ISettingManager>().GetSettingValue("PaymentMethod.SagePay.ProtocolNumber", "2.23");
        }

        public void Save()
        {
            IoCFactory.Resolve<ISettingManager>().SetParam("PaymentMethod.SagePay.UseSandbox", cbUseSandbox.Checked.ToString());
            IoCFactory.Resolve<ISettingManager>().SetParam("PaymentMethod.SagePay.PartnerId", txtPartnerId.Text);
            IoCFactory.Resolve<ISettingManager>().SetParam("PaymentMethod.SagePay.VendorName", txtVendorName.Text);
            IoCFactory.Resolve<ISettingManager>().SetParam("PaymentMethod.SagePay.VendorDescription", txtVendorDescription.Text);
            IoCFactory.Resolve<ISettingManager>().SetParam("PaymentMethod.SagePay.SendEmails", cbSendEmails.Checked.ToString());
            IoCFactory.Resolve<ISettingManager>().SetParam("PaymentMethod.SagePay.EmailThanksMessage", txtThanksMessage.Text);
            IoCFactory.Resolve<ISettingManager>().SetParam("PaymentMethod.SagePay.ApplyCVS", cbApplyCVS.Checked.ToString());
            IoCFactory.Resolve<ISettingManager>().SetParam("PaymentMethod.SagePay.Apply3DS", cbApply3DS.Checked.ToString());
            IoCFactory.Resolve<ISettingManager>().SetParam("PaymentMethod.SagePay.EncryptionPassword", txtEncyptionPassword.Text);
            IoCFactory.Resolve<ISettingManager>().SetParam("PaymentMethod.SagePay.ProtocolNumber", txtProtocolNumber.Text);
        }
    }
}
