<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CatalogEntrySubsetSelector.ascx.cs" Inherits="Mediachase.Commerce.Manager.Marketing.EntrySearch" %>
<%@ Register Assembly="Mediachase.BusinessFoundation" Namespace="Mediachase.BusinessFoundation" TagPrefix="mc" %>

<div style="background-color: #F8F8F8; height:100%">
	<table cellpadding="2" style="background-color: #F8F8F8; height: 100%;">
        <tr>
            <td class="FormLabelCell" style="width:150px">
                <b><asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:CatalogStrings, Catalog_Search_By_Keyword %>" />:</b>
            </td>
            <td class="FormFieldCell" colspan="2">
                <asp:TextBox ID="tbKeywords" Width="240" MaxLength="40" runat="server"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal3" runat="server" Text="<%$ Resources:CatalogStrings, Catalog_Filter_By_Language %>" />:
            </td>
            <td class="FormFieldCell" colspan="2">
                <asp:DropDownList runat="server" ID="ListLanguages" Width="250" DataValueField="LanguageId"
                    DataTextField="Name">
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal5" runat="server" Text="<%$ Resources:CatalogStrings, Catalog_Filter_By_Catalog %>" />:
            </td>
            <td class="FormFieldCell">
                <asp:DropDownList runat="server" ID="ListCatalogs" Width="250" DataValueField="CatalogId"
                    DataTextField="Name">
                </asp:DropDownList>
            </td>
            <td>
                <asp:Button ID="btnSearch" runat="server" Width="100" Text="<%$ Resources:SharedStrings, Search %>" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="LabelForResultsSize" runat="server" Text="<%$ Resources:MarketingStrings, Number_of_records_to_display %>"></asp:Label>
            </td>
            <td class="FormFieldCell" colspan="2">
                <asp:DropDownList ID="ListResultSizeOptions" runat="server">
                    <asp:ListItem Text="10" Value="10"></asp:ListItem>
                    <asp:ListItem Text="20" Value="20"></asp:ListItem>
                    <asp:ListItem Text="100" Value="100"></asp:ListItem>
                    <asp:ListItem Text="200" Value="200"></asp:ListItem>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell" colspan="3">
                <asp:Literal ID="litResultsFound" runat="server"></asp:Literal>
            </td>
        </tr>
        <tr>
			<td colspan="3">
                
			    <mc:ListToListSelector runat="server" ID="ltlSelector" OneItemToTargetButtonId="btnMoveSelectedToTarget" OneItemToSourceButtonId="btnMoveSelectedToSource" SourceListId="lbSource" TargetListId="lbTarget" SelectAllSourceItemsButtonId="btnSelectSourceAll" SelectAllTargetItemsButtonId="btnSelectTargetAll">
		        </mc:ListToListSelector>
				<table border="0" cellspacing="0">
					<tr>
						<td style="padding:10px;">
							<asp:Literal runat="server" Text="<%$ Resources:CatalogStrings, Catalog_Found_Catalog_Entries %>"></asp:Literal>&nbsp;&nbsp;<asp:Button ID="btnSelectSourceAll" runat="server" Text="Select All" OnClientClick="return false;" /><br />
							<div class="scrollable">
								<asp:ListBox runat="server" ID="lbSource" SelectionMode="Multiple" 
									DataTextField="Name" DataValueField="ID" Height="205" >
								</asp:ListBox>
							</div>
						</td>
						<td  style="vertical-align:middle; padding:10px; text-align:center;">
							<asp:Button runat="server" ID="btnMoveSelectedToTarget" Text=">" OnClientClick="return false;" /><br />
							<asp:Button runat="server" ID="btnMoveSelectedToSource" Text="<" OnClientClick="return false;"  />
						</td>
						<td style="padding:10 10 0 10">
							<asp:Literal  runat="server" Text="<%$ Resources:CatalogStrings, Catalog_Selected_Catalog_Entries %>"></asp:Literal>&nbsp;&nbsp;<asp:Button ID="btnSelectTargetAll" runat="server" Text="Select All" OnClientClick="return false;" /><br />
							<div class="scrollable">
								<asp:ListBox runat="server" ID="lbTarget" SelectionMode="Multiple" Height="205px"></asp:ListBox>
							</div>
						</td>
					</tr>
				</table>
			</td>
        </tr>
        <tr>
			<td colspan="3" style="text-align:right; padding:0 10 10 0">
				<asp:Button runat="server" ID="btnSave" Text="<%$Resources:CatalogStrings, Catalog_Save_Selection %>" Width="100" />
			</td>
        </tr>
    </table>
</div>
<script type="text/javascript">
$(document).ready(function() 
{
//	updateListBoxHeight();
	
	if (Mediachase && Mediachase.ListToListSelector)
	{
		Mediachase.ListToListSelector.prototype.AddOption = function(objTo,Option) 
			{
				var oOption = document.createElement("OPTION");
				oOption.text = Option.text;
				oOption.value = Option.value;
				if(objTo!=null)
					objTo.options[objTo.options.length] = oOption;
//				updateListBoxHeight();
			};
	}
});

function updateListBoxHeight()
{
	var selectArr = $('.scrollable select');
	for (var i = 0; i < selectArr.length; i++)
	{
		selectArr[i].style.height = selectArr[i].scrollHeight + 'px';
	}
}
</script>