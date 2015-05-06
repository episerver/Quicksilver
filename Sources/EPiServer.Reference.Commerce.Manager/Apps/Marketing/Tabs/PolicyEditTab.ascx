<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Marketing.Tabs.PolicyEditTab" Codebehind="PolicyEditTab.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/BooleanEditControl.ascx" TagName="BooleanEditControl" TagPrefix="ecf" %>

<div id="DataForm">
  <table class="DataForm"> 
   <tr>
     <td class="FormLabelCell"><asp:Label runat="server" Text="<%$ Resources:SharedStrings, Policy_Name %>"></asp:Label>:</td>
     <td class="FormFieldCell">
        <asp:TextBox runat="server" Width="250" ID="PolicyName" MaxLength="50"></asp:TextBox><br />
        <asp:Label CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Policy_Name_Enter %>"></asp:Label>
        <asp:RequiredFieldValidator runat="server" ID="PolicyNameRequired" ControlToValidate="PolicyName" Display="Dynamic" ErrorMessage="<%$ Resources:MarketingStrings, Policy_Name_Required %>" />
     </td>
   </tr>
   <tr>
     <td class="FormLabelCell"><asp:Label ID="Label2" runat="server" Text="<%$ Resources:MarketingStrings, Policy_Status %>"></asp:Label>:</td>
     <td class="FormFieldCell">
        <asp:TextBox runat="server" Width="250" ID="tbStatus" MaxLength="20"></asp:TextBox><br />
        <asp:Label ID="Label4" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Policy_Status_Enter %>"></asp:Label>
        <asp:RequiredFieldValidator runat="server" ID="PolicyStatusRequired" ControlToValidate="tbStatus" Display="Dynamic" ErrorMessage="<%$ Resources:MarketingStrings, Policy_Status_Required %>" />
     </td>
   </tr>
   <tr>
     <td class="FormLabelCell"><asp:Label ID="Label5" runat="server" Text="<%$ Resources:MarketingStrings, Policy_Is_Local %>"></asp:Label>:</td>
     <td class="FormFieldCell">
        <ecf:BooleanEditControl id="IsLocal" runat="server"></ecf:BooleanEditControl>
     </td>
   </tr>
   <tr>
     <td colspan="2" class="FormSpacerCell"></td> 
   </tr>
   <tr>
     <td class="FormLabelCell"><asp:Label ID="Label3" runat="server" Text="<%$ Resources:MarketingStrings, Policy_Expression %>"></asp:Label>:</td> 
     <td class="FormFieldCell">
         <asp:DropDownList ID="ddlExpression" runat="server" DataValueField="ExpressionId" DataTextField="Name"></asp:DropDownList><br />
         <asp:Label ID="Label6" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Policy_Expression_Select %>"></asp:Label>
         <asp:RequiredFieldValidator runat="server" ID="PolicyExpressionValidator" ControlToValidate="ddlExpression" Display="Dynamic" ErrorMessage="<%$ Resources:MarketingStrings, Policy_Expression_Required %>" />
     </td>
   </tr>
   <tr>
     <td colspan="2" class="FormSpacerCell"></td>
   </tr>
   
   <tr runat="server" id="PolicyGroupRow">
     <td class="FormLabelCell"><asp:Label ID="Label1" runat="server" Text="<%$ Resources:MarketingStrings, Policy_Groups %>"></asp:Label>:</td>
     <td class="FormFieldCell">
        <asp:ListBox runat="server" SelectionMode="Multiple" Height="60" Width="120" ID="GroupsListBox"></asp:ListBox>
        <br />
        <asp:Label ID="Label8" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Policy_Group_Select %>"></asp:Label>
     </td>
   </tr>
 </table>
</div>