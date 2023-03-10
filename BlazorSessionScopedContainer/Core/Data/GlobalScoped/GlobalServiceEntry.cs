using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Contracts.GlobalScoped;
using BlazorSessionScopedContainer.Contracts.SessionsScoped;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Core.Data.GlobalScoped
{
    internal class GlobalServiceEntry<T> : IServiceEntry where T : class, IGlobalScoped
    {
        private Lazy<T> _service;
        public Type ConcreteType { get; set; }

        public GlobalServiceEntry(NSessionHandler handler, object[] args)
        {
            ConcreteType = typeof(T);

            _service = new Lazy<T>(() =>
            {
                return handler.GetGlobalInstance<T>(args);
            });
        }
        public virtual bool AreServicesEqual<Service>()
        {
            return typeof(Service).Equals(ConcreteType);
        }

        public virtual bool AreServicesEqual(Type type)
        {
            return type.Equals(ConcreteType);
        }

        public object GetServiceInstance()
        {
            return _service.Value;
        }
    }
}
