<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Asset.FileItem" Codebehind="FileItem.ascx.cs" %>
<%@ Register Src="~/Apps/Core/SaveControl.ascx" TagName="SaveControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/EditViewControl.ascx" TagName="EditViewControl" TagPrefix="ecf" %>
<div class="editDiv">
<ecf:EditViewControl AppId="Asset" ViewId="FileItem-Edit" id="ViewControl" runat="server"></ecf:EditViewControl>
<ecf:SaveControl id="EditSaveControl" 
    CancelMessage="<%$ Resources:AssetStrings, Asset_Changes_Cancelled %>" 
    SavedMessage="<%$ Resources:AssetStrings, Asset_Changes_Saved %>" 
    CancelClientScript="CSAssetClient.AssetSaveRedirect();" 
    SavedClientScript="CSAssetClient.AssetSaveRedirect();" runat="server"></ecf:SaveControl>
<span class="msg-warning">
    <asp:Literal ID="Literal1" runat="server" Visible="false" />              
</span>
</div>