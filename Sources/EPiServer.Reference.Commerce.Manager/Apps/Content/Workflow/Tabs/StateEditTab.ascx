<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StateEditTab.ascx.cs" Inherits="Mediachase.Commerce.Manager.Apps.Content.Workflow.Tabs.StateEditTab" %>
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
   <tr>  
     <td class="FormLabelCell"><asp:Label ID="Label4" runat="server" Text="<%$ Resources:SharedStrings, Weight %>"></asp:Label>:</td> 
     <td class="FormFieldCell">
        <asp:TextBox runat="server" Width="50" ID="tbWeight"></asp:TextBox>
        <asp:RequiredFieldValidator runat="server" ID="rfvWeight" ControlToValidate="tbWeight" ErrorMessage="<%$ Resources:SharedStrings, Weight_Required %>" Display="Dynamic"></asp:RequiredFieldValidator>
        <asp:RangeValidator runat="server" ID="rvWeightRange" Type="Integer" MinimumValue="-1" MaximumValue="100" ControlToValidate="tbWeight" ErrorMessage="<%$ Resources:SharedStrings, Weight_Range_Warning %>" Display="Dynamic"></asp:RangeValidator>
     </td>
   </tr>
   <tr>  
     <td colspan="2" class="FormSpacerCell"></td> 
   </tr>
   <tr id="RolesRow" runat="server">
     <td class="FormLabelCell" nowrap><asp:Label ID="Literal1" runat="server" Text='<%$ Resources:ContentStrings, Folder_FullAccess %>'></asp:Label>:</td>
     <td class="FormFieldCell"><asp:CheckBoxList runat="server" ID="RolesList" RepeatColumns="3"></asp:CheckBoxList></td>
   </tr> 
   <tr>
     <td colspan="2" class="FormSpacerCell"></td> 
   </tr>
 </table>
</div>