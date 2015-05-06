<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NewsModule.ascx.cs" Inherits="Mediachase.Commerce.Manager.Dashboard.Modules.NewsModule" %>
<table cellspacing="0" cellpadding="0" width="100%" border="0" style="margin: 0;">
	<tr>
		<td>
			<div id="divNews" style="padding: 5px;">
			</div>
		</td>
	</tr>
</table>

<script type="text/javascript">
var rsslastTime = null;
var waitTime = 9000;
var fl = false;
window.setTimeout("CheckTime()", 1000);
function CheckTime()
{
  if(!fl && rsslastTime)
  {
    var rsscurTime = (new Date()).getTime();
    if (rsscurTime - rsslastTime > waitTime)
    {
        document.getElementById('divNews').innerHTML = "<div style='text-align:center;padding:10px;color:red;' class='text'>" + <%= GetRssErrorString() %> + "</div>";
      return;
    }
    else
    {
      window.setTimeout("CheckTime()", 1000);
    }
  }
}

function CreateRssNews(feedUrl, feedCount, containerId)
{
  rsslastTime = (new Date()).getTime();
  document.getElementById(containerId).innerHTML = "<div style='padding:10px;text-align:center;'><img alt='' align='absmiddle' border='0' src='"+'<%= Page.ResolveClientUrl("~/Apps/Shell/styles/images/Shell/loading_small.gif") %>' + "'></div>";
  var ajaxRequest;
    
  try
  {
    // Opera 8.0+, Firefox, Safari
    ajaxRequest = new XMLHttpRequest();
  } 
  catch (e)
  {
	  // Internet Explorer Browsers
	  try
	  {
		  ajaxRequest = new ActiveXObject("Msxml2.XMLHTTP");
	  } 
	  catch (e) 
	  {
		  try
		  {
			  ajaxRequest = new ActiveXObject("Microsoft.XMLHTTP");
		  } 
		  catch (e)
		  {
			  // Something went wrong
			  alert("Something's wrong with your browser!");
			  return false;
		  }
	  }
  }
  // Create a function that will receive data sent from the server
  ajaxRequest.onreadystatechange = function()
  {
    if(ajaxRequest.readyState == 4)
    {
      fl = true;
      document.getElementById(containerId).innerHTML = ajaxRequest.responseText;
    }
	}
	ajaxRequest.open("GET", '<%= Page.ResolveClientUrl("~/Apps/Dashboard/Modules/RssNewsSource.aspx") %>'+ "?RssPath="+feedUrl+"&RssCount="+feedCount, true);
	ajaxRequest.send(null);
}
</script>