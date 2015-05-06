<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OrderLinkTemplate.ascx.cs"
	Inherits="Mediachase.Commerce.Manager.Apps.Order.GridTemplates.OrderLinkTemplate" %>
<%@ Import Namespace="Mediachase.Web.Console.Common" %>
<%@ Import Namespace="Mediachase.Web.Console.Controls" %>
<asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl='<%# String.Format("javascript:CSOrderClient.ViewOrder2(\"{0}\", {1},\"{2}\");", GetViewName(), DataBinder.Eval(DataItem,"OrderGroupId"), DataBinder.Eval(DataItem,"CustomerId")) %>'> </asp:HyperLink>
<%--<div runat="server" visible="false" id="parentOrderDiv">
	(<asp:HyperLink ID="HyperLink2" runat="server"/>)
</div>
--%>