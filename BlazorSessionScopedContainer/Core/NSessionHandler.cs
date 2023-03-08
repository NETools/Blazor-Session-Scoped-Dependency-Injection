using BlazorSessionScopedContainer.Attributes;
using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Core.Data;
using BlazorSessionScopedContainer.Misc;
using BlazorSessionScopedContainer.Services;
using BlazorSessionScopedContainer.Services.Persistence;
using BlazorSessionScopedContainer.Services.Persistence.Json;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace BlazorSessionScopedContainer.Core
{
    public class NSessionHandler
    {
        internal Dictionary<Guid, List<IServiceEntry>> ServiceInstances { get; private set; } = new Dictionary<Guid, List<IServiceEntry>>();
        internal Dictionary<Guid, DateTime> SessionLastActiveTime { get; private set; } = new Dictionary<Guid, DateTime>();
        internal HashSet<Guid> InitializedServices { get; private set; } = new HashSet<Guid>();
        public NSessionGarbageCollection GarbageCollection { get; private set; }

        public Action<string> Logger { get; set; } = (message) =>
        {
            Console.WriteLine(message);
        };

        private NSessionHandler()
        {
            GarbageCollection = new NSessionGarbageCollection();
        }

        private static NSessionHandler _sessionHandler;
        internal static NSessionHandler Default()
        {
            if (_sessionHandler == null)
            {
                _sessionHandler = new NSessionHandler();

            }
            return _sessionHandler;
        }


        private bool PrepareServiceHandler(SessionId session)
        {
            if (session.Guid.HasValue)
            {
                if (!ServiceInstances.ContainsKey(session.Guid.Value))
                {
                    ServiceInstances.Add(session.Guid.Value, new List<IServiceEntry>());
                }

                return true;
            }

            return false;
        }

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

        public void RemoveService<T>(SessionId session) where T : class, ISessionScoped
        {
            if (PrepareServiceHandler(session))
            {
                var instance = ServiceInstances[session.Guid.Value].Find(p => p.AreServicesEqual<T>());
                if (instance != null)
                {
                    instance.GetServiceInstance().Dispose();
                    ServiceInstances[session.Guid.Value].Remove(instance);
                }
            }
        }

        private object[] GetConstructorDependencies(Guid? session, Type currentType)
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
            }

            return dependencies.ToArray();
		}

        internal T GetInstance<T>(Guid? session, params object[] args)
        {
            var instanceType = typeof(T);
            var dependencies = GetConstructorDependencies(session, instanceType);

            T instance = (T)Activator.CreateInstance(instanceType, dependencies.ToArray());

            if (instanceType.GetInterfaces().Contains(typeof(IPersistentSessionScoped)))
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
                                if (ServiceInstances.ContainsKey(session.Value))
                                {
                                    instanciatorResult.Value = ServiceInstances[session.Value].Find(s => s.AreServicesEqual(p.PropertyType)).GetServiceInstance();
                                }
                            }
                            else
                            {
                                instanciatorResult.Code = NJson.NJsonInstanciatorResultCode.SessionUnknown;
                                instanciatorResult.Value = null;
                            }

                            return instanciatorResult;
                        }

                        return instanciatorResult;
                    });
                }
            }

            var appropiateMethods = Helper.GetMethodsWithAttribute(instanceType, typeof(OnSessionInitialize));
            if (appropiateMethods.Any())
                appropiateMethods.First().Invoke(instance, args);

            return instance;
        }
    }
}
