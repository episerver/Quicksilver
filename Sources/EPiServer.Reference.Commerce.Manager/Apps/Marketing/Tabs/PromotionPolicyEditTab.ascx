<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Marketing.Tabs.PromotionPolicyEditTab" Codebehind="PromotionPolicyEditTab.ascx.cs" %>
<script type="text/javascript">
    function PromotionPolicy_AddRow()
    {
    }
    
        
</script>
<div id="DataForm">
 <table width="100%" class="DataForm"> 
 <tr>
    <td>
        <table>
            <tr>
                <td>
      <asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Policy_Name %>"/>:
      <br />
      <asp:TextBox runat="server" ID="PolicyName" MaxLength="50" Width="200"></asp:TextBox>      
      <br /><asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:SharedStrings, Expression %>"/>:<br />
            <ComponentArt:ComboBox id="ExpressionFilter" runat="server" 
        Width="200" DataTextField="Name" DataValueField="ExpressionId">
      </ComponentArt:ComboBox>                
                Status: 
                <br />
      <asp:DropDownList runat="server" ID="PolicyStatus" Width="200">
        <asp:ListItem Value="active"><asp:Literal runat="server" Text="<%$ Resources:SharedStrings, Active %>"/></asp:ListItem>
        <asp:ListItem Value="inactive"><asp:Literal runat="server" Text="<%$ Resources:SharedStrings, Inactive %>"/></asp:ListItem>
        <asp:ListItem Value="deleted"><asp:Literal runat="server" Text="<%$ Resources:SharedStrings, Deleted %>"/></asp:ListItem>
      </asp:DropDownList>
        
                </td>
            </tr>
            <tr>
                <td>
                    <a href="#" onclick="PromotionPolicy_AddRow()"><asp:Literal ID="Literal3" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Policy_Add %>"/></a>
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
<ComponentArt:Grid AllowEditing="true" RunningMode="Client" AutoFocusSearchBox="false" ShowHeader="false" ShowFooter="false" Width="100%" SkinID="Inline" runat="server" ID="PolicyGrid" AutoPostBackOnInsert="false" AutoPostBackOnDelete="false" AutoCallBackOnUpdate="false">
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
                        <img src="../../../Apps/Shell/Styles/images/dialog/spinner.gif" width="16" height="16" border="0"></td>
                </tr>
            </table>
        </ComponentArt:ClientTemplate>
          <ComponentArt:ClientTemplate Id="EditTemplate">
            <a href="javascript:DefaultGrid.edit(DefaultGrid.getItemFromClientId('## DataItem.ClientId ##'));">Edit</a> | <a href="javascript:DefaultGrid.deleteItem(DefaultGrid.getItemFromClientId('## DataItem.ClientId ##'))">Delete</a>
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
        <asp:Label runat="server" ID="myLabel" BorderWidth="2" Text='<%# GetExpressionName((int)GetPolicy((int)Container.DataItem["PolicyId"]).Policy[0]["ExpressionId"]) %>' />
      </Template>
    </ComponentArt:GridServerTemplate>
    <ComponentArt:GridServerTemplate ID="PolicyNameTemplate">
      <Template>
        <asp:Label runat="server" ID="Label1" BorderWidth="2" Text='<%# GetPolicy((int)Container.DataItem["PolicyId"]).Policy[0]["Name"] %>' />
      </Template>
    </ComponentArt:GridServerTemplate>
    <ComponentArt:GridServerTemplate ID="PolicyStatusTemplate">
      <Template>
        <asp:Label runat="server" ID="Label2" BorderWidth="2" Text='<%# GetPolicy((int)Container.DataItem["PolicyId"]).Policy[0]["Status"] %>' />
      </Template>
    </ComponentArt:GridServerTemplate>
    </ServerTemplates>
</ComponentArt:Grid>
</td>
 </tr>
 </table>
</div>