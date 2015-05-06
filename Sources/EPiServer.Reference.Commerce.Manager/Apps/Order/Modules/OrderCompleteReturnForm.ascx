<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OrderCompleteReturnForm.ascx.cs" Inherits="Mediachase.Commerce.Manager.Apps.Order.Modules.OrderCompleteReturnForm" %>
<%@ Register TagPrefix="mc2" Assembly="Mediachase.BusinessFoundation" Namespace="Mediachase.BusinessFoundation" %>
<%@ Register Src="~/Apps/Order/Modules/PaymentTransactionTypeControl.ascx" TagName="TransactionTypeControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Order/Modules/PaymentMethodSelectControl.ascx" TagName="PaymentMethodSelectControl" TagPrefix="ecf" %>

<div style="padding: 5px; height: 474px; overflow:auto;">
	<asp:Label runat="server" ID="lblErrorInfo" Style="color: Red"></asp:Label>
	<table cellpadding="0" cellspacing="0" width="100%">
		<tr>
			<td style="padding: 7px">
				<table class="DataForm" width="100%">
					<col width="150" />
					<tr>
						<td class="FormLabelCell">
							<asp:Label ID="Label1" runat="server" Text="<%$ Resources:OrderStrings, Amount %>"></asp:Label>:
						</td>
						<td class="FormFieldCell">
							<asp:TextBox runat="server" ID="tbAmount" Width="150"></asp:TextBox>
							<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ValidationGroup="PaymentMetaData" ControlToValidate="tbAmount" ErrorMessage="*"></asp:RequiredFieldValidator>
							<asp:RangeValidator ID="RangeValidator1" ValidationGroup="PaymentMetaData" runat="server" ControlToValidate="tbAmount" ErrorMessage="*" Type="Double" MinimumValue="0" MaximumValue="1000000"></asp:RangeValidator>
						</td>
					</tr>					
				</table>
			</td>
		</tr>
	</table>	
	<ecf:TransactionTypeControl runat="server" ID="TransactionTypeControl1" TranType="Credit" />
	<ecf:PaymentMethodSelectControl runat="server" ID="PaymentMethodSelectControl1" UseAmountControl="false" />
	<table width="100%">
		<tr>
			<td align="right" colspan="2" style="padding-top: 10px; padding-right: 10px;">
				<mc2:IMButton ID="btnSave" runat="server" class="btn-save" style="width: 105px;" OnServerClick="btnSave_ServerClick" Text="<%$ Resources:Common, btnOK %>">
				</mc2:IMButton>
				&nbsp;
				<mc2:IMButton ID="btnCancel" runat="server" class="btn-save" style="width: 105px;" CausesValidation="false" OnServerClick="btnCancel_ServerClick"  Text="<%$ Resources:Common, btnCancel %>">
				</mc2:IMButton>
			</td>
		</tr>
	</table>
</div>