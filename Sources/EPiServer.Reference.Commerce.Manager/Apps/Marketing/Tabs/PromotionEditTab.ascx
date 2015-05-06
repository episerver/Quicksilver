<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="Mediachase.Commerce.Manager.Marketing.Tabs.PromotionEditTab" Codebehind="PromotionEditTab.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/BooleanEditControl.ascx" TagName="BooleanEditControl"
    TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/CalendarDatePicker.ascx" TagName="CalendarDatePicker"
    TagPrefix="ecf" %>
<style type="text/css">
.ajax__validatorcallout_popup_table
{
	top: 55px;
	left:485px;
}
</style>
<script type="text/javascript">


function extendToolkitHidePopupTimer()
{
	if (AjaxControlToolkit && AjaxControlToolkit.PopupBehavior)
	{
		extendToolkitHidePopup();
	}
	else
	{
		window.setTimeout(extendToolkitHidePopup, 500);
	}
}

function extendToolkitHidePopup()
{
	AjaxControlToolkit.PopupBehavior.prototype.hide = function()
	{
		var eventArgs = new Sys.CancelEventArgs();
		this.raiseHiding(eventArgs);
		if (eventArgs.get_cancel()) {
			return;
		}

		// Either hide the popup or play an animation that does
		this._visible = false;
		if (this._onHide) {
			this.onHide();
		} else {
			this._hidePopup();
			this._hideCleanup();
		}
	}
}

extendToolkitHidePopupTimer();

