<%@ Page Language="C#" MasterPageFile="~/MasterPages/OneColumn.master" AutoEventWireup="true"
    Inherits="NopSolutions.NopCommerce.Web.NewsArchivePage" CodeBehind="NewsArchive.aspx.cs"
    ValidateRequest="false" %>

<%@ Register TagPrefix="nopCommerce" TagName="NewsArchive" Src="~/Modules/NewsArchive.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cph1" runat="Server">
    <nopCommerce:NewsArchive ID="ctrlNewsArchive" runat="server" />
</asp:Content>
