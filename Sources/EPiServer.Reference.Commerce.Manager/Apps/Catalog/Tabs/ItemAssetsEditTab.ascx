<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="Mediachase.Commerce.Manager.Catalog.Tabs.ItemAssetsEditTab" Codebehind="ItemAssetsEditTab.ascx.cs" %>

<script type="text/javascript">
    function Assets_AddRow()
    {
        var searchbox = AssetItemsFilter;        
        var selectedItem = null;         
        var useLegacyAssetSystem = $('#<%= UseLegacyAssetSystem.ClientID %>').attr('value');
        try 
        {
            selectedItem = searchbox.getSelectedItem();
            if(selectedItem == null)
            {
                if (useLegacyAssetSystem == 'true')
                {
                   alert('Please select an item');
                }
                else
                {
                    alert('<%= Literal3.Text %>');
                }                
                return;
            }
        }
        catch(e)
        {
            alert('Must select an item');
            return;
        }
        
        var id = selectedItem.get_value();
        var name = selectedItem.get_text();
        var type = "";
        
        if(name.substring(name.length-1) == "/")
            type = "folder";
        else
            type = "file";
        
        // check if item with this id already exists
        for(var index = 0; index < AssetsDefaultGrid.Table.getRowCount(); index++)
        {
            var row = AssetsDefaultGrid.Table.getRow(index);

            if(row.getMemberAt(1).get_text() == id && row.getMemberAt(2).get_text() == type)
            {
                alert('Asset named "'+name+'" already exists. Select a different asset.');
                return;
            }
        }

        var row = AssetsDefaultGrid.Table.addEmptyRow(); 
        AssetsDefaultGrid.beginUpdate();
        row.SetValue(1, id, true, true); 
        row.SetValue(2, type, true, true); 
        row.SetValue(3, name, true, true); 
        row.SetValue(4, $get('AssetGroupName').value, true, true); 
        row.SetValue(5, 0, false, false); 
        AssetsDefaultGrid.endUpdate();

        searchbox.beginUpdate();
        searchbox.unSelect();
        searchbox.set_text('');
        searchbox.collapse();
        searchbox.endUpdate();

        return false;
    }
    
    function Assets_onExpand(sender, eventArgs)
    {
        sender.unSelect();
        sender.filter(sender.get_text());
    }    
    
    /*
    function Assets_onCollapse(sender, eventArgs)
    {
        var index = sender.get_selectedIndex();
        if(index >= 0)
        {
            var text = sender.getItem(index).get_text();
            sender.set_text(text);
        }
    } 
    */

    function AssetsEditTabClose() {
        var dropDown = AssetItemsFilter;
        dropDown.Collapse();

    }

</script>

