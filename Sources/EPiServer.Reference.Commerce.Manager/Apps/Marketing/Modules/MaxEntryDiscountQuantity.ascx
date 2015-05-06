<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MaxEntryDiscountQuantity.ascx.cs" Inherits="Mediachase.Commerce.Manager.Apps.Marketing.Modules.MaxEntryDiscountQuantity" %>

<asp:TextBox runat="server" ID="MaxEntryDiscountQuantity1"></asp:TextBox>
<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="MaxEntryDiscountQuantity1"
    Display="Dynamic" ErrorMessage="*"></asp:RequiredFieldValidator>
<asp:RegularExpressionValidator ID="RegularExpressionValidator" runat="server" ControlToValidate="MaxEntryDiscountQuantity1"
    Display="Dynamic" ErrorMessage="*" ValidationExpression="(\d*\.?\d*)"></asp:RegularExpressionValidator>
<asp:RangeValidator ID="RangeValidator2" runat="server" ControlToValidate="MaxEntryDiscountQuantity1"
    Display="Dynamic" ErrorMessage="*" Type="Currency" MinimumValue="1" MaximumValue="100000000"></asp:RangeValidator>