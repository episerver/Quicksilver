<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Marketing.Tabs.SegmentEditTab" Codebehind="SegmentEditTab.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/BooleanEditControl.ascx" TagName="BooleanEditControl"
    TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/CalendarDatePicker.ascx" TagName="CalendarDatePicker"
    TagPrefix="ecf" %>
<%@ Register TagPrefix="ibn" Assembly="Mediachase.BusinessFoundation" Namespace="Mediachase.BusinessFoundation" %>    
<%@ Register src="../Modules/RulesEditor.ascx" tagname="RulesEditor" tagprefix="marketing" %>
<div id="DataForm">
  <table class="DataForm"> 
   <tr>  
     <td class="FormLabelCell"><asp:Label runat="server" Text="<%$ Resources:MarketingStrings, Segment_Name %>"></asp:Label>:</td> 
     <td class="FormFieldCell">
        <asp:TextBox runat="server" Width="250" ID="SegmentName" MaxLength="50"></asp:TextBox>
        <asp:RequiredFieldValidator runat="server" ID="SegmentNameRequired" ControlToValidate="SegmentName" Display="Dynamic" ErrorMessage="<%$ Resources:MarketingStrings, Segment_Name_Required %>" />
        <asp:CustomValidator runat="server" ID="NameCheckCustomValidator" ControlToValidate="SegmentName" OnServerValidate="NameCheck" Display="Dynamic" ErrorMessage="<%$ Resources:MarketingStrings, Segment_With_Name_Exists %>" />
        <br />
        <asp:Label ID="Label6" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Segment_Enter_Name %>"></asp:Label>
     </td> 
   </tr>   
   <tr>  
     <td colspan="2" class="FormSpacerCell"></td> 
   </tr>    
   <tr>  
     <td class="FormLabelCell"><asp:Label ID="Label4" runat="server" Text="<%$ Resources:MarketingStrings, Segment_Display_Name %>"></asp:Label>:</td> 
     <td class="FormFieldCell">
        <asp:TextBox runat="server" ID="DisplayName" Width="250" MaxLength="50"></asp:TextBox>
        <asp:RequiredFieldValidator runat="server" ID="DisplayNameRequired" ControlToValidate="DisplayName" Display="Dynamic" ErrorMessage="<%$ Resources:MarketingStrings, Segment_Display_Name_Required %>" />
        <br />
        <asp:Label ID="Label7" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Segment_Enter_Display_Name %>"></asp:Label>
     </td>  
   </tr> 
   <tr>  
     <td colspan="2" class="FormSpacerCell"></td> 
   </tr>    
   <tr>  
     <td class="FormLabelCell"><asp:Label ID="Label1" runat="server" Text="<%$ Resources:MarketingStrings, Segment_Description %>"></asp:Label>:</td> 
     <td class="FormFieldCell">
        <asp:TextBox runat="server" ID="Description" Rows="5" TextMode="MultiLine" Columns="60" MaxLength="100"></asp:TextBox>
     </td>
   </tr>
   <tr>  
     <td colspan="2" class="FormSpacerCell"></td> 
   </tr>
   <tr>  
     <td class="FormLabelCell"><asp:Label ID="Label2" runat="server" Text="<%$ Resources:MarketingStrings, Segment_Members %>"></asp:Label>:</td> 
     <td class="FormFieldCell">
         <ComponentArt:ComboBox id="MembershipFilter" TextBoxEnabled="false" runat="server" RunningMode="Callback" Width="250"
            AutoHighlight="false"
            AutoComplete="true"
            AutoFilter="true"
            CssClass="comboBox"
            HoverCssClass="comboBoxHover"
            FocusedCssClass="comboBoxHover"
            TextBoxCssClass="comboTextBox"
            TextBoxHoverCssClass="comboBoxHover"
            DropDownCssClass="comboDropDown"
            ItemCssClass="comboItem"
            ItemHoverCssClass="comboItemHover"
            SelectedItemCssClass="comboItemHover"
            DropHoverImageUrl="~/Apps/Shell/styles/images/combobox/drop_hover.gif"
            DropImageUrl="~/Apps/Shell/styles/images/combobox/drop.gif" />
         <asp:CheckBox runat="server" ID="Exclude" Text="<%$ Resources:MarketingStrings, Segment_Exclude_Member %>" />
         
         <asp:LinkButton CssClass="LinkButtonAddMember" runat="server" ID="AddMember" Text="<%$ Resources:MarketingStrings, Segment_Add_Member %>" CausesValidation="false"></asp:LinkButton> <br />
         <asp:DataList runat="server" ID="MemberList">
            <ItemTemplate>
                <asp:ImageButton runat="server" ID="DeleteButton" OnClientClick="isSubmit = true;" OnCommand="DeleteButton_Command" CausesValidation="false" ImageUrl="../images/delete.png" CommandArgument='<%# Eval("SegmentMemberId")%>' /> <%# GetPrincipalName((Guid)Eval("PrincipalId"))%> <%#GetStatus((bool)Eval("exclude"))%>
            </ItemTemplate>
         </asp:DataList>
     </td> 
   </tr>   
   <tr>  
     <td colspan="2" class="FormSpacerCell"></td> 
   </tr>
   <tr>  
     <td class="FormLabelCell"><asp:Label ID="Label5" runat="server" Text="<%$ Resources:MarketingStrings, Segment_Conditions %>"></asp:Label>:</td> 
     <td class="FormFieldCell">       
        
        <marketing:RulesEditor ID="RulesEditorCtrl" runat="server" />        
        <asp:Label ID="Label3" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings,Segment_Conditions_Description %>"></asp:Label>
     </td> 
   </tr>         
  
 </table>
</div>

