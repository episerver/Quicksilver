<%@ Control Language="C#" Inherits="Mediachase.Commerce.Manager.Apps.Content.Site.Tabs.SiteImportConfirmTab" CodeBehind="SiteImportConfirmTab.ascx.cs"%>
<table cellspacing="10" style="width:100%;margin-left:10px;" border="0">
    <col width="50%" />
    <tr>
        <td>
            <asp:RadioButton runat="server" ID="rbNewSite" Text="Create New Site" Checked="True" GroupName="SiteImportOptionGroup" />
        </td>
    </tr>
    <tr>
        <td>
            <asp:RadioButton runat="server" ID="rbExistingSite" Text="Overwrite Existing Site" GroupName="SiteImportOptionGroup" />
        </td>
        <td>
            <asp:DropDownList runat="server" ID="ddlExistingSite" Width="200"></asp:DropDownList>
        </td>
    </tr>
    <tr>
        <td>
            <asp:Button runat="server" ID="btnDoImport" Text="Do Import" Width="100" OnClick="btnDoImport_Click"/>          
        </td>
    </tr>
</table>
