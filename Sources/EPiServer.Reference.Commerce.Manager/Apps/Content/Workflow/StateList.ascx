<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Content.Workflow.StateList" Codebehind="StateList.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/EcfListViewControl.ascx" TagName="EcfListViewControl" TagPrefix="core" %>
<core:EcfListViewControl id="MyListView" runat="server" DataKey="StatusId" AppId="Content" ViewId="State-List" ShowTopToolbar="true"></core:EcfListViewControl>

