<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Marketing.Tabs.PromotionCodeEditTab" Codebehind="PromotionCondEditTab.ascx.cs" %>
<script type="text/javascript">
    function PromotionCondition_AddRow()
    {
        // Get filter values
        var nodeFilter = NodeFilter.getSelectedItem();;
        var entryFilter = EntryFilter.getSelectedItem();;
        var catalogFilter = CatalogFilter.getSelectedItem();
        var expressionFilter = ExpressionFilter.getSelectedItem();
        
        var node = "";
        var nodeText = "";
        var entry = "";
        var entryText = "";
        var catalog = "";
        var expression = "";
        var expressionText = "";

        if(nodeFilter != null)
        {
            node = nodeFilter.get_value().split("$")[0];
            nodeText = nodeFilter.get_text();
        }
        
        if(entryFilter != null)
        {
            entry = entryFilter.get_value();
            entryText = entryFilter.get_text();
        }
        
        if(catalogFilter != null)
        {
            catalog = catalogFilter.get_text();
        }
        
        if(expressionFilter != null)
        {
            expression = expressionFilter.get_value();
            expressionText = expressionFilter.get_text();               
        }
        else
        {
            alert('Please select expression');
            return;
        }
        
        



        var row = DefaultGrid.Table.addEmptyRow(); 

        DefaultGrid.beginUpdate();
        row.SetValue(2, expression, true, true); 
        row.SetValue(3, expressionText, true, true); 
        row.SetValue(4, catalog, true, true); 
        row.SetValue(5, node, true, true); 
        row.SetValue(6, nodeText, true, true); 
        row.SetValue(7, entry, true, true); 
        row.SetValue(8, entryText, false, false); 
        DefaultGrid.endUpdate();

        CSManagementClient.MarkDirty();
    }
    
  function catalogChange(sender, eventArgs)
  {
    NodeFilter.disable();
    EntryFilter.disable();
    NodeFilter.filter(sender.getSelectedItem().get_value());
  }
  
  function nodeChange(sender, eventArgs)
  {
    EntryFilter.disable();
    EntryFilter.filter(sender.getSelectedItem().get_value());
  }  

  function nodeFilterComplete(sender, eventArgs)
  {
    NodeFilter.enable();
  }
    
  function entryFilterComplete(sender, eventArgs)
  {
    EntryFilter.enable();
  }
        
</script>
<div id="DataForm">
 <table width="100%" class="DataForm"> 
 <tr>
    <td>
        <table>
            <tr>
                <td>
      <asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:SharedStrings, Expression %>"/>:
            <ComponentArt:ComboBox id="ExpressionFilter" runat="server" TextBoxEnabled="false" DataMember="Expression"
        Width="200" DataTextField="Name" DataValueField="ExpressionId">
      </ComponentArt:ComboBox>                
      
                <asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:SharedStrings, Catalog %>"/>: 
      <ComponentArt:ComboBox EnableViewState="true" id="CatalogFilter" TextBoxEnabled="false" DataMember="Catalog" runat="server" Width="200" DataTextField="Name"
        DataValueField="Name">
        <ClientEvents>
         <Change EventHandler="catalogChange" />
        </ClientEvents>
      </ComponentArt:ComboBox>
      <asp:Literal ID="Literal3" runat="server" Text="<%$ Resources:SharedStrings, Catalog_Node %>"/>:
            <ComponentArt:ComboBox id="NodeFilter" runat="server" Enabled="false" TextBoxEnabled="false"
        Width="200">
        <ClientEvents>
         <CallbackComplete EventHandler="nodeFilterComplete" />
         <Change EventHandler="nodeChange" />
        </ClientEvents>
        
      </ComponentArt:ComboBox>
      <asp:Literal ID="Literal4" runat="server" Text="<%$ Resources:SharedStrings, Catalog_Entry %>"/>:
            <ComponentArt:ComboBox id="EntryFilter" runat="server" Enabled="false" TextBoxEnabled="false"
        Width="200">
        <ClientEvents>
         <CallbackComplete EventHandler="entryFilterComplete" />
        </ClientEvents>
      </ComponentArt:ComboBox>


                </td>
            </tr>
            <tr>
                <td>
                    <a href="#" onclick="PromotionCondition_AddRow()"><asp:Literal ID="Literal5" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Condition_Add %>"/>/a>
                </td>            
            </tr>
        </table>
    </td>
 </tr>
 <tr>
    <td>&nbsp;</td>
 </tr>
 <tr>
    <td class="wh100">
