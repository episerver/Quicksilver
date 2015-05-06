<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WorkflowEditTab.ascx.cs" Inherits="Mediachase.Commerce.Manager.Apps.Content.Workflow.Tabs.WorkflowEditTab" %>
<%@ Register Src="~/Apps/Core/Controls/BooleanEditControl.ascx" TagName="BooleanEditControl" TagPrefix="ecf" %>
<div id="DataForm">
 <table class="DataForm">
   <tr>
     <td class="FormLabelCell"><asp:Label ID="Literal5" runat="server" Text='<%$ Resources:SharedStrings, Name %>'></asp:Label>:</td>
     <td class="FormFieldCell">
         <asp:TextBox runat="server" Width="250" ID="Name"></asp:TextBox>
         <asp:RequiredFieldValidator runat="server" ID="valName" ControlToValidate="Name" ErrorMessage="<%$ Resources:SharedStrings, Name_Required %>" Display="Dynamic"></asp:RequiredFieldValidator>
     </td>
   </tr>
   <tr>
     <td colspan="2" class="FormSpacerCell"></td> 
   </tr>
   <tr id="DefaultRow" runat="server">
     <td class="FormLabelCell"><asp:Label ID="lblIsDefault" runat="server" Text="<%$ Resources:SharedStrings, IsDefault %>"></asp:Label>:</td>
     <td class="FormFieldCell"><ecf:BooleanEditControl id="IsDefault" runat="server"></ecf:BooleanEditControl></td>
   </tr>
   <tr>
     <td colspan="2" class="FormSpacerCell"></td> 
   </tr>
 </table>
</div>