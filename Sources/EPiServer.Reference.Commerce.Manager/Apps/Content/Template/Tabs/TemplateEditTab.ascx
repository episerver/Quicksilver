<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Template.Tabs.TemplateEditTab" Codebehind="TemplateEditTab.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/HtmlEditControl.ascx" TagName="HtmlEditControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/BooleanEditControl.ascx" TagName="BooleanEditControl" TagPrefix="ecf" %>
<div id="DataForm">
    <table>
        <tr>
            <td class="FormLabelCell"><asp:Label runat="server" Text="<%$ Resources:SharedStrings, Name %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="200" ID="Name"></asp:TextBox>
                <asp:RequiredFieldValidator ID="NameRequired" ControlToValidate="Name" Display="Dynamic" Font-Name="verdana" Font-Size="9pt" ErrorMessage="<%$ Resources:SharedStrings, Name_Required %>" runat="server"/>
                <asp:RegularExpressionValidator ID="NameRegularExpression" Display="Dynamic" runat="server" ErrorMessage="<%$ Resources:SharedStrings, Latin_Symbols_Only_Start_With_Letter %>" ValidationExpression="^[a-zA-Z](\w)*" ControlToValidate="Name"/>
                <asp:CustomValidator runat="server" ID="NameCheckCustomValidator" ControlToValidate="Name" OnServerValidate="NameCheck" Display="Dynamic" ErrorMessage="<%$ Resources:ContentStrings, DisplayTemplate_With_Name_Exists %>" />
            </td>
        </tr>
       <tr>  
         <td colspan="2" class="FormSpacerCell"></td> 
       </tr>           
        <tr>
            <td class="FormLabelCell"><asp:Label runat="server" Text="<%$ Resources:SharedStrings, Friendly_Name %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="300" ID="FriendlyName"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidatorFriendlyName" ControlToValidate="FriendlyName" Display="Dynamic" Font-Name="verdana" Font-Size="9pt" ErrorMessage="<%$ Resources:SharedStrings, Friendly_Name_Required %>" runat="server"/>
            </td>
        </tr>
       <tr>  
         <td colspan="2" class="FormSpacerCell"></td> 
       </tr>            
       <tr>
            <td class="FormLabelCell"><asp:Label ID="Label1" runat="server" Text="<%$ Resources:SharedStrings, Type %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="100" ID="TemplateType"></asp:TextBox>
            </td>
        </tr>
       <tr>  
         <td colspan="2" class="FormSpacerCell"></td> 
       </tr> 
       <tr>
            <td class="FormLabelCell"><asp:Label runat="server" Text="<%$ Resources:SharedStrings, Path %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="350" ID="Path"></asp:TextBox>
            </td>
        </tr>
       <tr>  
         <td colspan="2" class="FormSpacerCell"></td> 
       </tr>            
           
    </table>
</div>