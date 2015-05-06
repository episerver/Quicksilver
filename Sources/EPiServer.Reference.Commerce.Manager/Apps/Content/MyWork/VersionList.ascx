<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="Mediachase.Commerce.Manager.Content.MyWork.VersionList" Codebehind="VersionList.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/EcfListViewControl.ascx" TagName="EcfListViewControl" TagPrefix="core" %>    
<core:EcfListViewControl id="MyListView" runat="server" AppId="Content" ViewId="Work-List" ShowTopToolbar="true"></core:EcfListViewControl>