</script>
<div id="DataForm">
    <table class="DataForm">
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label15" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Type %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:DropDownList runat="server" ID="PromotionTypeList" AutoPostBack="true" OnSelectedIndexChanged="PromotionTypeList_SelectedIndexChanged">
                        </asp:DropDownList>            
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>           
        <tr>
            <td class="FormLabelCell">
                <asp:Label runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Name %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="250" ID="PromotionName" MaxLength="50"></asp:TextBox>
                <br />
                <asp:Label ID="Label21" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Enter_Name %>"></asp:Label>
                <asp:RequiredFieldValidator runat="server" ID="PromotionNameRequired" ControlToValidate="PromotionName"
                    Display="Dynamic" ErrorMessage="<%$ Resources:MarketingStrings, Promotion_Name_Required %>" />
                <asp:CustomValidator runat="server" ID="NameCheckCustomValidator" ControlToValidate="PromotionName" OnServerValidate="NameCheck" Display="Dynamic" ErrorMessage="<%$ Resources:MarketingStrings, Promotion_With_Name_Exists %>" />
                <asp:RegularExpressionValidator runat="server" id="PromotionNameValidator" ControlToValidate="PromotionName" ErrorMessage="<%$ Resources:MarketingStrings, Promotion_Name_Invalid %>" Display="Dynamic" ValidationExpression="^[^@].*$" />
            </td>
        </tr>
        <tr>
            <td class="FormSectionCell" colspan="2">
                <asp:Label runat="server" ID="lblDisplayName" Text="<%$ Resources:SharedStrings, Display_Name %>"></asp:Label>:
            </td>
        </tr>
		<asp:Repeater ID="LanguagesList" runat="server">
		    <ItemTemplate>
		        <tr>
                    <td class="FormLabelCell">
                        <asp:Label ID="lblLanguageCode" runat="server" Text='<%# Eval("FriendlyName") %>'></asp:Label>
                    </td>
			        <td class="FormFieldCell" colspan="2">
			            <asp:HiddenField runat="server" ID="hfLangCode" Value='<%# Eval("LanguageCode") %>' />
			            <asp:TextBox runat="server" ID="tbDisplayName" Text='<%# Eval("DisplayName") %>' Width="350px" MaxLength="250"></asp:TextBox>
			        </td>
		        </tr>
		    </ItemTemplate>
		</asp:Repeater>       
        <tr>
            <td colspan="2" class="FormSectionCell"><asp:Literal ID="Literal4" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Properties %>"/>:</td>
        </tr>                 
        <tr>
            <td colspan="2">
            </td>
        </tr>                
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label14" runat="server" Text="<%$ Resources:SharedStrings, Campaign %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <ComponentArt:ComboBox ID="CampaignFilter" runat="server" TextBoxEnabled="false" Width="250" 
                    RunningMode="Callback"
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
                    DropImageUrl="~/Apps/Shell/styles/images/combobox/drop.gif"
                    OnSelectedIndexChanged="CampaignFilter_SelectedIndexChanged" 
                    AutoPostBack="true"></ComponentArt:ComboBox>
                <asp:CustomValidator ID="CampaignRequiredValidator" runat="server" ControlToValidate="CampaignFilter" 
                    Display="Dynamic" ErrorMessage="<%$ Resources:MarketingStrings, Campaign_Required %>" ValidateEmptyText="true" OnServerValidate="CampaignRequiredValidator_ServerValidate"></asp:CustomValidator>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>    
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label22" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Target_Markets %>"></asp:Label>:
            </td>
            <td> 
                <asp:ListBox runat="server" SelectionMode="Multiple" Height="150" Width="250" ID="TargetMarkets" DataMember="Market" DataTextField="MarketName" DataValueField="MarketId">
                </asp:ListBox>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>        
        <tr>
            <td class="FormLabelCell"><asp:Label ID="Label8" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Group %>"></asp:Label>:</td>
       
             <td class="FormFieldCell">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server" RenderMode="Block" UpdateMode="Always">
                    <ContentTemplate>
                        <asp:TextBox Enabled="false" runat="server" ID="PromotionGroup"></asp:TextBox>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </td>
        </tr>         
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>         
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label3" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Combination_With_Other%>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:DropDownList runat="server" ID="ExclusivityType">
                    <asp:ListItem Value="none" Text="<%$ Resources:MarketingStrings, Promotion_Combine_With_Other %>"></asp:ListItem>
                    <asp:ListItem Value="group" Text="<%$ Resources:MarketingStrings, Promotion_Exclusive_Groups_Selected %>"></asp:ListItem>
                    <asp:ListItem Value="global" Text="<%$ Resources:MarketingStrings, Promotion_Exclusive_Groups_All %>"></asp:ListItem>
                </asp:DropDownList><br />
                <asp:Label ID="Label12" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Select_Combine_Degree %>"></asp:Label>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label10" runat="server" Text="<%$ Resources:SharedStrings, Priority %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="50" ID="Priority"></asp:TextBox><br />
                <asp:Label ID="Label4" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Priority_Evaluation %>"></asp:Label>
                <asp:RangeValidator ID="PriorityRangeValidator" Type="Integer" runat="server" ControlToValidate="Priority" ErrorMessage="*"></asp:RangeValidator>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label7" runat="server" Text="<%$ Resources:SharedStrings, Coupon_Code %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="CouponCode" MaxLength="50"></asp:TextBox><br />
                <asp:Label ID="Label6" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Will_Require_Code %>"></asp:Label>
                <asp:RegularExpressionValidator runat="server" id="CouponNameValidator" ControlToValidate="CouponCode" ErrorMessage="<%$ Resources:MarketingStrings, Promotion_Name_Invalid %>" Display="Dynamic" ValidationExpression="^[-\w ]*$" />
                <asp:CustomValidator ID="ValidateUniqueCoupon" runat="server" ControlToValidate="CouponCode" Display="Dynamic" ErrorMessage="<%$ Resources:MarketingStrings, Coupon_Code_Existed %>" ValidateEmptyText="true" OnServerValidate="ValidateUniqueCoupon_ServerValidate"></asp:CustomValidator>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label5" runat="server" Text="<%$ Resources:SharedStrings, Status %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:DropDownList runat="server" ID="PromotionStatus">
                    <asp:ListItem Text="<%$ Resources:SharedStrings, Active %>" Value="active"></asp:ListItem>
                    <asp:ListItem Text="<%$ Resources:SharedStrings, Inactive %>" Value="inactive"></asp:ListItem>
                    <asp:ListItem Text="<%$ Resources:SharedStrings, Suspended %>" Value="suspended"></asp:ListItem>
                    <asp:ListItem Text="<%$ Resources:SharedStrings, Deleted %>" Value="deleted"></asp:ListItem>
                </asp:DropDownList><br />
                <asp:Label ID="Label13" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Status %>"></asp:Label>
            </td>
        </tr>              
        <tr>
            <td colspan="2" class="FormSectionCell"><asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Purchase_Condition_Reward %>"/>:</td>
        </tr>         
        <tr>
             <td colspan="2" class="FormFieldCell">
                <asp:UpdatePanel ID="ConfigurePromotionPanel" runat="server" RenderMode=Block UpdateMode="Always">
                    <ContentTemplate>
                        <asp:PlaceHolder runat="server" ID="PromotionConfigPlaceholder"></asp:PlaceHolder>
                    </ContentTemplate>
                    <Triggers>
                    </Triggers>
                </asp:UpdatePanel>
            </td>
        </tr>
        <tr>
            <td colspan="2">
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSectionCell"><asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Redemption_Limits %>"/>:</td>
        </tr>                        
        <tr>
            <td colspan="2">
            </td>
        </tr>                       
        <tr runat="server" visible="true">
            <td class="FormLabelCell"><asp:Label ID="Label9" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Max_Redemptions_Total %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="MaxTotalRedemptions" Text="0"></asp:TextBox>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="MaxTotalRedemptions" ErrorMessage="*" Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:RangeValidator runat="server" ControlToValidate="MaxTotalRedemptions" MinimumValue="0" MaximumValue="100000" Display="Dynamic" ErrorMessage="* [0-100000]" Type="Integer"></asp:RangeValidator>
                (<asp:Label ID="Label20" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Used_Count %>"></asp:Label>:<asp:Label runat="server" ID="MaxTotalRedemptionsUsed"></asp:Label>)
                <br /><asp:Label ID="Label17" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Use_All_Customers_0_Unlimited %>"></asp:Label>
            </td>
        </tr>
        <tr id="Tr2" runat="server" visible="true">
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell"><asp:Label ID="Label11" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Max_Redemptions_Per_Order %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="MaxOrderRedemptions" Text="0"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="MaxOrderRedemptions" ErrorMessage="*" Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:RangeValidator ID="RangeValidator1" runat="server" ControlToValidate="MaxOrderRedemptions" MinimumValue="0" MaximumValue="100000" Display="Dynamic" ErrorMessage="* [0-100000]" Type="Integer"></asp:RangeValidator>               
                <br /><asp:Label ID="Label18" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Use_Single_Order_0_Unlimited %>"></asp:Label>
            </td>
        </tr>
        <tr id="Tr3" runat="server" visible="true">
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr id="Tr1" runat="server" visible="true">
            <td class="FormLabelCell"><asp:Label ID="Label16" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Max_Redemptions_Per_Customer %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="MaxCustomerRedemptions" Text="0"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="MaxCustomerRedemptions" ErrorMessage="*" Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:RangeValidator ID="RangeValidator2" runat="server" ControlToValidate="MaxCustomerRedemptions" MinimumValue="0" MaximumValue="100000" Display="Dynamic" ErrorMessage="* [0-100000]" Type="Integer"></asp:RangeValidator>               
                <br /><asp:Label ID="Label19" CssClass="FormFieldDescription" runat="server" Text="<%$ Resources:MarketingStrings, Promotion_Use_Single_Customer_0_Unlimited %>"></asp:Label>
            </td>
        </tr>        
        <tr>
            <td colspan="2">
            </td>
        </tr>        
        <tr>
            <td colspan="2" class="FormSectionCell"><asp:Literal ID="Literal3" runat="server" Text="<%$ Resources:SharedStrings, Schedule %>"/>:</td>
        </tr>                        
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label1" runat="server" Text="<%$ Resources:SharedStrings, Available_From %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <ecf:CalendarDatePicker runat="server" ID="AvailableFrom" />
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label2" runat="server" Text="<%$ Resources:SharedStrings, Expires_On %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <ecf:CalendarDatePicker runat="server" ID="ExpiresOn" />
            </td>
        </tr>
    </table>
    <asp:CustomValidator EnableClientScript="false" ControlToValidate="AvailableFrom" ID="datetimeValidator" runat="server" OnServerValidate="DateTimeComparedValidator" ErrorMessage="<%$ Resources:MarketingStrings, Promotion_DateTime_Order %>"></asp:CustomValidator>
</div>
