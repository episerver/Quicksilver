<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Marketing.SegmentEdit" Codebehind="SegmentEdit.ascx.cs" %>
<%@ Register Src="~/Apps/Core/SaveControl.ascx" TagName="SaveControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/EditViewControl.ascx" TagName="EditViewControl" TagPrefix="ecf" %>
<div class="editDiv">
<ecf:EditViewControl AppId="Marketing" ViewId="Segment-Edit" id="ViewControl" runat="server"></ecf:EditViewControl>
<ecf:SaveControl id="EditSaveControl" CancelClientScript="CSManagementClient.ChangeView('Marketing','Segment-List');" SavedClientScript="CSManagementClient.ChangeView('Marketing', 'Segment-List');" runat="server"></ecf:SaveControl>
</div>
