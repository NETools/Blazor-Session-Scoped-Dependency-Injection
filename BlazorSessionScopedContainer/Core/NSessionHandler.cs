using BlazorSessionScopedContainer.Attributes;
using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Core.Data;
using BlazorSessionScopedContainer.Misc;
using BlazorSessionScopedContainer.Services;
using System.Collections.Concurrent;
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

        public void AddService<T>(SessionId session, params object[] args) where T : class, ISessionScoped
        {
            if (session.Guid.HasValue)
            {
                if (!ServiceInstances.ContainsKey(session.Guid.Value))
                {
                    ServiceInstances.Add(session.Guid.Value, new List<IServiceEntry>());
                }

                if (!ServiceInstances[session.Guid.Value].Exists(p => p.IsEqual<T>()))
                {
                    ServiceInstances[session.Guid.Value].Add(new ServiceEntry<T>(this, session, args));
                }
            }
        }

        public void AddService<Interface, Concrete>(SessionId session, params object[] args) 
            where Interface : class, ISessionScoped 
            where Concrete : class, Interface
        {
            if (session.Guid.HasValue)
            {
                if (!ServiceInstances.ContainsKey(session.Guid.Value))
                {
                    ServiceInstances.Add(session.Guid.Value, new List<IServiceEntry>());
                }

                if (!ServiceInstances[session.Guid.Value].Exists(p => p.IsEqual<Interface>()))
                {
                    ServiceInstances[session.Guid.Value].Add(new ServiceInterfaceEntry<Interface, Concrete>(this, session, args));
                }
            }
        }

        public void RemoveService<T>(SessionId session) where T : class, ISessionScoped
        {
            if (session.Guid.HasValue)
            {
                if (!ServiceInstances.ContainsKey(session.Guid.Value))
                {
                    ServiceInstances.Add(session.Guid.Value, new List<IServiceEntry>());
                }

                var instance = ServiceInstances[session.Guid.Value].Find(p => p.IsEqual<T>());
                if (instance != null)
                {
                    ServiceInstances[session.Guid.Value].Remove(instance);
                }
            }
        }

        internal T GetInstance<T>(Guid? session, params object[] args) where T : class, ISessionScoped
        {
            var instanceType = typeof(T);
            var ctors = instanceType.GetConstructors();
            if (ctors.Length > 1)
            {
                throw new InvalidOperationException();
            }

            var ctor = ctors[0];
            var ctorParams = ctor.GetParameters();

            if (!session.HasValue)
                return default;

            T instance = default;
            List<object> dependencies = new List<object>();

            foreach (var param in ctorParams)
            {
                if (param.ParameterType.Equals(GetType()))
                {
                    dependencies.Add(this);
                }
                else
                {
                    var paramInterfaces = param.ParameterType.GetInterfaces();
                    if (paramInterfaces.Contains(typeof(ISessionScoped)))
                    {
                        var suitableService = ServiceInstances[session.Value].Find(p => p.IsEqual(param.ParameterType));

                        if (suitableService != null)
                            dependencies.Add(suitableService.GetInstance());
                    }
                }
            }

            instance = (T)Activator.CreateInstance(instanceType, dependencies.ToArray());

            var appropiateMethods = Helper.GetMethodsWithAttribute(instanceType, typeof(OnSessionInitialize));
            if (appropiateMethods.Any())
                appropiateMethods.First().Invoke(instance, args);

            return instance;
        }
    }
}
