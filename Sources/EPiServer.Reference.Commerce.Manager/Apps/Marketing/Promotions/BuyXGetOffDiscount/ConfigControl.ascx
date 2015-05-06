<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigControl.ascx.cs"
    Inherits="Apps_Marketing_Promotions_BuyXGetOffDiscount" %>
<%@ Register Src="../../Modules/MaxEntryDiscountQuantity.ascx" TagName="MaxEntryDiscountQuantity" TagPrefix="uc1" %>
<div id="DataForm">
    <table class="DataForm">
        <tr>
            <td colspan="2" class="FormFieldCell">
                <b><i><asp:Label runat="server" ID="Description"></asp:Label></i></b><br /><br />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label11" runat="server" Text="<%$ Resources:SharedStrings, Minimum_Quantity %>"></asp:Label>:
            </td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="MinQuantity"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="MinQuantity"
                    Display="Dynamic" ErrorMessage="*"></asp:RequiredFieldValidator>
                <asp:RangeValidator ID="RangeValidator2" runat="server" ControlToValidate="MinQuantity"
                    Display="Dynamic" ErrorMessage="<%$ Resources:MarketingStrings, Promotion_Range_GT_One %>" Type="Integer" MinimumValue="1" MaximumValue="100000000"></asp:RangeValidator>
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label2" runat="server" Text="<%$ Resources:MarketingStrings, MaxEntryDiscountQuantity %>"></asp:Label>
            </td>
            <td class="FormFieldCell">
                <uc1:MaxEntryDiscountQuantity runat="server" ID="MaxEntryDiscountQuantity1"></uc1:MaxEntryDiscountQuantity>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label12" runat="server" Text="<%$ Resources:SharedStrings, Amount %>"></asp:Label>:
            </td>
            <td class="FormFieldCell" valign="top">
                <asp:TextBox runat="server" ID="OfferAmount"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="OfferAmount"
                    Display="Dynamic" ErrorMessage="*"></asp:RequiredFieldValidator>
                <asp:RangeValidator ID="RangeValidator3" runat="server" ControlToValidate="OfferAmount"
                    Display="Dynamic" ErrorMessage="* >=0" Type="Currency" MinimumValue="0"
                    MaximumValue="100000000"></asp:RangeValidator>
                <asp:DropDownList runat="server" ID="OfferType">
                    <asp:ListItem Text="<%$ Resources:MarketingStrings, Promotion_Percentage_Based %>" Value="1"></asp:ListItem>
                    <asp:ListItem Text="<%$ Resources:MarketingStrings, Promotion_Value_Based %>" Value="2"></asp:ListItem>
                </asp:DropDownList>                    
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label13" runat="server" Text="<%$ Resources:SharedStrings, Variations %>"></asp:Label>:
            </td>
            <td class="FormFieldCell">
                <asp:LinkButton runat="server" ID="lbEntryList" CausesValidation="false"></asp:LinkButton>
            </td>
        </tr>
    </table>
</div>
