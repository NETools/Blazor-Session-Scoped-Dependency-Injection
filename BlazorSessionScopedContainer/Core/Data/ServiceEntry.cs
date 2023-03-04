using BlazorSessionScopedContainer.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Core.Data
{
    internal class ServiceEntry<T> : IServiceEntry where T : class, ISessionScoped
    {
        private Lazy<T> _service;
        public Type ConcreteType { get; set; }
        public ServiceEntry(NSessionHandler handler, SessionId sessionId, object[] args)
        {
            ConcreteType = typeof(T);
            _service = new Lazy<T>(() =>
            {
                return handler.GetInstance<T>(sessionId.Guid, args);
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

        public ISessionScoped GetServiceInstance()
        {
            return _service.Value;
        }
    }
}
