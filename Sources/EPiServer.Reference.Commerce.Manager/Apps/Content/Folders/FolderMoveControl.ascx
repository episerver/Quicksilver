<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FolderMoveControl.ascx.cs" Inherits="Mediachase.Commerce.Manager.Content.Folders.FolderMoveControl" %>
<div style="padding-top: 5px;">
    <table width="100%" cellpadding="1" cellspacing="0">
        <tr>
            <td valign="middle" align="right">
                <asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:ContentStrings, Folder_TargetFolder %>" />:&nbsp;
            </td>
            <td style="padding: 1px;" align="left" valign="middle">
                <asp:DropDownList runat="server" ID="ddlFolders" Width="300"></asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td colspan="2" style="height: 40px; padding-right: 10px;" align="right">
                <asp:Button runat="server" ID="btnOK" Text="<%$ Resources:SharedStrings, OK %>" Width="80px" />
                &nbsp;&nbsp;&nbsp;
                <asp:Button runat="server" ID="btnClose" Text="<%$ Resources:SharedStrings, Close %>" Width="80px" />
            </td>
        </tr>
    </table>
</div>