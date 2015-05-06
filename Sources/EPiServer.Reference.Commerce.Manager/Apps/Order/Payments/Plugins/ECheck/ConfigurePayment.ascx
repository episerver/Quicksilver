<%@ Control Language="c#" Inherits="Mediachase.Commerce.Manager.Order.Payments.Plugins.ECheck.ConfigurePayment" Codebehind="ConfigurePayment.ascx.cs" %>
<div id="DataForm">
    <table cellpadding="0" cellspacing="2">
	    <tr>
		    <td class="FormLabelCell" colspan="2"><b><%$ Resources:OrderStrings, Payment_IBiz_E_Payment_Integrator_Configure  %></b></td>
	    </tr>
    </table>
    <br />
    <table class="DataForm">
	    <tr>
		    <td class="FormLabelCell"><%$ Resources:SharedStrings, Gateway %>:</td>
		    <td class="FormFieldCell">
		        <asp:UpdatePanel UpdateMode="Conditional" ID="GatewaysContentPanel" runat="server" RenderMode="Inline" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <asp:DropDownList AutoPostBack="true" Runat="server" ID="Gateways" Width="330px">
                            <asp:ListItem Value="" Text="<%$ Resources:OrderStrings, Payment_Pick_A_Gateway %>" />
                            <asp:ListItem Value="gwAuthorizeNet" Text="<%$ Resources:OrderStrings, Payment_Authorize_Net %>" />
                            <asp:ListItem Value="gwITransact" Text="<%$ Resources:OrderStrings, Payment_iTransact_RediCharge_HTML %>" />
                            <asp:ListItem Value="gwNetBilling" Text="<%$ Resources:OrderStrings, Payment_NetBilling %>" />
                            <asp:ListItem Value="gwUSAePay" Text="<%$ Resources:OrderStrings, Payment_USA_ePay %>" />
                            <asp:ListItem Value="gwPlanetPayment" Text="<%$ Resources:OrderStrings, Payment_Planet_Payment %>" />
                            <asp:ListItem Value="gwMPCS" Text="<%$ Resources:OrderStrings, Payment_MPCS %>" />
                            <asp:ListItem Value="gwRTWare" Text="<%$ Resources:OrderStrings, Payment_RTWare %>" />
                            <asp:ListItem Value="gwECX" Text="<%$ Resources:OrderStrings, Payment_ECX %>" />
                            <asp:ListItem Value="gwMerchantAnywhere" Text="<%$ Resources:OrderStrings, Payment_Merchant_Anywhere %>" />
                            <asp:ListItem Value="gwTrustCommerce" Text="<%$ Resources:OrderStrings, Payment_TrustCommerce %>" />
                            <asp:ListItem Value="gwFastTransact" Text="<%$ Resources:OrderStrings, Payment_Fast_Transact %>" />
                            <asp:ListItem Value="gwMerchantPartners" Text="<%$ Resources:OrderStrings, Payment_Merchant_Partners %>" />
                            <asp:ListItem Value="gwACHPAyments" Text="<%$ Resources:OrderStrings, Payment_ACH_Payments %>" />
                            <asp:ListItem Value="gwPaymentsGateway" Text="<%$ Resources:OrderStrings, Payment_Payments_Gateway %>" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator runat="server" ID="GatewayRequired" ControlToValidate="Gateways" ErrorMessage="*" Display="Dynamic"></asp:RequiredFieldValidator>
    			    </ContentTemplate>
			    </asp:UpdatePanel>
		    </td>
	    </tr>
	    <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
	    <tr>
		    <td class="FormLabelCell" colspan="2"><%$ Resources:OrderStrings, Payment_IBiz_E_Payment_Integrator_Get %></td>
	    </tr>
	    <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
	    <tr>
		    <td class="FormLabelCell" colspan="2"><br /><b><%$ Resources:SharedStrings, Configuration_Parameters %></b></td>
	    </tr>
    </table>
    <asp:UpdatePanel UpdateMode="Conditional" ID="GatewayParametersUpdatePanel" runat="server" RenderMode="Inline">
         <ContentTemplate>
            <table id="GenericTable" runat="server" class="DataForm">
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>