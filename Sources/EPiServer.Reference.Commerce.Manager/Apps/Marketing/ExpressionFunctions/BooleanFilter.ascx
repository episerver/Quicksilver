<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BooleanFilter.ascx.cs" Inherits="Mediachase.Commerce.Manager.Apps.Marketing.ExpressionFunctions.BooleanFilter" %>
<div runat="server" id="container" style="display: inline;" changeVisibility="1">
	<asp:DropDownList runat="server" ID="ddlValue" CssClass="dropLabelText"/>
</div><asp:Label runat="server" ID="lblText" CssClass="dropLabel dropLabelText" />
<asp:Label runat="server" ID="lblError" CssClass="ErrorRed" />