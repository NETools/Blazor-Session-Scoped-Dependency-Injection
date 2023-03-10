using BlazorSessionScopedContainer.Attributes;
using BlazorSessionScopedContainer.Contracts.GlobalScoped;
using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Misc;
using BlazorSessionScopedContainer.Services.Persistence.Json;
using BlazorSessionScopedContainer.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorSessionScopedContainer.Core.Data.GlobalScoped;

namespace BlazorSessionScopedContainer.Core
{
    public partial class NSessionHandler
    {

        public void AddGlobalService<T>(params object[] args) where T : class, IGlobalScoped
        {
            if (!GlobalServices.Exists(p => p.AreServicesEqual<T>()))
            {
                GlobalServices.Add(new GlobalServiceEntry<T>(this, args));
            }
        }

        public void AddGlobalService<Interface, Concrete>(params object[] args)
            where Interface : class, IGlobalScoped
            where Concrete : class, Interface
        {
            if (!GlobalServices.Exists(p => p.AreServicesEqual<Interface>()))
            {
                GlobalServices.Add(new GlobalServiceInterfaceEntry<Interface, Concrete>(this, args));
            }
        }
        private void RestoreGlobalInstance<T>(object instance)
        {
            var instanceType = typeof(T);

            if (instanceType.GetInterfaces().Contains(typeof(IPersist)))
            {
                if (instanceType.GetInterfaces().Contains(typeof(IGlobalScoped)))
                {
                    var json = SessionPersistence.RetrieveGlobal<T>();
                    if (json != null)
                    {
                        NJson.DeserializeIntoInstance(json, instance, (p) =>
                        {
                            NJson.NJsonInstanciatorResult instanciatorResult = new NJson.NJsonInstanciatorResult();
                            instanciatorResult.Code = NJson.NJsonInstanciatorResultCode.Failed;

                            if (p.PropertyType.GetInterfaces().Contains(typeof(IGlobalScoped)))
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

        private object[] GetGlobalConstructorDependencies(Type currentType)
        {
            var ctors = currentType.GetConstructors();
            if (ctors.Length > 1)
            {
                throw new InvalidOperationException();
            }

            var ctor = ctors[0];
            var ctorParams = ctor.GetParameters();

            List<object> dependencies = new List<object>();

            foreach (var param in ctorParams)
            {
                var paramInterfaces = param.ParameterType.GetInterfaces();

                if (paramInterfaces.Contains(typeof(IGlobalScoped)))
                {
                    var suitableService = GlobalServices.Find(p => p.AreServicesEqual(param.ParameterType));
                    if (suitableService != null)
                        dependencies.Add(suitableService.GetServiceInstance());
                }
            }

            return dependencies.ToArray();
        }

        internal T GetGlobalInstance<T>(params object[] args)
        {
            var instanceType = typeof(T);
            var dependencies = GetGlobalConstructorDependencies(instanceType);

            T instance = (T)Activator.CreateInstance(instanceType, dependencies.ToArray());

            RestoreGlobalInstance<T>(instance);

            var appropiateMethods = Helper.GetMethodsWithAttribute(instanceType, typeof(OnSessionInitialize));
            if (appropiateMethods.Any())
                appropiateMethods.First().Invoke(instance, args);


            return instance;
        }

    }
}
