<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigControl.ascx.cs"
    Inherits="Apps_Marketing_Promotions_BuyXGetNofYatReducedPrice" %>
<%@ Register Src="../../Modules/EntryFilter.ascx" TagName="EntryFilter" TagPrefix="uc1" %>
<div id="DataForm">
    <asp:HiddenField runat="server" ID="SelectedEntries" />
    <table class="DataForm">
        <tr>
            <td colspan="2" class="FormFieldCell">
                <b><i>
                    <asp:Label runat="server" ID="Description"></asp:Label></i></b><br />
                <br />
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label13" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Entry_X_Variation %>"></asp:Label>:
            </td>
            <td class="FormFieldCell">
                <table>
                    <tr>
                        <td>
                            <asp:LinkButton runat="server" ID="lbEntryListX" CausesValidation="false"></asp:LinkButton>
                        </td>
                        <td>
                            <asp:CheckBox ID="ExcludeEntry" runat="server" AutoPostBack="false" Text="<%$ Resources:SharedStrings, Exclude %>"
                                TextAlign="Right" />
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label15" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Select_Catalog_Entry_Y %>"></asp:Label>:
            </td>
            <td class="FormFieldCell">
                <uc1:EntryFilter ID="EntryYFilter" runat="server" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label11" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Max_Quantity_Y %>"></asp:Label>:
            </td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="MaxQuantity"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="MaxQuantity"
                    Display="Dynamic" ErrorMessage="*"></asp:RequiredFieldValidator>
                <asp:RangeValidator ID="RangeValidator2" runat="server" ControlToValidate="MaxQuantity"
                    Display="Dynamic" ErrorMessage="*" Type="Currency" MinimumValue="0" MaximumValue="100000000"></asp:RangeValidator>
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
                    Display="Dynamic" ErrorMessage="*" Type="Currency" MinimumValue="-100000000"
                    MaximumValue="100000000"></asp:RangeValidator>
                <asp:DropDownList runat="server" ID="OfferType">
                    <asp:ListItem Text="<%$ Resources:MarketingStrings, Promotion_Percentage_Based %>"
                        Value="1"></asp:ListItem>
                    <asp:ListItem Text="<%$ Resources:MarketingStrings, Promotion_Value_Based %>" Value="2"></asp:ListItem>
                </asp:DropDownList>
            </td>
        </tr>
    </table>
</div>
