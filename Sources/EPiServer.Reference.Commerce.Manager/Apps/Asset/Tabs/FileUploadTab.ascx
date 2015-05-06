<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Asset.Tabs.FileUploadTab" Codebehind="FileUploadTab.ascx.cs" %>
<%@ Register Assembly="Mediachase.FileUploader" Namespace="Mediachase.FileUploader.Web.UI" TagPrefix="mcf" %>
<script type="text/javascript">  
    function ShowProgress()
	{
		var w = 300;
		var h = 180;
		var l = (screen.width - w) / 2;
		var t = (screen.height - h) / 2;
		winprops = 'resizable=0, height='+h+',width='+w+',top='+t+',left='+l+'w';
		var f = window.open('<%=ResolveClientUrl("~/Apps/Core/Controls/Uploader/uploadprogress.aspx")%>?key=progress&progressUid='+document.forms[0].__MEDIACHASE_FORM_UNIQUEID.value, "_blank", winprops);
	}
</script>
<table width="100%" cellpadding="10" cellspacing="10" border="0">
    <tr>
        <td>
    	<mcf:mchtmlinputfile id="mcHtmlInputFile" runat="server" maxlength="-1" multifilecount="5"
		multifileupload="True" name="McHtmlInputFile1" size="-1" type="file" width="400px"></mcf:mchtmlinputfile>
        </td>    
    </tr>
    <tr>
        <td>    
	    <input id="Submit1" type="submit" value="<%$ Resources:SharedStrings, Upload %>" runat="server" onclick="ShowProgress()" onserverclick="btnUpload_ServerClick" />        
        </td>
    </tr>
</table>

