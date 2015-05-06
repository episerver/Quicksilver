<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InformationControl.ascx.cs" Inherits="Mediachase.Commerce.Manager.Core.Common.InformationControl" %>
<div style="position:relative;" id="popupOuterDiv">
    <div style="width:100%; padding: 10px 10px 0px 10px;">
        <asp:Label ID="ErrorText" runat="server" ForeColor="Red" Visible="false"></asp:Label>
        <asp:Literal runat="server" Text="<%$ Resources:SharedStrings, Application_System_Version %>"/>:&nbsp;<asp:Label runat="server" ID="ApplicationSystemVersionText"></asp:Label>
        <br />
        <br />
        <asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:SharedStrings, Business_Foundation_System_Version %>"/>:&nbsp;<asp:Label runat="server" ID="BusinessFoundationSystemVersionText"></asp:Label>
        <br />
        <br />
        <asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:SharedStrings, Catalog_System_Version %>"/>:&nbsp;<asp:Label runat="server" ID="CatalogSystemVersionText"></asp:Label>
        <br />
        <br />
        <asp:Literal ID="Literal4" runat="server" Text="<%$ Resources:SharedStrings, Marketing_System_Version %>"/>:&nbsp;<asp:Label runat="server" ID="MarketingSystemVersionText"></asp:Label>
        <br />
        <br />
        <asp:Literal ID="Literal5" runat="server" Text="<%$ Resources:SharedStrings, Order_System_Version %>"/>:&nbsp;<asp:Label runat="server" ID="OrderSystemVersionText"></asp:Label>
        <br />
        <br />
        <asp:Literal ID="Literal6" runat="server" Text="<%$ Resources:SharedStrings, Security_System_Version %>"/>:&nbsp;<asp:Label runat="server" ID="SecuritySystemVersionText"></asp:Label>
        <br />
        <br />
        <br />
        <br />
    </div>
    <div style="position:absolute; right:10px; bottom: 10px;">
        <asp:Button runat="server" ID="btnClose" Text="<%$ Resources:SharedStrings, Close %>" Width="80px" OnClientClick="javascript:window.close();" />
    </div>
</div>