<div id="DataForm">
    <table class="DataForm">
        <tr>
            <td>
                <table>
                    <tr>
                        <td>
                            <asp:HiddenField ID="UseLegacyAssetSystem" runat="server"/>                           
                            <asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:CatalogStrings, Item_Find_Assets %>" />:</td>
                        <td>
                            <ComponentArt:ComboBox ID="AssetItemsFilter" RunningMode="Callback" runat="server" AutoHighlight="true"
                                AutoComplete="true" AutoFilter="true" CssClass="comboBox" HoverCssClass="comboBoxHover"
                                FocusedCssClass="comboBoxHover" TextBoxCssClass="comboTextBox" TextBoxHoverCssClass="comboBoxHover"
                                DropDownCssClass="comboDropDown" ItemCssClass="comboItem" ItemHoverCssClass="comboItemHover"
                                SelectedItemCssClass="comboItemHover" DropHoverImageUrl="~/Apps/Shell/Styles/images/combobox/drop_hover.gif"
                                DropImageUrl="~/Apps/Shell/Styles/images/combobox/drop.gif" DropDownPageSize="20"
                                FilterCacheSize="0" CacheSize="0" ItemClientTemplateId="itemTemplate" Width="500">
                                <ClientEvents>
                                    <Expand EventHandler="Assets_onExpand" />                                    
                                </ClientEvents>
                                <ClientTemplates>
                                    <ComponentArt:ClientTemplate ID="itemTemplate">
                                        <img src="## DataItem.getProperty('icon') ##" />
                                        ## DataItem.getProperty('Text') ##
                                    </ComponentArt:ClientTemplate>
                                </ClientTemplates>
                            </ComponentArt:ComboBox>
                        </td>
                        <td>
                            <asp:Button runat="server" ID="AssetAddButton" UseSubmitBehavior="false" OnClientClick="Assets_AddRow();return;"
                                Text="<%$ Resources:CatalogStrings, Item_Add_Asset %>" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:CatalogStrings, Item_Group_Name %>" />:</td>
                        <td>
                            <input id="AssetGroupName" name="AssetGroupName" value="default" />
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td>
                &nbsp;
            </td>
        </tr>
        <tr>
            <td>
                <ComponentArt:Grid AllowEditing="true" RunningMode="Client" AutoFocusSearchBox="false"
                    ShowHeader="false" ShowFooter="true" Width="800" SkinID="Inline" runat="server"
                    ID="AssetsDefaultGrid" AutoPostBackOnInsert="false" AutoPostBackOnDelete="false"
                    AutoCallBackOnUpdate="false" 
                    FooterTextCssClass="ca-GridFooterText" ImagesBaseUrl="~/Apps/Shell/styles/images/" CssClass="ca-Grid" AllowMultipleSelect="false"
                    SearchTextCssClass="ca-GridHeaderText"
                    HeaderCssClass="ca-GridHeader" FooterCssClass="ca-GridFooter" GroupByCssClass="ca-GroupByCell"
                    GroupByTextCssClass="ca-GroupByText" PageSize="20" PagerStyle="Numbered" PagerTextCssClass="ca-GridFooterText"
                    PagerButtonWidth="41" PagerButtonHeight="22" SliderHeight="20" SliderWidth="150"
                    SliderGripWidth="9" SliderPopupOffsetX="20" GroupingPageSize="5" PreExpandOnGroup="true"    
                    IndentCellWidth="1" GroupingNotificationTextCssClass="ca-GridHeaderText" GroupBySortImageWidth="10" GroupBySortImageHeight="10"
                    LoadingPanelClientTemplateId="LoadingFeedbackTemplate" LoadingPanelPosition="MiddleCenter" FillContainer="True">
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
                            <a href="javascript:AssetsDefaultGrid.edit(AssetsDefaultGrid.getItemFromClientId('## DataItem.Arguments ##'));">
                                <img alt="edit" src="../../../Apps/Shell/Styles/Images/edit.gif" /></a> | <a href="javascript:AssetsDefaultGrid.deleteItem(AssetsDefaultGrid.getItemFromClientId('## DataItem.ClientId ##'))">
                                    <img alt="delete" src="../../../Apps/Shell/Styles/Images/toolbar/delete.gif" /></a>
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
                            <a href="javascript:AssetsDefaultGrid.edit(AssetsDefaultGrid.getItemFromClientId('## DataItem.ClientId ##'));">
                                <img alt="edit" title="edit" src="../../../Apps/Shell/Styles/Images/edit.gif" /></a> | <a href="javascript:AssetsDefaultGrid.deleteItem(AssetsDefaultGrid.getItemFromClientId('## DataItem.ClientId ##'))">
                                    <img alt="delete" title="delete" src="../../../Apps/Shell/Styles/Images/toolbar/delete.gif" /></a>
                        </ComponentArt:ClientTemplate>
                        <ComponentArt:ClientTemplate ID="EditCommandTemplate">
                            <a href="javascript:AssetsDefaultGrid.editComplete();">Update</a> | <a href="javascript:AssetsDefaultGrid.editCancel();">Cancel</a>
                        </ComponentArt:ClientTemplate>
                        <ComponentArt:ClientTemplate ID="InsertCommandTemplate">
                            <a href="javascript:AssetsDefaultGrid.editComplete();">Insert</a> | <a href="javascript:AssetsDefaultGrid.editCancel();">Cancel</a>
                        </ComponentArt:ClientTemplate>
                    </ClientTemplates>
                </ComponentArt:Grid>
            </td>
        </tr>
        <tr>
            <td>
                <span class="msg-warning">
                <asp:Literal ID="Literal3" runat="server" Visible="false" />              
                </span>
            </td>       
        </tr>
    </table>
</div>
