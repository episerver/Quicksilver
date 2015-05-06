<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StateEdit.ascx.cs" Inherits="Mediachase.Commerce.Manager.Apps.Content.Workflow.StateEdit" %>
<%@ Register Src="~/Apps/Core/SaveControl.ascx" TagName="SaveControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/EditViewControl.ascx" TagName="EditViewControl" TagPrefix="ecf" %>
<div class="editDiv">
    <ecf:EditViewControl AppId="Content" ViewId="State-Edit" ID="ViewControl" runat="server"></ecf:EditViewControl>
    <ecf:SaveControl ID="EditSaveControl" runat="server"></ecf:SaveControl>
</div>