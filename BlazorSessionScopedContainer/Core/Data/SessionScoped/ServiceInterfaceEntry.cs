using BlazorSessionScopedContainer.Contracts.SessionsScoped;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Core.Data.SessionScoped
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

        public override bool AreServicesEqual<Service>()
        {
            return typeof(Service).Equals(InterfaceType);
        }

        public override bool AreServicesEqual(Type type)
        {
            return type.Equals(InterfaceType);
        }
    }
}
