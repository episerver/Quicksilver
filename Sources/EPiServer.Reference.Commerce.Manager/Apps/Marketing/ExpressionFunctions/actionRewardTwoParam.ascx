<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="actionRewardTwoParam.ascx.cs" Inherits="Mediachase.Commerce.Manager.Marketing.ExpressionFunctions.Count" %>
<div runat="server" id="containerAmount" style="display: inline;" changeVisibility="1">
	<asp:TextBox runat="server" ID="tbAmount" Width = "40" CssClass="dropLabelText" />
</div><asp:Label runat="server" ID="lblAmountText" CssClass="dropLabel dropLabelText" />
<asp:Label runat="server" ID="lblAmountError" CssClass="ErrorRed" />

<asp:Label runat="server" ID = "Label2" Text= " on quantity " CssClass="dropLabel dropLabelText"/>

<div runat="server" id="containerQuantity" style="display: inline;" changeVisibility="1">
	<asp:TextBox runat="server" ID="tbQuantity"  Width = "30" CssClass="dropLabelText" />
</div><asp:Label runat="server" ID="lblQuantityText"  CssClass="dropLabel dropLabelText" />
<asp:Label runat="server" ID="lblQuantityError" CssClass="ErrorRed" />
