using Mediachase.Commerce.Website;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Models
{
    public interface IPaymentMethodViewModel<out T> where T : IPaymentOption
    {
        T PaymentMethod { get; }
    }
}
