<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="Mediachase.Commerce.Manager.Content.Menu.MenuItemEdit" Codebehind="MenuItemEdit.ascx.cs" %>
<%@ Register Src="~/Apps/Core/SaveControl.ascx" TagName="SaveControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/EditViewControl.ascx" TagName="EditViewControl" TagPrefix="ecf" %>
<div class="editDiv">
<ecf:EditViewControl AppId="Content" ViewId="MenuItem-Edit" id="ViewControl" runat="server"></ecf:EditViewControl>
<ecf:SaveControl id="EditSaveControl" 
    CancelMessage="<%$ Resources:ContentStrings, Menu_Item_Changes_Discarded %>" 
    SavedMessage="<%$ Resources:ContentStrings, Menu_Item_Updated %>" 
    CancelClientScript="CSContentClient.MenuItemSaveRedirect();" 
    SavedClientScript="CSContentClient.MenuItemSaveRedirect();" runat="server"></ecf:SaveControl>
</div>