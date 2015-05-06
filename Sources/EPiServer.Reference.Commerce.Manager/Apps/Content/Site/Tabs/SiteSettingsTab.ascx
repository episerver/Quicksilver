<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Site.Tabs.SiteSettingsTab" Codebehind="SiteSettingsTab.ascx.cs" %>
<script type="text/javascript">
    function SiteSettings_AddRow()
    {
        var keyControl = $get('<%= tbKey.ClientID %>');
        var valueControl = $get('<%= tbValue.ClientID %>');
        
        if(keyControl == null || valueControl==null)
            return;
            
        var keyString = keyControl.value;
        
        if(keyString.trim() == '')
        {
            alert('Key must not be empty!');
            return;
        }
        
        if(IsSiteVariableSystem(keyString))
        {
            alert('This key is already used in site system variables!');
            return;
        }

        var valueString = valueControl.value;

        // check if item with this key already exists
        for(var index = 0; index < SiteSettingsDefaultGrid.Table.getRowCount(); index++)
        {
            var row = SiteSettingsDefaultGrid.Table.getRow(index);

            if (row.getMemberAt(2).get_text() == keyString) 
            {
                alert('Record with "'+id+'" key already exists');
                return;
            }
        }

        // add new item
        var row = SiteSettingsDefaultGrid.Table.addEmptyRow();
        
        SiteSettingsDefaultGrid.beginUpdate();
        row.SetValue(1, -1, true, true);
        row.SetValue(2, keyString, true, true);
        row.SetValue(3, valueString, false, false); 
        SiteSettingsDefaultGrid.endUpdate();
        
        keyControl.value = "";
        valueControl.value = "";
        
        return false;
    }
    
    function IsSiteVariableSystem(key)
    {
        var found = false;
        
        if(systemVariablesArray!=null)
        {   
            for(var index = 0; index < systemVariablesArray.length; index++)
            {
                if(systemVariablesArray[index].toLowerCase() == key.toLowerCase())
                {
                    found = true;
                    break;
                }
            }
        }
        
        return found;
    }
</script>

