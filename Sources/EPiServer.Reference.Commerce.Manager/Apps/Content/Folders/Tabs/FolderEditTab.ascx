<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Folders.Tabs.FolderEditTab" Codebehind="FolderEditTab.ascx.cs" %>
<div id="DataForm">
    <table class="DataForm" width="650"> 
        <tr>
            <td class="FormLabelCell"><asp:Literal ID="Literal1" runat="server" Text='<%$ Resources:SharedStrings, Name %>'></asp:Literal>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="250" ID="Name"></asp:TextBox><br />
                <asp:RequiredFieldValidator runat="server" ID="valName" ControlToValidate="Name" 
                    ErrorMessage="<%$ Resources:SharedStrings, Name_Required %>" Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:CustomValidator runat="server" ID="NameCheckCustomValidator" ControlToValidate="Name" OnServerValidate="NameCheck" Display="Dynamic" ErrorMessage="<%$ Resources:ContentStrings, Folder_With_Name_Exists %>" />
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr id="ParentFolderRow" runat="server" visible="false">
            <td class="FormLabelCell" nowrap><asp:Literal ID="Literal2" runat="server" Text='<%$ Resources:ContentStrings, Folder_ParentFolder %>'></asp:Literal>:</td>
            <td class="FormFieldCell"><asp:DropDownList runat="server" ID="Root"></asp:DropDownList></td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr id="RolesRow" runat="server">
            <td class="FormLabelCell" nowrap><asp:Literal ID="Literal3" runat="server" Text='<%$ Resources:ContentStrings, Folder_FullAccess %>'></asp:Literal>:</td>
            <td class="FormFieldCell"><asp:CheckBoxList runat="server" ID="RolesList" RepeatColumns="3"></asp:CheckBoxList></td>
        </tr>
    </table>
</div>