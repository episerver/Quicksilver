<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FolderHyperlinkTemplate.ascx.cs" Inherits="Mediachase.Commerce.Manager.Apps.Content.Folders.GridTemplates.FolderHyperlinkTemplate" %>
<%@ Import Namespace="Mediachase.Web.Console.Controls" %>
<asp:Image ID="ObjectImage" runat="server" />
<asp:HyperLink ID="FolderLink" runat="server" NavigateUrl='<%# String.Format("javascript:CSManagementClient.ChangeView(\"Content\", \"Folder-List\",\"folderid={0}&siteid={1}\");", DataBinder.Eval(DataItem, "[PageId]"), DataBinder.Eval(DataItem, "[SiteId]")) %>' Text='<%# EcfListView.GetDataCellValue(DataBinder.Eval(DataItem, "[Name]")) %>'></asp:HyperLink>
<asp:Label ID="PageLabel" runat="server" Text='<%# EcfListView.GetDataCellValue(DataBinder.Eval(DataItem, "[Name]")) %>'></asp:Label>