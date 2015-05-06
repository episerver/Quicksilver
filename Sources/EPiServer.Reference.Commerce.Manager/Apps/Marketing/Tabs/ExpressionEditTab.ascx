<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Marketing.Tabs.ExpressionEditTab" Codebehind="ExpressionEditTab.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/BooleanEditControl.ascx" TagName="BooleanEditControl" TagPrefix="ecf" %>
<div id="DataForm">
  <table class="DataForm">
   <tr>
     <td class="FormLabelCell"><asp:Label runat="server" Text="<%$ Resources:MarketingStrings, Expression_Name %>"></asp:Label>:</td>
     <td class="FormFieldCell">
        <asp:TextBox runat="server" Width="250" ID="tbExpressionName" MaxLength="50"></asp:TextBox><br />
        <asp:Label CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Expression_Name_Enter %>"></asp:Label>
        <asp:RequiredFieldValidator runat="server" ID="ExpressionNameRequired" ControlToValidate="tbExpressionName" Display="Dynamic" ErrorMessage="<%$ Resources:MarketingStrings, Expression_Name_Required %>" />
     </td>
   </tr>
   <tr>
     <td colspan="2" class="FormSpacerCell"></td>
   </tr>
   <tr>
     <td class="FormLabelCell"><asp:Label ID="Label1" runat="server" Text="<%$ Resources:SharedStrings, Description %>"></asp:Label>:</td>
     <td class="FormFieldCell">
        <asp:TextBox runat="server" ID="tbDescription" Rows="10" TextMode="MultiLine" Columns="150" MaxLength="50"></asp:TextBox><br />
        <asp:Label ID="Label2" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Expression_Description_Enter %>"></asp:Label>
     </td>
   </tr>
   <tr>
     <td colspan="2" class="FormSpacerCell"></td>
   </tr>
   <tr>
     <td class="FormLabelCell"><asp:Label runat="server" Text="<%$ Resources:MarketingStrings, Expression_Xml %>"></asp:Label>:</td>
     <td class="FormFieldCell">
        <asp:TextBox runat="server" ID="tbXml" Rows="20" TextMode="MultiLine" Columns="150"></asp:TextBox><br />
        <asp:Label ID="Label3" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Expression_Xml_Enter %>"></asp:Label>
     </td>
   </tr>
   <tr>
     <td colspan="2" class="FormSpacerCell"></td>
   </tr>

   <tr runat="server" id="ExpressionCategoryRow" visible="false">
     <td class="FormLabelCell"><asp:Label runat="server" Text="<%$ Resources:SharedStrings, Category %>"></asp:Label>:</td>
     <td class="FormFieldCell">
        <asp:DropDownList runat="server" ID="ddlExpressionCategory">
        </asp:DropDownList><br />
        <asp:Label ID="Label8" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Expression_Category_Select %>"></asp:Label>
     </td>
   </tr>

 </table>
</div>