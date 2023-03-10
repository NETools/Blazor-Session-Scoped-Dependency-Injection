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
    public static class SessionPersistence
    {
        public static string RootFolderSessions { get; set; } = $"{Directory.GetCurrentDirectory()}\\sessions";
        public static string RootFolderGlobal { get; set; } = $"{Directory.GetCurrentDirectory()}\\globals";

        internal static void SaveSession(Guid? session, object instance)
        {
            var serviceType = instance.GetType();
            var json = NJson.SerializeInstance(instance);
            if (!Directory.Exists($"{RootFolderSessions}\\{session.Value}"))
                Directory.CreateDirectory($"{RootFolderSessions}\\{session.Value}");
            File.WriteAllText($"{RootFolderSessions}\\{session.Value}\\{serviceType.FullName}.json", json);
        }

        internal static string RetrieveSession<T>(Guid? session) 
        {
            if (File.Exists($"{RootFolderSessions}\\{session.Value}\\{typeof(T).FullName}.json"))
            {
                return File.ReadAllText($"{RootFolderSessions}\\{session.Value}\\{typeof(T).FullName}.json");
            }
            return null;
        }

        internal static void SaveGlobal(object instance)
        {
            var serviceType = instance.GetType();
            var json = NJson.SerializeInstance(instance);
            if (!Directory.Exists($"{RootFolderGlobal}"))
                Directory.CreateDirectory($"{RootFolderGlobal}");
            File.WriteAllText($"{RootFolderGlobal}\\{serviceType.FullName}.json", json);
        }

        internal static string RetrieveGlobal<T>()
        {
            if (File.Exists($"{RootFolderGlobal}\\{typeof(T).FullName}.json"))
            {
                return File.ReadAllText($"{RootFolderGlobal}\\{typeof(T).FullName}.json");
            }
            return null;
        }


    }
}
