<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Asset.List" Codebehind="List.ascx.cs" %>
<%@ Register Src="../Core/Controls/EcfListViewControl.ascx" TagName="EcfListViewControl" TagPrefix="core" %>

<asset:AssetsDatasource runat="server" ID="AssetsDataSource"></asset:AssetsDatasource>
<core:EcfListViewControl id="MyListView" DataSourceID="AssetsDataSource" DataKey="ID" runat="server" AppId="Asset" ViewId="Asset-List" ShowTopToolbar="true"></core:EcfListViewControl>
<span class="msg-warning">
    <asp:Literal ID="Literal1" runat="server" Visible="false" />              
</span>
