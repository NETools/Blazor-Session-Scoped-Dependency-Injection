using BlazorSessionScopedContainer.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Core.Data
{
    internal class ServiceInterfaceEntry<Interface, Concrete> : ServiceEntry<Concrete>
        where Interface : class, ISessionScoped
        where Concrete : class, Interface
    {
        Type InterfaceType { get; set; }
        public ServiceInterfaceEntry(NSessionHandler handler, SessionId sessionId, object[] args) : base(handler, sessionId, args)
        {
            InterfaceType = typeof(Interface);
        }

        public override bool IsEqual<Service>()
        {
            return typeof(Service).Equals(InterfaceType);
        }

        public override bool IsEqual(Type type)
        {
            return type.Equals(InterfaceType);
        }
    }
}
