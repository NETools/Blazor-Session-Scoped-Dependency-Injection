using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Contracts
{
    internal interface IServiceEntry 
    {
        bool IsEqual<Service>();
        bool IsEqual(Type type);

        ISessionScoped GetInstance();
    }
}
