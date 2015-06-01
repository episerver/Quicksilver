using Mediachase.Commerce.Website;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Services
{
    public interface IPaymentService
    {
        void ProcessPayment(IPaymentOption method);
    }
}
