<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DIBSPayment.aspx.cs" Inherits="EPiServer.Business.Commerce.Payment.DIBS.DIBSPayment" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Redirecting to DIBS provider</title>
</head>
<body>
    <h1>Redirecting to DIBS provider ...</h1>
    <img src="https://cdn.dibspayment.com/logo/checkout/combo/horiz/DIBS_checkout_kombo_horizontal_14.png" />
    <form id="paymentForm" target="_top" method="post" runat="server" >
        <input type="hidden" name="paymentprovider" value="<%= DIBSSystemName %>" />
        <input type="hidden" name="merchant" value="<%=MerchantID%>" />
        <input type="hidden" name="amount" value="<%=Amount%>" />
        <input type="hidden" name="currency" value="<%= Currency %>" />
        <input type="hidden" name="orderid" value="<%=OrderID%>" />
        <input type="hidden" name="uniqueoid" value="<%=OrderID%>" />
        <input type="hidden" name="accepturl" value="<%=CallbackUrl%>" />
        <input type="hidden" name="cancelurl" id="cancelurl" runat="server" clientidmode="Static"/>
        <input type="hidden" name="test" value="foo" />
        <input type="hidden" name="HTTP_COOKIE" value="<%=Request.ServerVariables["HTTP_COOKIE"]%>" />
        <input type="hidden" name="lang" value="<%=Language %>" />
        <input type="hidden" name="md5key" value="<%=MD5Key%>" />
        <input type="hidden" name="voucher" value="yes" />
    </form>
</body>
</html>
