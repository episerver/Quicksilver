<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="Mediachase.Commerce.Manager.Marketing.CampaignList" Codebehind="CampaignList.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/EcfListViewControl.ascx" TagName="EcfListViewControl" TagPrefix="core" %>
<core:EcfListViewControl id="MyListView" runat="server" DataKey="CampaignId" AppId="Marketing" ViewId="Campaign-List" ShowTopToolbar="true"></core:EcfListViewControl>