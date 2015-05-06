<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Marketing.PolicyEdit" Codebehind="PolicyEdit.ascx.cs" %>
<%@ Register Src="~/Apps/Core/SaveControl.ascx" TagName="SaveControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/EditViewControl.ascx" TagName="EditViewControl" TagPrefix="ecf" %>
<div class="editDiv">
<ecf:EditViewControl AppId="Marketing" ViewId="Policy-Edit" id="ViewControl" runat="server"></ecf:EditViewControl>
<ecf:SaveControl id="EditSaveControl" CancelClientScript="CSManagementClient.ChangeView('Marketing','Policy-List', 'group='+CSManagementClient.QueryString('group'));" SavedClientScript="CSManagementClient.ChangeView('Marketing', 'Policy-List', 'group='+CSManagementClient.QueryString('group'));" runat="server"></ecf:SaveControl>
</div>