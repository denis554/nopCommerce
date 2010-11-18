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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NopSolutions.NopCommerce.BusinessLogic.ExportImport;
using NopSolutions.NopCommerce.BusinessLogic.Products;
using NopSolutions.NopCommerce.BusinessLogic.Profile;
using NopSolutions.NopCommerce.BusinessLogic.Promo.Affiliates;
using NopSolutions.NopCommerce.Common.Utils;
using NopSolutions.NopCommerce.Web.Administration.Modules;
using NopSolutions.NopCommerce.BusinessLogic.Infrastructure;

namespace NopSolutions.NopCommerce.Web.Administration.Modules
{
    public partial class PricelistInfoControl : BaseNopAdministrationUserControl
    {
        private int CompareCultures(CultureInfo x, CultureInfo y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {

                    return x.IetfLanguageTag.CompareTo(y.IetfLanguageTag);
                }
            }
        }

        protected void BindData()
        {
            StringBuilder allowedTokensString = new StringBuilder();
            string[] allowedTokens = Pricelist.GetListOfAllowedTokens();
            for (int i = 0; i < allowedTokens.Length; i++)
            {
                string token = allowedTokens[i];
                allowedTokensString.Append(token);
                if (i != allowedTokens.Length - 1)
                    allowedTokensString.Append(", ");
            }
            this.lblAllowedTokens.Text = allowedTokensString.ToString();

            Pricelist pricelist = this.ProductService.GetPricelistById(this.PricelistId);
            if (pricelist != null)
            {
                this.txtAdminNotes.Text = pricelist.AdminNotes;
                this.txtBody.Text = pricelist.Body;
                this.txtCacheTime.Value = pricelist.CacheTime;
                this.txtDescription.Text = pricelist.Description;
                this.txtDisplayName.Text = pricelist.DisplayName;
                this.txtFooter.Text = pricelist.Footer;
                this.txtHeader.Text = pricelist.Header;
                this.txtPricelistGuid.Text = pricelist.PricelistGuid;
                this.txtShortName.Text = pricelist.ShortName;
                CommonHelper.SelectListItem(this.ddlExportMode, pricelist.ExportModeId);
                CommonHelper.SelectListItem(this.ddlExportType, pricelist.ExportTypeId);
                CommonHelper.SelectListItem(this.ddlPriceAdjustmentType, pricelist.PriceAdjustmentTypeId);
                CommonHelper.SelectListItem(this.ddlAffiliate, pricelist.AffiliateId);
                this.chkOverrideIndivAdjustment.Checked = pricelist.OverrideIndivAdjustment;
                this.txtPriceAdjustment.Value = pricelist.PriceAdjustment;
                this.ddlFormatLocalization.SelectedValue = pricelist.FormatLocalization;

                int totalRecords = 0;
                var productVariants = this.ProductService.GetAllProductVariants(0, 0, 
                    string.Empty, int.MaxValue, 0, out totalRecords);
                if (productVariants.Count > 0)
                {
                    gvProductVariants.DataSource = productVariants;
                    gvProductVariants.DataBind();
                }
            }
            else
            {
                ddlFormatLocalization.SelectedValue = System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag;

                int totalRecords = 0;
                var productVariants = this.ProductService.GetAllProductVariants(0, 0, 
                    string.Empty, int.MaxValue, 0, out totalRecords);
                if (productVariants.Count > 0)
                {
                    gvProductVariants.DataSource = productVariants;
                    gvProductVariants.DataBind();
                }
            }
        }

        protected void FillDropDowns()
        {
            CommonHelper.FillDropDownWithEnum(this.ddlExportMode, typeof(PriceListExportModeEnum));

            CommonHelper.FillDropDownWithEnum(this.ddlExportType, typeof(PriceListExportTypeEnum));

            CommonHelper.FillDropDownWithEnum(this.ddlPriceAdjustmentType, typeof(PriceAdjustmentTypeEnum));

            List<CultureInfo> cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList();
            cultures.Sort(CompareCultures);
            this.ddlFormatLocalization.Items.Clear();
            foreach (CultureInfo ci in cultures)
            {
                string name = string.Format("{0}. {1}", ci.IetfLanguageTag, ci.EnglishName);
                ListItem item2 = new ListItem(name, ci.IetfLanguageTag);
                this.ddlFormatLocalization.Items.Add(item2);
            }

            this.ddlAffiliate.Items.Clear();
            ListItem ddlAffiliateItem = new ListItem(GetLocaleResourceString("Admin.PricelistInfo.Affiliate.None"), "0");
            this.ddlAffiliate.Items.Add(ddlAffiliateItem);
            var affiliateCollection = this.AffiliateService.GetAllAffiliates();
            foreach (var affiliate in affiliateCollection)
            {
                ListItem ddlAffiliateItem2 = new ListItem(affiliate.LastName + " (ID=" + affiliate.AffiliateId.ToString() + ")", affiliate.AffiliateId.ToString());
                this.ddlAffiliate.Items.Add(ddlAffiliateItem2);
            }
        }

