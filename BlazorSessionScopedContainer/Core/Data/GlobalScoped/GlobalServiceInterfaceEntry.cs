using BlazorSessionScopedContainer.Contracts.GlobalScoped;
using BlazorSessionScopedContainer.Core.Data.SessionScoped;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Core.Data.GlobalScoped
{
    internal class GlobalServiceInterfaceEntry<Interface, Concrete> : GlobalServiceEntry<Concrete>
        where Interface : class, IGlobalScoped
        where Concrete : class, Interface
    {
        Type InterfaceType { get; set; }
        public GlobalServiceInterfaceEntry(NSessionHandler handler, object[] args) : base(handler, args)
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
