<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Asset.Tabs.FileAddTab" Codebehind="FileAddTab.ascx.cs" %>
<%@ Register Assembly="Mediachase.FileUploader" Namespace="Mediachase.FileUploader.Web.UI" TagPrefix="mcf" %>
<%@ Register TagPrefix="ibn" TagName="ctrlView" Src="~/Apps/MetaDataBase/MetaUI/MetaForm/FormDocumentView.ascx" %>

<%@ Register Assembly="Mediachase.FileUploader" Namespace="Mediachase.FileUploader.Web.UI" TagPrefix="mc" %>
<%@ Register src="~/Apps/Core/Controls/ProgressControl.ascx" tagname="ProgressControl" tagprefix="core" %>

<asp:Panel runat="server" ID="panelUploadFiles">
<script type="text/javascript">
      function CancelUpload()
      {
          var ctrl = $get('<%=FileUpCtrl1.ClientID%>');
          if(ctrl != null && MCFU_Array && MCFU_Array[ctrl.id])
          {
              MCFU_Array[ctrl.id].Cancel();
          }
      }
      
      function BeginUpload()
      {
          var prgc = $get('<%= fuProgress.ClientID %>');
          if(prgc != null)
          {
              StateChanged(id);          
          }
          
          var ctrl = $get('<%=FileUpCtrl1.ClientID%>');
          if(ctrl != null && MCFU_Array && MCFU_Array[ctrl.id])
          {
              MCFU_Array[ctrl.id].Upload();
          }
          
      }
      
      function StateChanged(id) 
	    {
		      if(!MCFU_Array || !MCFU_Array[id] || !MCFU_Array[MCFU_Array[id].MCFUId])
          {
              setTimeout('StateChanged("'+id+'")', 1000);
              return;
          }
          var MCFUP = MCFU_Array[id];
          var MCFU = MCFU_Array[MCFUP.MCFUId];
		      if(MCFU.State == MCFU_States[7])//upload to server successful
          {
              var btn = $get('<%=btnUpload.ClientID%>');

              var utos = $get('<%= GetClientId("lblUploadingToServer") %>');
              if(utos != null)
                  utos.style.display = "none";

              var utop = $get('<%= GetClientId("lblUploadingToProvider") %>');
              if(utop != null)
                  utop.style.display = "";
 
              MCFU.ChangeState(MCFU, MCFU_States[1]);                 
              btn.click();
              MCFU.RefreshUploading();
          }
	    }	
    
    </script>
	    
    <!-- START: Upload block -->
    <table width="100%" border="0">
        <tr>
            <td>
                <mc:FileUploadIFrameControl Visible="true" ID="FileUpCtrl1" runat="server" BlockHeight="80px">
                </mc:FileUploadIFrameControl>
                <asp:UpdatePanel runat="server">
                    <ContentTemplate>
                        <asp:CustomValidator ID="fileNameValidator" runat="server" Display="Dynamic"></asp:CustomValidator>
                        <asp:Button runat="server" ID="btnUpload" style="display: none;" OnClick="btnUpload_ServerClick" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </td>
        </tr>
        <tr>
            <td valign="top">
                <asp:Button Text="<%$ Resources:SharedStrings, Upload %>" OnClientClick="javascript:BeginUpload();return false;" Width="100" runat="server"/>

            </td>
        </tr>            
        <tr onMCFUstatechanged="StateChanged('<%= fuProgress.ClientID %>')">
            <td style="height:60px;">    
                <mc:FileUploadProgress ID="fuProgress" runat="server" FileUploadControlID="FileUpCtrl1" 
                    ProgressBarStyle-BackColor="MediumBlue" ProgressBarStyle-Height="8px"
                    ProgressBarBoundaryStyle-BorderColor="MediumBlue" ProgressBarBoundaryStyle-BorderStyle="Solid" 
                    ProgressBarBoundaryStyle-BorderWidth="1px" ProgressBarBoundaryStyle-Height="10px"  
                    ProgressBarBoundaryStyle-Width="250px" ScriptPath="">
                  <WaitTemplate></WaitTemplate>  
                  <InfoTemplate>
                    <asp:Label ID="lblUploadingToServer" runat="server" Text="<%$ Resources:AssetStrings, Uploading_to_Server %>" /><asp:Label ID="lblUploadingToProvider" runat="server" Text="<%$ Resources:AssetStrings, Uploading_to_Storage_Provider %>" style="display: none;"/>: 
                    <%# DataBinder.Eval(Container, "UploadBytesReceived")%>
                    &nbsp;<asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:SharedStrings, From_Lowercase %>"/>&nbsp;<%# DataBinder.Eval(Container, "UploadBytesTotal")%> &nbsp; <a href="javascript:CancelUpload();"><asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:SharedStrings, Cancel %>"/></a>
                  </InfoTemplate>
                </mc:FileUploadProgress>
            </td>
        </tr>
    </table>
    <!-- END: Upload block -->
</asp:Panel>
<asp:Panel runat="server" ID="panelMetaFields">
<table runat="server" ID="tblMetaFields" cellpadding="10" cellspacing="10" border="0">
    <tr>
        <td>
            <ibn:ctrlView ID="ucView" runat="server" />
        </td>
    </tr>
</table>
</asp:Panel>

