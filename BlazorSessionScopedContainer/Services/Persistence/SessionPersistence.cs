using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Core;
using BlazorSessionScopedContainer.Services.Persistence.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Services.Persistence
{
    internal static class SessionPersistence
    {
        public static string RootFolder { get; set; } = $"{Directory.GetCurrentDirectory()}\\wwwroot\\sessions";

        public static void SaveService(Guid? session, object instance)
        {
            var serviceType = instance.GetType();
            var serviceInterfaces = serviceType.GetInterfaces();
            if (serviceInterfaces.Contains(typeof(IPersistentSessionScoped)))
            {
                var json = NJson.SerializeInstance(instance);
                if (!Directory.Exists($"{RootFolder}\\{session.Value}"))
                    Directory.CreateDirectory($"{RootFolder}\\{session.Value}");

                File.WriteAllText($"{RootFolder}\\{session.Value}\\{serviceType.FullName}.json", json);
            }
        }

        public static string RetrieveSession<T>(Guid? session) where T : class, ISessionScoped
        {
            if (File.Exists($"{RootFolder}\\{session.Value}\\{typeof(T).FullName}.json"))
            {
                return File.ReadAllText($"{RootFolder}\\{session.Value}\\{typeof(T).FullName}.json");
            }
            return null;
        }
    }
}
