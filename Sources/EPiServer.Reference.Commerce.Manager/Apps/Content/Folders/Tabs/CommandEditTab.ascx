<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Folders.Tabs.CommandEditTab" Codebehind="CommandEditTab.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/HtmlEditControl.ascx" TagName="HtmlEditControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/BooleanEditControl.ascx" TagName="BooleanEditControl" TagPrefix="ecf" %>
<div id="DataForm">
    <table>
        <tr>
            <td class="FormLabelCell"><asp:Label ID="Literal5" runat="server" Text='<%$ Resources:ContentStrings, Folder_Command_QueryString %>'></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="250" ID="tbQueryString"></asp:TextBox>
            </td>
        </tr>
        <tr id="RolesRow" runat="server">
            <td class="FormLabelCell" nowrap><asp:Label ID="Literal1" runat="server" Text='<%$ Resources:SharedStrings, Name %>'></asp:Label>:</td>
            <td class="FormFieldCell"><asp:DropDownList runat="server" ID="ddNavCmd" />&nbsp;<asp:ImageButton runat="Server" Visible="false" ID="imgAddCmd" ImageUrl="images/img_48.jpg" /> </td>
        </tr>
    </table>
</div>