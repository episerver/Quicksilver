<%@ Control Language="C#" %>
<%@ Import Namespace="ComponentArt.Web.UI" %>
<asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl='<%# String.Format("{0}", ((GridServerTemplateContainer)Container).DataItem["Url"])%>' ImageUrl='<%# String.Format("{0}", ((GridServerTemplateContainer)Container).DataItem["Icon"]) %>'/>
<asp:HyperLink ID="HyperLink2" runat="server" NavigateUrl='<%# String.Format("javascript:CSAssetClient.OpenItem(\"{0}\",\"{1}\");", ((GridServerTemplateContainer)Container).DataItem["Type"], ((GridServerTemplateContainer)Container).DataItem["ID"]) %>' Text='<%# ((GridServerTemplateContainer)Container).DataItem["FileName"] %>'>
</asp:HyperLink>

