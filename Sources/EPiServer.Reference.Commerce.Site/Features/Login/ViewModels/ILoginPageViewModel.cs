using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    public interface ILoginPageViewModel<out T> where T : PageData
    {
        T CurrentPage { get; }
    }
}
