using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Core;
using BlazorSessionScopedContainer.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Services
{
    internal class GarbageCollection
    {
        private Timer _gcTimer;

        internal GarbageCollection()
        {
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            _gcTimer = new Timer(new TimerCallback((o) =>
            {
                OnTick();
            }), null, 0, 60_000 * 5);
        }

        private void OnTick()
        {
            var oldSessions = NSessionHandler.Default().SessionLastActiveTime.Select(p => p).ToList().FindAll(p => (DateTime.Now - p.Value).Minutes > 5);
            for (int i = 0; i < oldSessions.Count; i++)
            {
                var entry = oldSessions[i];

                ((UserNotificationService)NSessionHandler.Default().ServiceInstances[entry.Key][typeof(UserNotificationService)].Value).NotifyUser("The session has been closed. Refresh the page!", UserSessionNotification.SessionNotificationType.SessionClosed);

                NSessionHandler.Default().SessionLastActiveTime.Remove(entry.Key, out DateTime dateTime);
                NSessionHandler.Default().InitializedServices.Remove(entry.Key);
                NSessionHandler.Default().ServiceInstances.Remove(entry.Key, out Dictionary<Type, Lazy<ISessionScoped>> loadedServiceSession);

                Console.WriteLine($"[*] Session closed for {entry}");
                if (loadedServiceSession != null)
                {
                    foreach (var item in loadedServiceSession)
                    {
                        item.Value.Value.Dispose();
                    }
                    Console.WriteLine($"[**] Removed {loadedServiceSession.Count} services for session {entry.Key}");
                    loadedServiceSession.Clear();
                }
            }
        }

    }
}
