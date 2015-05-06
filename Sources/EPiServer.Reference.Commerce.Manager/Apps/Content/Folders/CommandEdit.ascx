<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="Mediachase.Commerce.Manager.Content.Folders.CommandEdit" Codebehind="CommandEdit.ascx.cs" %>
<%@ Register Src="~/Apps/Core/SaveControl.ascx" TagName="SaveControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/EditViewControl.ascx" TagName="EditViewControl" TagPrefix="ecf" %>
<div class="editDiv">
<ecf:EditViewControl AppId="Content" ViewId="PageCmd-Edit" id="ViewControl" runat="server"></ecf:EditViewControl>
<ecf:SaveControl id="EditSaveControl" CancelClientScript="CSContentClient.PageCmdSaveRedirect();" SavedClientScript="CSContentClient.PageCmdSaveRedirect();" runat="server"></ecf:SaveControl>
</div>