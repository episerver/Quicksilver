<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="Mediachase.Commerce.Manager.Content.Folders.LanguageMenu" Codebehind="LanguageMenu.ascx.cs" %>
<asp:Menu runat="server" ID="menuPattern" BorderWidth="0" Orientation="Horizontal" Visible="false">
    <StaticMenuItemStyle CssClass="StaticMenu" />
    <DynamicMenuStyle CssClass="DynamicMenu" />
    <DynamicItemTemplate>
        <%#Eval("Text")%>
    </DynamicItemTemplate>
</asp:Menu>

<asp:Table runat="server">
    <asp:TableRow runat="server" ID="trLanguageMenu"/>
</asp:Table>