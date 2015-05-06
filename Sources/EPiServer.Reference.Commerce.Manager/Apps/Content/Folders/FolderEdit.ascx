<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="Mediachase.Commerce.Manager.Content.Folders.FolderEdit" Codebehind="FolderEdit.ascx.cs" %>
<%@ Register Src="~/Apps/Core/SaveControl.ascx" TagName="SaveControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/EditViewControl.ascx" TagName="EditViewControl" TagPrefix="ecf" %>
<div class="editDiv">
<ecf:EditViewControl AppId="Content" ViewId="Folder-Edit" id="ViewControl" runat="server">
</ecf:EditViewControl>
<ecf:SaveControl id="EditSaveControl" 
    CancelMessage="<%$ Resources:ContentStrings, Folder_Page_Changes_Discarded %>" 
    SavedMessage="<%$ Resources:ContentStrings, Folder_Page_Updated %>"
    CancelClientScript="CSContentClient.FolderSaveRedirect();" 
    SavedClientScript="CSContentClient.FolderSaveRedirect();" runat="server"></ecf:SaveControl>
</div>