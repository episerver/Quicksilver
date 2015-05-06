<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Marketing.PromotionList" Codebehind="PromotionList.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/EcfListViewControl.ascx" TagName="EcfListViewControl" TagPrefix="core" %>
<core:EcfListViewControl id="MyListView" runat="server" DataKey="PromotionId" AppId="Marketing" DataKeyField="PromotionId" ViewId="Promotion-List" ShowTopToolbar="true"></core:EcfListViewControl>

