using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Extensions
{
    public static class OrderGroupExtensions
    {
        public static Money ToMoney(this OrderGroup orderGroup, decimal amount)
        {
            return new Money(amount, orderGroup.BillingCurrency);
        }
    }
}