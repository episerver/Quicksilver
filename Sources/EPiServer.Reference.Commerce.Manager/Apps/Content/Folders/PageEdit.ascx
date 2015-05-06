<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Content.Folders.PageEdit"
    CodeBehind="PageEdit.ascx.cs" %>
<%@ Register Src="~/Apps/Core/SaveControl.ascx" TagName="SaveControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/EditViewControl.ascx" TagName="EditViewControl"
    TagPrefix="ecf" %>
<div class="editDiv">
    <ecf:EditViewControl AppId="Content" ViewId="Page-Edit" ID="ViewControl" runat="server">
    </ecf:EditViewControl>
    <ecf:SaveControl ID="EditSaveControl" 
        CancelMessage="<%$ Resources:ContentStrings, Folder_Page_Changes_Discarded %>"
        SavedMessage="<%$ Resources:ContentStrings, Folder_Page_Updated %>" 
        CancelClientScript="CSContentClient.FolderSaveRedirect();"
        SavedClientScript="CSContentClient.FolderSaveRedirect();" runat="server"></ecf:SaveControl>
</div>
