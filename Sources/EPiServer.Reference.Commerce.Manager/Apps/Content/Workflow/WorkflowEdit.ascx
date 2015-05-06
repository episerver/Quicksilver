<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WorkflowEdit.ascx.cs" Inherits="Mediachase.Commerce.Manager.Apps.Content.Workflow.WorkflowEdit" %>
<%@ Register Src="~/Apps/Core/SaveControl.ascx" TagName="SaveControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/EditViewControl.ascx" TagName="EditViewControl" TagPrefix="ecf" %>
<div class="editDiv">
    <ecf:EditViewControl AppId="Content" ViewId="Workflow-Edit" ID="ViewControl" runat="server"></ecf:EditViewControl>
    <ecf:SaveControl ID="EditSaveControl" CancelClientScript="CSManagementClient.ChangeView('Content','Workflow-List');"
        SavedClientScript="CSManagementClient.ChangeView('Content','Workflow-List');" runat="server"></ecf:SaveControl>
</div>