        private void TogglePanels()
        {
            PriceListExportModeEnum exportMode = (PriceListExportModeEnum)int.Parse(this.ddlExportMode.SelectedItem.Value);
            pnlProductVariants.Visible = exportMode == PriceListExportModeEnum.AssignedProducts;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                this.FillDropDowns();
                this.BindData();
                this.TogglePanels();
            }
        }

        protected void SavePricelistChanges(int priceListId)
        {
            foreach (GridViewRow objRow in gvProductVariants.Rows)
            {
                if (objRow.RowType == DataControlRowType.DataRow)
                {
                    CheckBox chkSelected = objRow.FindControl("chkSelected") as CheckBox;
                    DropDownList ddlPriceAdjustmentType = objRow.FindControl("ddlPriceAdjustmentType") as DropDownList;
                    DecimalTextBox txtPriceAdjustment = objRow.FindControl("txtPriceAdjustment") as DecimalTextBox;
                    HiddenField hfProductVariantPricelistId = objRow.FindControl("hfProductVariantPricelistId") as HiddenField;
                    HiddenField hfProductVariantId = objRow.FindControl("hfProductVariantId") as HiddenField;

                    int productVariantPricelistId = 0;
                    int.TryParse(hfProductVariantPricelistId.Value, out productVariantPricelistId);

                    ProductVariantPricelist productVariantPricelist = this.ProductService.GetProductVariantPricelistById(productVariantPricelistId);
                    if (chkSelected.Checked)
                    {
                        int productVariantId = 0;
                        int.TryParse(hfProductVariantId.Value, out productVariantId);

                        PriceAdjustmentTypeEnum priceAdjustmentType = (PriceAdjustmentTypeEnum)Enum.ToObject(typeof(PriceAdjustmentTypeEnum), int.Parse(ddlPriceAdjustmentType.SelectedItem.Value));
                        decimal priceAdjustment = txtPriceAdjustment.Value;

                        if (productVariantPricelist != null)
                        {
                            productVariantPricelist.ProductVariantId = productVariantId;
                            productVariantPricelist.PricelistId = priceListId;
                            productVariantPricelist.PriceAdjustmentTypeId = (int)priceAdjustmentType;
                            productVariantPricelist.PriceAdjustment = priceAdjustment;
                            productVariantPricelist.UpdatedOn = DateTime.UtcNow;
                            this.ProductService.UpdateProductVariantPricelist(productVariantPricelist);
                        }
                        else
                        {
                            productVariantPricelist = new ProductVariantPricelist();
                            productVariantPricelist.ProductVariantId = productVariantId;
                            productVariantPricelist.PricelistId = priceListId;
                            productVariantPricelist.PriceAdjustmentTypeId = (int)priceAdjustmentType;
                            productVariantPricelist.PriceAdjustment = priceAdjustment;
                            productVariantPricelist.UpdatedOn = DateTime.UtcNow;
                            this.ProductService.InsertProductVariantPricelist(productVariantPricelist);
                        }
                    }
                    else
                    {
                        if (productVariantPricelist != null)
                        {
                            this.ProductService.DeleteProductVariantPricelist(productVariantPricelistId);
                        }
                    }
                }
            }
        }

        protected void gvProductVariants_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ProductVariant productVariant = (ProductVariant)e.Row.DataItem;

                DropDownList ddlPriceAdjustmentType = e.Row.FindControl("ddlPriceAdjustmentType") as DropDownList;
                CheckBox chkSelected = e.Row.FindControl("chkSelected") as CheckBox;
                DecimalTextBox txtPriceAdjustment = e.Row.FindControl("txtPriceAdjustment") as DecimalTextBox;
                HiddenField hfProductVariantPricelistId = e.Row.FindControl("hfProductVariantPricelistId") as HiddenField;

                if (chkSelected != null && ddlPriceAdjustmentType != null && txtPriceAdjustment != null && hfProductVariantPricelistId != null)
                {
                    CommonHelper.FillDropDownWithEnum(ddlPriceAdjustmentType, typeof(PriceAdjustmentTypeEnum));

                    ProductVariantPricelist productVariantPricelist = this.ProductService.GetProductVariantPricelist(
                        productVariant.ProductVariantId, this.PricelistId);

                    if (productVariantPricelist != null)
                    {
                        chkSelected.Checked = true;
                        CommonHelper.SelectListItem(ddlPriceAdjustmentType, productVariantPricelist.PriceAdjustmentTypeId);
                        txtPriceAdjustment.Value = productVariantPricelist.PriceAdjustment;
                        hfProductVariantPricelistId.Value = productVariantPricelist.ProductVariantPricelistId.ToString();
                    }
                }
            }
        }

        protected void ddlExportMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            TogglePanels();
        }

        public Pricelist SaveInfo()
        {
            int affiliateId = int.Parse(ddlAffiliate.SelectedValue);

            PriceListExportModeEnum exportMode = (PriceListExportModeEnum)Enum.ToObject(typeof(PriceListExportModeEnum), int.Parse(this.ddlExportMode.SelectedItem.Value));
            PriceListExportTypeEnum exportType = (PriceListExportTypeEnum)Enum.ToObject(typeof(PriceListExportTypeEnum), int.Parse(this.ddlExportType.SelectedItem.Value));
            PriceAdjustmentTypeEnum priceAdjustmentType = (PriceAdjustmentTypeEnum)Enum.ToObject(typeof(PriceAdjustmentTypeEnum), int.Parse(this.ddlPriceAdjustmentType.SelectedItem.Value));
            decimal priceAdjustment = txtPriceAdjustment.Value;

            Pricelist pricelist = this.ProductService.GetPricelistById(this.PricelistId);
            if (pricelist != null)
            {
                pricelist.ExportModeId = (int)exportMode;
                pricelist.ExportTypeId = (int)exportType;
                pricelist.AffiliateId = affiliateId;
                pricelist.DisplayName = this.txtDisplayName.Text;
                pricelist.ShortName = this.txtShortName.Text;
                pricelist.PricelistGuid = this.txtPricelistGuid.Text;
                pricelist.CacheTime = this.txtCacheTime.Value;
                pricelist.FormatLocalization = this.ddlFormatLocalization.SelectedValue;
                pricelist.Description = this.txtDescription.Text;
                pricelist.AdminNotes = this.txtAdminNotes.Text;
                pricelist.Header = this.txtHeader.Text;
                pricelist.Body = this.txtBody.Text;
                pricelist.Footer = this.txtFooter.Text;
                pricelist.PriceAdjustmentTypeId = (int)priceAdjustmentType;
                pricelist.PriceAdjustment = priceAdjustment;
                pricelist.OverrideIndivAdjustment = chkOverrideIndivAdjustment.Checked;
                pricelist.UpdatedOn = DateTime.UtcNow;

                this.ProductService.UpdatePricelist(pricelist);

                SavePricelistChanges(pricelist.PricelistId);

            }
            else
            {
                pricelist = new Pricelist();
                pricelist.ExportModeId = (int)exportMode;
                pricelist.ExportTypeId = (int)exportType;
                pricelist.AffiliateId = affiliateId;
                pricelist.DisplayName = this.txtDisplayName.Text;
                pricelist.ShortName = this.txtShortName.Text;
                pricelist.PricelistGuid = this.txtPricelistGuid.Text;
                pricelist.CacheTime = this.txtCacheTime.Value;
                pricelist.FormatLocalization = this.ddlFormatLocalization.SelectedValue;
                pricelist.Description = this.txtDescription.Text;
                pricelist.AdminNotes = this.txtAdminNotes.Text;
                pricelist.Header = this.txtHeader.Text;
                pricelist.Body = this.txtBody.Text;
                pricelist.Footer = this.txtFooter.Text;
                pricelist.PriceAdjustmentTypeId = (int)priceAdjustmentType;
                pricelist.PriceAdjustment = priceAdjustment;
                pricelist.OverrideIndivAdjustment = chkOverrideIndivAdjustment.Checked;
                pricelist.CreatedOn = DateTime.UtcNow;
                pricelist.UpdatedOn = DateTime.UtcNow;

                this.ProductService.InsertPricelist(pricelist);

                SavePricelistChanges(pricelist.PricelistId);
            }

            return pricelist;
        }

        public int PricelistId
        {
            get
            {
                return CommonHelper.QueryStringInt("PricelistId");
            }
        }
    }
}