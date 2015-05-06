<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Marketing.PromotionEdit" Codebehind="PromotionEdit.ascx.cs" %>
<%@ Register Src="~/Apps/Core/SaveControl.ascx" TagName="SaveControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/EditViewControl.ascx" TagName="EditViewControl" TagPrefix="ecf" %>
<div class="editDiv">
<ecf:EditViewControl AppId="Marketing" ViewId="Promotion-Edit" id="ViewControl" runat="server"></ecf:EditViewControl>
<ecf:SaveControl id="EditSaveControl" CancelClientScript="CSManagementClient.ChangeView('Marketing','Promotion-List');" SavedMessage="Changes saved. Please be advised that promotion rules are typically cached for 10 min, so you might not see your changes immediately." SavedClientScript="CSManagementClient.ChangeView('Marketing', 'Promotion-List');" runat="server"></ecf:SaveControl>
</div>