<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="Mediachase.Commerce.Manager.Marketing.SegmentList" Codebehind="SegmentList.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/EcfListViewControl.ascx" TagName="EcfListViewControl" TagPrefix="core" %>
<core:EcfListViewControl id="MyListView" runat="server" DataKey="SegmentId" AppId="Marketing" ViewId="Segment-List" ShowTopToolbar="true"></core:EcfListViewControl>
