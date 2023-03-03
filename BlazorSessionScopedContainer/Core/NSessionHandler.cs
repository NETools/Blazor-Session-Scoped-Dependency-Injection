using BlazorSessionScopedContainer.Attributes;
using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Misc;
using BlazorSessionScopedContainer.Services;
using System.Collections.Concurrent;
namespace BlazorSessionScopedContainer.Core
{
    public class NSessionHandler
    {
        internal Dictionary<Guid, Dictionary<Type, Lazy<ISessionScoped>>> ServiceInstances { get; private set; } = new Dictionary<Guid, Dictionary<Type, Lazy<ISessionScoped>>>();
        internal Dictionary<Guid, DateTime> SessionLastActiveTime { get; private set; } = new Dictionary<Guid, DateTime>();
        internal HashSet<Guid> InitializedServices { get; private set; } = new HashSet<Guid>();
        internal GarbageCollection GarbageCollection { get; private set; }
        private NSessionHandler()
        {
            GarbageCollection = new GarbageCollection();    
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
                    ServiceInstances.Add(session.Guid.Value, new Dictionary<Type, Lazy<ISessionScoped>>());
                }

                if (!ServiceInstances[session.Guid.Value].ContainsKey(typeof(T)))
                {
                    ServiceInstances[session.Guid.Value].Add(typeof(T), new Lazy<ISessionScoped>(() =>
                    {
                        return GetInstance<T>(session.Guid, args);
                    }));
                }
            }
        }

        private T GetInstance<T>(Guid? session, params object[] args) where T : class, ISessionScoped
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
                        if (ServiceInstances[session.Value].ContainsKey(param.ParameterType))
                            dependencies.Add(ServiceInstances[session.Value][param.ParameterType].Value);
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
