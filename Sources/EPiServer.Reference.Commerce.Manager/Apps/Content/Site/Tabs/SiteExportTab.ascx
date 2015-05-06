<%@ Control Language="c#" Inherits="Mediachase.Commerce.Manager.Content.Site.Tabs.SiteExportTab" Codebehind="SiteExportTab.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/FileListControl.ascx" TagName="FileList" TagPrefix="core" %>
<%@ Register src="~/Apps/Core/Controls/ProgressControl.ascx" tagname="ProgressControl" tagprefix="core" %>
<div class="w100">
<div style="padding-left:10px; padding-right:5px; padding-top:5px; padding-bottom:5px;">
    <br />
    <asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:ContentStrings, Site_Export_Instructions_Partial_1 %>"/>&nbsp;<b><%= SiteName %></b>.&nbsp;<asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:ContentStrings, Site_Export_Instructions_Partial_2 %>"/><br /><br />
    <!-- START: Export Button -->
    <asp:UpdatePanel ID="MainPanel" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Button runat="server" ID="BtnExport" Text="<%$ Resources:SharedStrings, Start_Export %>" />
        </ContentTemplate>
    </asp:UpdatePanel>
    <!-- END: Export Button -->
    <core:ProgressControl Title="Exporting site" ID="ProgressControl1" runat="server" />
</div>
<br />
<br />
<div style="padding-left:5px; padding-right:5px; padding-top:5px; padding-bottom:5px;">
    <asp:Label runat="server" ID="lblFilesListHeader" Font-Bold="true" 
        Text="<%$ Resources:ContentStrings, Site_Exported_Files_Description %>"></asp:Label>:
    <br />
    <asp:UpdatePanel ID="FilesPanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <table>
                <tr>
                    <td align="left">
                        <core:FileList ID="FilesControl" runat="server" GridAppId="Content" GridViewId="ContentFilesList-Export" />
                    </td>
               </tr>
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
</div>