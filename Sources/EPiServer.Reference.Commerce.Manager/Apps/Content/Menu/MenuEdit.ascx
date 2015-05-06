<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="Mediachase.Commerce.Manager.Content.Menu.MenuEdit" Codebehind="MenuEdit.ascx.cs" %>
<%@ Register Src="~/Apps/Core/SaveControl.ascx" TagName="SaveControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/EditViewControl.ascx" TagName="EditViewControl" TagPrefix="ecf" %>
<div class="editDiv">
<ecf:EditViewControl AppId="Content" ViewId="Menu-Edit" id="ViewControl" runat="server"></ecf:EditViewControl>
<ecf:SaveControl id="EditSaveControl" 
    CancelMessage="<%$ Resources:ContentStrings, Menu_Changes_Discarded %>" 
    SavedMessage="<%$ Resources:ContentStrings, Menu_Updated %>" 
    CancelClientScript="CSContentClient.MenuSaveRedirect();" 
    SavedClientScript="CSContentClient.MenuSaveRedirect();" runat="server"></ecf:SaveControl>
</div>