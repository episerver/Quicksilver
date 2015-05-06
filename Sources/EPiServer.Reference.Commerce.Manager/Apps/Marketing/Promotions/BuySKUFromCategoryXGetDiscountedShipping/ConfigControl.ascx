<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="Apps_Marketing_Promotions_BuySKUFromCategoryXGetDiscountedShipping" Codebehind="ConfigControl.ascx.cs" %>
<%@ Register Src="../../Modules/CategoryFilter.ascx" TagName="CategoryFilter" TagPrefix="uc1" %>
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
                <asp:Label ID="Label3" runat="server" Text="Select Category Of Products"></asp:Label>:</td>
            <td class="FormFieldCell">
                <uc1:CategoryFilter ID="CategoryXFilter" IsFieldRequired="false" runat="server" />
            </td>
        </tr>
            <tr>
                <td class="FormLabelCell">
                    <asp:Label ID="Label11" runat="server" Text="Total Minimum Quantity Of SKUs From This Category: "></asp:Label>
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