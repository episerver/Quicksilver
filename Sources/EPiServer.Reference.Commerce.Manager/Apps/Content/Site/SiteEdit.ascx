<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="Mediachase.Commerce.Manager.Content.Site.SiteEdit" Codebehind="SiteEdit.ascx.cs" %>
<%@ Register Src="~/Apps/Core/SaveControl.ascx" TagName="SaveControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/EditViewControl.ascx" TagName="EditViewControl" TagPrefix="ecf" %>
<div class="editDiv">
<ecf:EditViewControl AppId="Content" ViewId="Site-Edit" id="ViewControl" runat="server"></ecf:EditViewControl>
<ecf:SaveControl id="EditSaveControl" 
    CancelMessage="<%$ Resources:ContentStrings, Site_Changes_Discarded %>" 
    SavedMessage="<%$ Resources:ContentStrings, Site_Updated %>" 
    CancelClientScript="CSManagementClient.CloseTab();CSManagementClient.ChangeView('Content','Site-List');" 
    SavedClientScript="CSManagementClient.CloseTab();CSManagementClient.ChangeView('Content', 'Site-List');" runat="server"></ecf:SaveControl>
</div>