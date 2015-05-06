<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Content.Template.TemplateList" Codebehind="TemplateList.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/EcfListViewControl.ascx" TagName="EcfListViewControl" TagPrefix="core" %>
<core:EcfListViewControl id="MyListView" runat="server" DataKey="TemplateId" AppId="Content" ViewId="Templates-List" ShowTopToolbar="true"></core:EcfListViewControl>

