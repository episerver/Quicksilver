<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Menu.Tabs.OverviewEditTab" Codebehind="OverviewMenuItemEditTab.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/BooleanEditControl.ascx" TagName="BooleanEditControl" TagPrefix="ecf" %>
<div id="DataForm">
    <table class="DataForm" width="650">
        <tr>
            <td class="FormLabelCell"><asp:Label ID="Literal5" runat="server" Text='<%$ Resources:SharedStrings, Title %>'></asp:Label> <asp:Label runat="server" ID="NameLanguageCode"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="250" ID="Name"></asp:TextBox>
                <asp:RequiredFieldValidator runat="server" ID="NameValidation" ControlToValidate="Name" 
                    ErrorMessage="<%$ Resources:ContentStrings, Menu_Item_Name_Required %>" Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:CustomValidator runat="server" ID="NameCheckCustomValidator" ControlToValidate="Name" OnServerValidate="NameCheck" Display="Dynamic" ErrorMessage="<%$ Resources:ContentStrings, MenuItem_With_Name_Exists %>" />
                <br />
                <asp:Label ID="Label4" CssClass="FormFieldDescription" runat="server"
                    Text="<%$ Resources:ContentStrings, Menu_Item_Name_Instructions %>"></asp:Label>
            </td>
        </tr>
        <asp:PlaceHolder runat="server" ID="MenuItemHolder">
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>       
        <tr>
            <td class="FormLabelCell" nowrap><asp:Label ID="Literal1" runat="server" Text='<%$ Resources:SharedStrings, Tooltip %>'></asp:Label> <asp:Label runat="server" ID="TooltipLanguageCode"></asp:Label>:</td>
            <td class="FormFieldCell"><asp:TextBox Width="250" runat="server" ID="ToolTip"></asp:TextBox><br />
                <asp:Label ID="Label3" CssClass="FormFieldDescription" runat="server"
                    Text="<%$ Resources:ContentStrings, Menu_Item_Tooltip_Instructions %>"></asp:Label>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell"><asp:Label ID="Literal3" runat="server" Text='<%$ Resources:SharedStrings, Active %>'></asp:Label>:</td>
            <td class="FormFieldCell"><ecf:BooleanEditControl id="IsVisible" runat="server"></ecf:BooleanEditControl></td>
        </tr>
        <tr>  
            <td colspan="2" class="FormSpacerCell"></td> 
        </tr>
        <tr>
            <td class="FormLabelCell"><asp:Label ID="Label11" runat="server" Text="Sort Order:"></asp:Label></td> 
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="50" ID="SortOrder" Text="0"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="SortOrder" 
                    ErrorMessage="*" Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:RangeValidator ID="RangeValidator1" runat="server" Type="Integer" ControlToValidate="SortOrder" 
                    MinimumValue="-2147483648" MaximumValue="2147483647" 
                    ErrorMessage="<%$ Resources:ContentStrings, Menu_Item_Sort_Order_Invalid %>" Display="Dynamic"></asp:RangeValidator>
                <br />
                <asp:Label ID="Label5" CssClass="FormFieldDescription" runat="server" 
                    Text="<%$ Resources:ContentStrings, Menu_Item_Sort_Order_Description %>"></asp:Label>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell"><asp:Label ID="Label1" runat="server" Text='<%$ Resources:ContentStrings, Menu_Command_Type %>'></asp:Label>:</td>
            <td class="FormFieldCell">
                    <asp:RadioButton runat="server" ID="rbNone" GroupName="CommandType" onclick=<%#"CSContentClient.EditMenuCommand(0,'" + this.ClientID + "');" %>
                        Text="<%$ Resources:ContentStrings, Menu_Command_None %>" />
                    <asp:RadioButton runat="server" ID="rbUrl" GroupName="CommandType" onclick=<%#"CSContentClient.EditMenuCommand(1,'" + this.ClientID + "');" %>
                        Text="<%$ Resources:ContentStrings, Menu_Command_Link %>" />
                    <asp:RadioButton runat="server" ID="rbScript" GroupName="CommandType" onclick=<%#"CSContentClient.EditMenuCommand(2,'" + this.ClientID + "');" %>
                        Text="<%$ Resources:ContentStrings, Menu_Command_Script %>" />
                    <asp:RadioButton runat="Server" ID="rbNavigation" GroupName="CommandType" onclick=<%#"CSContentClient.EditMenuCommand(3,'" + this.ClientID + "');" %>
                        Text="<%$ Resources:ContentStrings, Menu_Command_Navigation %>"/>           
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>           
        <tr id="trCommand">
            <td class="FormLabelCell">
                <div runat="server" id="CommandTitle" name="CommandTitle" style="display: none;">
                    <asp:Literal ID="Literal9" runat="server" Text="<%$ Resources:ContentStrings, Menu_Command %>" />:</div>
                <div runat="server" id="NavigationTitle" name="NavigationTitle" style="display:none;"> 
                    <asp:Literal ID="Literal10" runat="server" Text="<%$ Resources:ContentStrings, Menu_Command_Navigation %>" />: <br />
                    <div style="height: 3px;">&nbsp</div>
                    <asp:Literal ID="Literal11" runat="server" Text="<%$ Resources:ContentStrings, Menu_Command_Parameters %>" />: <br />
                </div>
            </td>
            <td class="FormLabelCell">
                <div runat="server" id="CommandText" name="CommandText" style="display: none;">
                    <asp:TextBox runat="server" ID="tbCommand" TextMode="MultiLine" Rows="10" Width="350"></asp:TextBox></div>
                <div runat="server" id="NavigationText" name="NavigationText" style="display:none;">
                    <asp:DropDownList runat="server" ID="ddNavigationItems" OnSelectedIndexChanged="ddNavigationItems_SelectedIndexChanged" AutoPostBack="true"/> <br />
                    <div style="padding-top: 3px;" runat="server" id="divParams"></div> <br />
                    <asp:Literal ID="Literal12" runat="server" Text="<%$ Resources:ContentStrings, Menu_Command_Parameters_Description %>" />: <br />
                    <asp:TextBox Width="80%" runat="Server" ID="txtValues"></asp:TextBox>
                </div>
            </td>
            <td>
            </td>
        </tr>        
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell"><asp:Label ID="Label2" runat="server" Text='<%$ Resources:SharedStrings, Parent %>'></asp:Label>:</td>
            <td class="FormFieldCell"><asp:DropDownList runat="server" ID="ParentMenuItem"></asp:DropDownList></td>
        </tr>
        </asp:PlaceHolder>
    </table>
</div>
