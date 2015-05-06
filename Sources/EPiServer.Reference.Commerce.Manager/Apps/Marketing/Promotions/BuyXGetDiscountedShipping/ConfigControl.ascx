<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="Apps_Marketing_Promotions_BuyXGetDiscountedShipipng" Codebehind="ConfigControl.ascx.cs" %>
<%@ Register Src="../../Modules/EntryFilter.ascx" TagName="EntryFilter" TagPrefix="uc1" %>
<div id="DataForm">
    <asp:HiddenField runat="server" ID="SelectedEntries" />
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
            <td class="FormLabelCell">
                <asp:Label ID="Label3" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Select_Catalog_Entry_X %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <uc1:EntryFilter ID="EntryXFilter" IsFieldRequired="false" runat="server" />
                <asp:LinkButton runat="server" ID="AddEntry" Text="<%$ Resources:MarketingStrings, Promotion_Add_Variation %>" CausesValidation="false"></asp:LinkButton>
                <br />
                <asp:DataList runat="server" ID="EntryList">
                    <ItemTemplate>
                        <asp:ImageButton CausesValidation="false" runat="server" ID="DeleteButton" OnCommand="DeleteButton_Command"
                            ImageUrl="../../images/delete.png" CommandArgument='<%#Container.DataItem%>' />
                        <%#GetCatalogEntryName(Container.DataItem)%>
                    </ItemTemplate>
                </asp:DataList>
            </td>
        </tr>
            <tr>
                <td class="FormLabelCell">
                    <asp:Label ID="Label11" runat="server" Text="Minimum Quantity For Any Of The Selected Entries"></asp:Label>:
                </td>
                <td class="FormFieldCell">
                    <asp:TextBox runat="server" ID="MinQuantity"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="MinQuantity"
                        Display="Dynamic" ErrorMessage="*"></asp:RequiredFieldValidator>
                    <asp:CompareValidator ID="CompareValidator1" runat="server" ValueToCompare="0" ControlToValidate="MinQuantity" 
	                    ErrorMessage="Must enter a positive decimal value" Operator="GreaterThan" Type="Double"></asp:CompareValidator> 
                </td>
            </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <span>Shipping Method: </span></td>
            <td class="FormFieldCell">
                <asp:DropDownList ID="ddlShippingMethods" runat="server" DataTextField="DisplayName" DataValueField="ShippingMethodId"></asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>    
        <tr>
            <td class="FormLabelCell">
                <span>Discount Amount: </span></td>
            <td class="FormFieldCell">
                <asp:TextBox ID="txtDiscount" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rqvDiscount" runat="server" ControlToValidate="txtDiscount" ErrorMessage="*"></asp:RequiredFieldValidator>
                <asp:CompareValidator ID="cvDiscount" runat="server" ValueToCompare="0" ControlToValidate="txtDiscount" 
	                ErrorMessage="Must enter a positive decimal value" Operator="GreaterThan" Type="Double"></asp:CompareValidator> 
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>    
        <tr>
            <td class="FormLabelCell">
                <span>Discount Type: </span></td>
            <td class="FormFieldCell">
                <asp:DropDownList ID="ddlDiscountType" runat="server">
                    <asp:ListItem Value="0">Percentage</asp:ListItem>
                    <asp:ListItem Value="1">Value</asp:ListItem>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>    
    </table>
</div>