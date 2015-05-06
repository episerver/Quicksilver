<%@ Control Language="C#" AutoEventWireup="true" 
CodeBehind="ConfigControl.ascx.cs" Inherits="Apps_Marketing_Order_Volume_Discount_ConfigControl" %>
<table class="DataForm">
    <tr>
        <td colspan="2" class="FormFieldCell">
                <b><i><asp:Label runat="server" ID="Description"></asp:Label></i></b><br /><br />
        </td>
    </tr>
    <tr>
        <td colspan="2" class="FormSpacerCell">
        </td>
    </tr>
    <tr>
        <td colspan="2" class="FormSpacerCell">
        </td>
    </tr>    
    <tr>
        <td class="FormLabelCell">
            <asp:Label ID="Label4" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Minimum_Order_Amount %>"></asp:Label>:</td>
        <td class="FormFieldCell">
            <asp:TextBox runat="server" ID="MinOrderAmount"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="MinOrderAmount" Display="Dynamic" ErrorMessage="*"></asp:RequiredFieldValidator>
            <asp:RangeValidator ID="RangeValidator2" runat="server" ControlToValidate="MinOrderAmount" Display="Dynamic" ErrorMessage="*" Type="Currency" MinimumValue="0" MaximumValue="100000000"></asp:RangeValidator>
        </td>
    </tr>    
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label9" runat="server" Text="<%$ Resources:SharedStrings, Amount %>"></asp:Label>:</td>
            <td class="FormFieldCell" valign="top">
                <asp:TextBox runat="server" ID="OfferAmount"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="OfferAmount" Display="Dynamic" ErrorMessage="*"></asp:RequiredFieldValidator>
                <asp:RangeValidator ID="RangeValidator3" runat="server" ControlToValidate="OfferAmount" Display="Dynamic" ErrorMessage="*" Type="Currency" MinimumValue="-100000000" MaximumValue="100000000"></asp:RangeValidator>
                <asp:DropDownList runat="server" ID="OfferType">
                    <asp:ListItem Text="<%$ Resources:MarketingStrings, Promotion_Percentage_Based %>" Value="1"></asp:ListItem>
                    <asp:ListItem Text="<%$ Resources:MarketingStrings, Promotion_Value_Based %>" Value="2"></asp:ListItem>
                </asp:DropDownList>
            </td>
        </tr>           
</table>

