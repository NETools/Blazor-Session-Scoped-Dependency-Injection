using BlazorSessionScopedContainer.Attributes;
using BlazorSessionScopedContainer.Contracts.GlobalScoped;
using BlazorSessionScopedContainer.Contracts.SessionsScoped;
using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Core.Data.SessionScoped;
using BlazorSessionScopedContainer.Misc;
using BlazorSessionScopedContainer.Services.Persistence.Json;
using BlazorSessionScopedContainer.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Core
{
    public partial class NSessionHandler
    {
        public void AddService<T>(SessionId session, params object[] args) where T : class, ISessionScoped
        {
            if (PrepareServiceHandler(session))
            {
                if (!ServiceInstances[session.Guid.Value].Exists(p => p.AreServicesEqual<T>()))
                {
                    ServiceInstances[session.Guid.Value].Add(new ServiceEntry<T>(this, session, args));
                }
            }
        }

        public void AddService<Interface, Concrete>(SessionId session, params object[] args)
            where Interface : class, ISessionScoped
            where Concrete : class, Interface
        {
            if (PrepareServiceHandler(session))
            {
                if (!ServiceInstances[session.Guid.Value].Exists(p => p.AreServicesEqual<Interface>()))
                {
                    ServiceInstances[session.Guid.Value].Add(new ServiceInterfaceEntry<Interface, Concrete>(this, session, args));
                }
            }
        }

        private object[] GetSessionScopedConstructorDependencies(Guid? session, Type currentType)
        {
            var ctors = currentType.GetConstructors();
            if (ctors.Length > 1)
            {
                throw new InvalidOperationException();
            }

            var ctor = ctors[0];
            var ctorParams = ctor.GetParameters();

            if (!session.HasValue)
                return default;

            List<object> dependencies = new List<object>();

            foreach (var param in ctorParams)
            {
                var paramInterfaces = param.ParameterType.GetInterfaces();
                if (paramInterfaces.Contains(typeof(ISessionScoped)))
                {
                    if (ServiceInstances.ContainsKey(session.Value))
                    {
                        var suitableService = ServiceInstances[session.Value].Find(p => p.AreServicesEqual(param.ParameterType));
                        if (suitableService != null)
                            dependencies.Add(suitableService.GetServiceInstance());
                    }
                    else dependencies.Add(null);
                }
                else if (paramInterfaces.Contains(typeof(IGlobalScoped)))
                {
                    var suitableService = GlobalServices.Find(p => p.AreServicesEqual(param.ParameterType));
                    if (suitableService != null)
                        dependencies.Add(suitableService.GetServiceInstance());
                }
            }

            return dependencies.ToArray();
        }

        internal T GetSessionInstance<T>(Guid? session, params object[] args)
        {
            var instanceType = typeof(T);
            var dependencies = GetSessionScopedConstructorDependencies(session, instanceType);

            T instance = (T)Activator.CreateInstance(instanceType, dependencies.ToArray());


            RestoreSessionInstance<T>(session, instance);

            var appropiateMethods = Helper.GetMethodsWithAttribute(instanceType, typeof(OnSessionInitialize));
            if (appropiateMethods.Any())
                appropiateMethods.First().Invoke(instance, args);

            return instance;
        }

        private void RestoreSessionInstance<T>(Guid? session, object instance)
        {
            var instanceType = typeof(T);
            if (instanceType.GetInterfaces().Contains(typeof(IPersist)))
            {
                if (instanceType.GetInterfaces().Contains(typeof(ISessionScoped)))
                {
                    var json = SessionPersistence.RetrieveSession<T>(session);
                    if (json != null)
                    {
                        NJson.DeserializeIntoInstance(json, instance, (p) =>
                        {
                            NJson.NJsonInstanciatorResult instanciatorResult = new NJson.NJsonInstanciatorResult();
                            instanciatorResult.Code = NJson.NJsonInstanciatorResultCode.Failed;

                            if (p.PropertyType.GetInterfaces().Contains(typeof(ISessionScoped)))
                            {
                                if (ServiceInstances.ContainsKey(session.Value))
                                {
                                    instanciatorResult.Code = NJson.NJsonInstanciatorResultCode.Success;
                                    instanciatorResult.Value = ServiceInstances[session.Value].Find(s => s.AreServicesEqual(p.PropertyType)).GetServiceInstance();
                                }
                                else
                                {
                                    instanciatorResult.Code = NJson.NJsonInstanciatorResultCode.SessionUnknown;
                                    instanciatorResult.Value = null;
                                }
                            }
                            else if (p.PropertyType.GetInterfaces().Contains(typeof(IGlobalScoped)))
                            {
                                instanciatorResult.Code = NJson.NJsonInstanciatorResultCode.Success;
                                instanciatorResult.Value = GlobalServices.Find(s => s.AreServicesEqual(p.PropertyType)).GetServiceInstance();
                            }

                            return instanciatorResult;
                        });
                    }
                }
            }
        }
    }
}
