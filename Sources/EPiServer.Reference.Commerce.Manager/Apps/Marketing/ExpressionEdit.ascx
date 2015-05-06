<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Marketing.ExpressionEdit" Codebehind="ExpressionEdit.ascx.cs" %>
<%@ Register Src="~/Apps/Core/SaveControl.ascx" TagName="SaveControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/EditViewControl.ascx" TagName="EditViewControl" TagPrefix="ecf" %>
<div class="editDiv">
<ecf:EditViewControl AppId="Marketing" ViewId="Expression-Edit" id="ViewControl" runat="server"></ecf:EditViewControl>
<ecf:SaveControl id="EditSaveControl" CancelClientScript="CSManagementClient.ChangeView('Marketing','Expression-List', 'group='+CSManagementClient.QueryString('group'));" SavedClientScript="CSManagementClient.ChangeView('Marketing', 'Expression-List', 'group='+CSManagementClient.QueryString('group'));" runat="server"></ecf:SaveControl>
</div>