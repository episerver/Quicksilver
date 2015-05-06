<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigControl.ascx.cs"
    Inherits="Apps_Marketing_Promotions_SpendXGetFreeStandardShippingDiscount" %>

<div id="DataForm">
    <asp:HiddenField runat="server" ID="SelectedEntries" />
    <table class="DataForm">
        <tr>
            <td colspan="2" class="FormFieldCell">
                <b><i><asp:Label runat="server" ID="Description"></asp:Label></i></b><br /><br />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label11" runat="server" Text="Minimum Order Amount"></asp:Label>:
            </td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="tbMinAmount"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="tbMinAmount"
                    Display="Dynamic" ErrorMessage="*"></asp:RequiredFieldValidator>
                <asp:RangeValidator ID="RangeValidator2" runat="server" ControlToValidate="tbMinAmount"
                    Display="Dynamic" ErrorMessage="*" Type="Currency" MinimumValue="0" MaximumValue="100000000"></asp:RangeValidator>
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
                <asp:TextBox runat="server" ID="tbOfferAmount"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="tbOfferAmount"
                    Display="Dynamic" ErrorMessage="*"></asp:RequiredFieldValidator>
                <asp:RangeValidator ID="RangeValidator3" runat="server" ControlToValidate="tbOfferAmount"
                    Display="Dynamic" ErrorMessage="*" Type="Currency" MinimumValue="-100000000"
                    MaximumValue="100000000"></asp:RangeValidator>
                <asp:DropDownList runat="server" ID="OfferType">
                    <asp:ListItem Text="<%$ Resources:MarketingStrings, Promotion_Percentage_Based %>" Value="1"></asp:ListItem>
                    <asp:ListItem Text="<%$ Resources:MarketingStrings, Promotion_Value_Based %>" Value="2"></asp:ListItem>
                </asp:DropDownList>
                <br />
                <asp:Label runat="server" CssClass="FormFieldDescription" Text="enter whole number for percentage (example: 30 = 30%)" ></asp:Label>                  
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
    </table>
</div>