<ComponentArt:Grid AllowEditing="true" RunningMode="Client" AutoFocusSearchBox="false" ShowHeader="false" ShowFooter="false" Width="100%" SkinID="Inline" runat="server" ID="DefaultGrid" AutoPostBackOnInsert="false" AutoPostBackOnDelete="false" AutoCallBackOnUpdate="false">
    <ClientTemplates>
        <ComponentArt:ClientTemplate ID="CheckHeaderTemplate">
            <input type="checkbox" name="HeaderCheck" />
        </ComponentArt:ClientTemplate>
        <ComponentArt:ClientTemplate ID="HyperlinkTemplate">
            <a href="javascript:DefaultGrid.edit(DefaultGrid.getItemFromClientId('## DataItem.Arguments ##'));">Edit</a> | <a href="javascript:DefaultGrid.deleteItem(DefaultGrid.getItemFromClientId('## DataItem.ClientId ##'))">Delete</a>
        </ComponentArt:ClientTemplate>
        
        <ComponentArt:ClientTemplate ID="LoadingFeedbackTemplate">
            <table cellspacing="0" cellpadding="0" border="0">
                <tr>
                    <td style="font-size: 10px;">
                        Loading...&nbsp;</td>
                    <td>
                        <img src="Apps/Shell/Styles/images/dialog/spinner.gif" width="16" height="16" border="0"></td>
                </tr>
            </table>
        </ComponentArt:ClientTemplate>
          <ComponentArt:ClientTemplate Id="EditTemplate">
            <a href="javascript:DefaultGrid.deleteItem(DefaultGrid.getItemFromClientId('## DataItem.ClientId ##'))">Delete</a>
          </ComponentArt:ClientTemplate>
          <ComponentArt:ClientTemplate Id="EditCommandTemplate">
            <a href="javascript:DefaultGrid.editComplete();">Update</a> | <a href="javascript:DefaultGrid.editCancel();">Cancel</a>
          </ComponentArt:ClientTemplate>
          <ComponentArt:ClientTemplate Id="InsertCommandTemplate">
            <a href="javascript:DefaultGrid.editComplete();">Insert</a> | <a href="javascript:DefaultGrid.editCancel();">Cancel</a>
          </ComponentArt:ClientTemplate>       
    </ClientTemplates>   
    <ServerTemplates>
    <ComponentArt:GridServerTemplate ID="ExpressionTemplate">
      <Template>
        <asp:Label runat="server" ID="myLabel" Text='<%# GetExpressionName((int)Container.DataItem["ExpressionId"]) %>' />
      </Template>
    </ComponentArt:GridServerTemplate>
    <ComponentArt:GridServerTemplate ID="ExpressionTemplateEdit">
      <Template>
        <%# Container.DataItem["ExpressionId"] %>
      </Template>
    </ComponentArt:GridServerTemplate>
    
    <ComponentArt:GridServerTemplate ID="CatalogNodeTemplate">
      <Template>
        <asp:Label runat="server" ID="Label1" Text='<%# GetCatalogNodeName(Container.DataItem["Code"]) %>' />
      </Template>
    </ComponentArt:GridServerTemplate>
    <ComponentArt:GridServerTemplate ID="CatalogEntryTemplate">
      <Template>
        <asp:Label runat="server" ID="Label2" Text='<%# GetCatalogEntryName(Container.DataItem["Code"]) %>' />
      </Template>
    </ComponentArt:GridServerTemplate>

    </ServerTemplates>
</ComponentArt:Grid>
</td>
 </tr>
 </table>
</div>


