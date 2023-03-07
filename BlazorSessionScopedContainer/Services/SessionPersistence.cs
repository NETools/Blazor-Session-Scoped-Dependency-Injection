using BlazorSessionScopedContainer.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Services
{
    internal class SessionPersistence
    {
        public void SaveService(Guid? session, object instance)
        {
            var serviceType = instance.GetType();
            var serviceInterfaces = serviceType.GetInterfaces();
            if (serviceInterfaces.Contains(typeof(ISavedSessionScoped)))
            {
                var json = JsonSerializer.Serialize(instance);
                if (!Directory.Exists($"{System.IO.Directory.GetCurrentDirectory()}\\wwwroot\\{session.Value}"))
                    Directory.CreateDirectory($"{System.IO.Directory.GetCurrentDirectory()}\\wwwroot\\{session.Value}");

                File.WriteAllText($"{System.IO.Directory.GetCurrentDirectory()}\\wwwroot\\{session.Value}\\{serviceType.FullName}.json", json);
            }
        }
    }
}