<div id="DataForm">
    <table class="DataForm">
        <tr>
            <td>
                <table>
                    <tr>
                        <td>
                            <asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:SharedStrings, Key %>" />:</td>
                        <td>
                            <asp:TextBox runat="server" ID="tbKey" ValidationGroup="SiteAdditionalSettingsVG"></asp:TextBox>
                            <asp:RequiredFieldValidator runat="server" ID="KeyRequiredValidator" Display="Dynamic" ControlToValidate="tbKey" ValidationGroup="SiteAdditionalSettingsVG">*</asp:RequiredFieldValidator>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Literal ID="Literal11" runat="server" Text="<%$ Resources:SharedStrings, Value %>" />:</td>
                        <td>
                            <asp:TextBox runat="server" ID="tbValue" ValidationGroup="SiteAdditionalSettingsVG"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" align="right">
                            <asp:Button runat="server" ID="SettingAddButton" UseSubmitBehavior="false" OnClientClick="SiteSettings_AddRow();return;"
                                Text="<%$ Resources:SharedStrings, Add_Attribute %>" CssClass="button" ValidationGroup="SiteAdditionalSettingsVG" />
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>
                <ComponentArt:Grid AllowEditing="true" AllowPaging="true" RunningMode="Client" AutoFocusSearchBox="false"
                    ShowHeader="false" ShowFooter="true" Width="700" SkinID="Inline" runat="server"
                    ID="SiteSettingsDefaultGrid" AutoPostBackOnInsert="false" AutoPostBackOnDelete="false" AutoPostBackOnUpdate="false" 
                    AutoCallBackOnInsert="false" AutoCallBackOnDelete="false" AutoCallBackOnUpdate="false"
                    FooterTextCssClass="ca-GridFooterText" ImagesBaseUrl="~/Apps/Shell/styles/images/" CssClass="ca-Grid" AllowMultipleSelect="false"
                    SearchTextCssClass="ca-GridHeaderText"
                    HeaderCssClass="ca-GridHeader" FooterCssClass="ca-GridFooter" GroupByCssClass="ca-GroupByCell"
                    GroupByTextCssClass="ca-GroupByText" PageSize="20" PagerStyle="Numbered" PagerTextCssClass="ca-GridFooterText"
                    PagerButtonWidth="41" PagerButtonHeight="22" SliderHeight="20" SliderWidth="150"
                    SliderGripWidth="9" SliderPopupOffsetX="20" GroupingPageSize="5" PreExpandOnGroup="true"    
                    IndentCellWidth="1" GroupingNotificationTextCssClass="ca-GridHeaderText" GroupBySortImageWidth="10" GroupBySortImageHeight="10"
                    LoadingPanelClientTemplateId="LoadingFeedbackTemplate" LoadingPanelPosition="MiddleCenter">
                <Levels>
                    <ComponentArt:GridLevel ShowTableHeading="false" ShowSelectorCells="false"  RowCssClass="ca-Row"
                        DataCellCssClass="ca-DataCell" HeadingCellCssClass="ca-HeadingCell"
                        HeadingCellHoverCssClass="ca-HeadingCellHover" HeadingCellActiveCssClass="ca-HeadingCellActive"
                        HeadingRowCssClass="ca-HeadingRow" HeadingTextCssClass="ca-HeadingCellText" SelectedRowCssClass="ca-SelectedRow"
                        GroupHeadingCssClass="ca-GroupHeading" SortAscendingImageUrl="grid/asc.gif" SortDescendingImageUrl="grid/desc.gif"
                        SortImageWidth="10" SortImageHeight="19" EditCommandClientTemplateId="EditCommandTemplate" InsertCommandClientTemplateId="InsertCommandTemplate">
                        <Columns>
                        </Columns>
                    </ComponentArt:GridLevel>
                </Levels>                    
                    <ClientTemplates>
                        <ComponentArt:ClientTemplate ID="CheckHeaderTemplate">
                            <input type="checkbox" name="HeaderCheck" />
                        </ComponentArt:ClientTemplate>
                        <ComponentArt:ClientTemplate ID="HyperlinkTemplate">
                            <a href="javascript:SiteSettingsDefaultGrid.edit(SiteSettingsDefaultGrid.getItemFromClientId('## DataItem.Arguments ##'));">Edit</a> | <a href="javascript:SiteSettingsDefaultGrid.deleteItem(SiteSettingsDefaultGrid.getItemFromClientId('## DataItem.ClientId ##'))">Delete</a>
                        </ComponentArt:ClientTemplate>
                        <ComponentArt:ClientTemplate ID="LoadingFeedbackTemplate">
                            <table cellspacing="0" cellpadding="0" border="0">
                                <tr>
                                    <td style="font-size: 10px;">
                                        Loading...&nbsp;</td>
                                    <td>
                                        <img src="Apps/Shell/Styles/images/grid/spinner.gif" width="16" height="16" border="0"></td>
                                </tr>
                            </table>
                        </ComponentArt:ClientTemplate>
                        <ComponentArt:ClientTemplate ID="EditTemplate">
                            <a href="javascript:SiteSettingsDefaultGrid.edit(SiteSettingsDefaultGrid.getItemFromClientId('## DataItem.ClientId ##'));">
                                <img alt="edit" src="../../../Apps/Shell/Styles/Images/edit.gif" /></a></a> | <a href="javascript:SiteSettingsDefaultGrid.deleteItem(SiteSettingsDefaultGrid.getItemFromClientId('## DataItem.ClientId ##'))">
                                    <img alt="delete" src="../../../Apps/Shell/Styles/Images/toolbar/delete.gif" /></a>
                        </ComponentArt:ClientTemplate>
                        <ComponentArt:ClientTemplate ID="EditCommandTemplate">
                            <a href="javascript:SiteSettingsDefaultGrid.editComplete();">Update</a> | <a href="javascript:SiteSettingsDefaultGrid.editCancel();">Cancel</a>
                        </ComponentArt:ClientTemplate>
                        <ComponentArt:ClientTemplate ID="InsertCommandTemplate">
                            <a href="javascript:SiteSettingsDefaultGrid.editComplete();">Insert</a> | <a href="javascript:SiteSettingsDefaultGrid.editCancel();">Cancel</a>
                        </ComponentArt:ClientTemplate>
                    </ClientTemplates>
                </ComponentArt:Grid>
            </td>
        </tr>
    </table>
</div>