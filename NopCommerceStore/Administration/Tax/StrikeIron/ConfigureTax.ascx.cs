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
using NopSolutions.NopCommerce.BusinessLogic.Tax;
using NopSolutions.NopCommerce.Common.Utils;
using NopSolutions.NopCommerce.Tax;
using NopSolutions.NopCommerce.Web.Templates.Tax;
using NopSolutions.NopCommerce.BusinessLogic.Infrastructure;

namespace NopSolutions.NopCommerce.Web.Administration.Tax.StrikeIron
{
    public partial class ConfigureTax : BaseNopAdministrationUserControl, IConfigureTaxModule
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
                BindData();
        }

        private void BindData()
        {
            txtUserId.Text = IoC.Resolve<ISettingManager>().GetSettingValue("Tax.TaxProvider.StrikeIron.UserId");
            txtPassword.Text = IoC.Resolve<ISettingManager>().GetSettingValue("Tax.TaxProvider.StrikeIron.Password");
        }

        protected void btnTestUS_Click(object sender, EventArgs e)
        {
            try
            {
                StrikeIronTaxProvider strikeIronTaxProvider = new StrikeIronTaxProvider();
                string zip = txtZip_TestUSA.Text.Trim();
                string userId = txtUserId.Text.Trim();
                string password = txtPassword.Text.Trim();
                string error = string.Empty;
                decimal taxRate = strikeIronTaxProvider.GetTaxRateUSA(zip, userId, password, ref error);
                if (!String.IsNullOrEmpty(error))
                {
                    lblTestResultUSA.Text = error;
                }
                else
                {
                    lblTestResultUSA.Text = string.Format("Rate for zip {0}: {1}", zip, taxRate.ToString("p"));
                }
            }
            catch (Exception exc)
            {
                ProcessException(exc);
            }
        }

        protected void btnTestCA_Click(object sender, EventArgs e)
        {
            try
            {
                StrikeIronTaxProvider strikeIronTaxProvider = new StrikeIronTaxProvider();
                string province = txtProvince_TestCanada.Text.Trim();
                string userId = txtUserId.Text.Trim();
                string password = txtPassword.Text.Trim();
                string error = string.Empty;
                decimal taxRate = strikeIronTaxProvider.GetTaxRateCanada(province, 
                    userId, password, ref error);
                if (!String.IsNullOrEmpty(error))
                {
                    lblTestResultCanada.Text = error;
                }
                else
                {
                    lblTestResultCanada.Text = string.Format("Rate for province {0}: {1}", province, taxRate.ToString("p"));
                }
            }
            catch (Exception exc)
            {
                ProcessException(exc);
            }
        }

        public void Save()
        {
            IoC.Resolve<ISettingManager>().SetParam("Tax.TaxProvider.StrikeIron.UserId", txtUserId.Text);
            IoC.Resolve<ISettingManager>().SetParam("Tax.TaxProvider.StrikeIron.Password", txtPassword.Text);
        }
    }
}
