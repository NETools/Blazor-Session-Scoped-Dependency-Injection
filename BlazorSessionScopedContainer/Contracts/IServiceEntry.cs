using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Contracts
{
    internal interface IServiceEntry 
    {
        bool AreServicesEqual<Service>();
        bool AreServicesEqual(Type type);
        ISessionScoped GetServiceInstance();
    }
}
