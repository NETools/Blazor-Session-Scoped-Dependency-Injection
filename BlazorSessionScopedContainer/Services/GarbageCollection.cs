using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Core;
using BlazorSessionScopedContainer.Services.Data;
using BlazorSessionScopedContainer.Services.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Services
{
    public class NSessionGarbageCollector
    {
        private Timer _gcTimer;
        private int _timerPeriod = 60_000 * 5;

        /// <summary>
        /// In milliseconds
        /// </summary>
        public int TimerPeriod
        {
            get => _timerPeriod;
            set
            {
                if (_timerPeriod != value)
                {
                    _timerPeriod = value;
                    _gcTimer.Change(0, _timerPeriod);
                }
            }
        }

        internal NSessionGarbageCollector()
        {
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            _gcTimer = new Timer(new TimerCallback((o) =>
            {
                Collect();
            }), null, 0, _timerPeriod);
        }

        private void Collect()
        {

            foreach (var sessionService in NSessionHandler.Default().GlobalServices)
            {
                var serviceInstance = sessionService.GetServiceInstance();
                if (serviceInstance.GetType().GetInterfaces().Contains(typeof(IPersist)))
                {
                    SessionPersistence.SaveGlobal(serviceInstance);
                }
            }

            var oldSessions = NSessionHandler.Default().SessionLastActiveTime.Select(p => p).ToList().FindAll(p => (DateTime.Now - p.Value).TotalMilliseconds > _timerPeriod);
            for (int i = 0; i < oldSessions.Count; i++)
            {
                var entry = oldSessions[i];

                if (!NSessionHandler.Default().ServiceInstances.ContainsKey(entry.Key))
                    continue;

                var notificationService = ((UserNotificationService)NSessionHandler
                    .Default().ServiceInstances[entry.Key]
                    .Find(p => p.AreServicesEqual(typeof(UserNotificationService))).GetServiceInstance());

                NSessionHandler.Default().SessionLastActiveTime.Remove(entry.Key, out DateTime dateTime);
                NSessionHandler.Default().InitializedServices.Remove(entry.Key);
                NSessionHandler.Default().ServiceInstances.Remove(entry.Key, out List<IServiceEntry> loadedServiceSession);

                NSessionHandler.Default().Logger?.Invoke($"[*] Session closed for {entry}");
                if (loadedServiceSession != null)
                {
                    foreach (var sessionService in loadedServiceSession)
                    {
                        var serviceInstance = sessionService.GetServiceInstance();
                        if (serviceInstance.GetType().GetInterfaces().Contains(typeof(IPersist)))
                        {
                            SessionPersistence.SaveSession(entry.Key, serviceInstance);
                        }
                        ((IDisposable)serviceInstance).Dispose();
                    }
                    NSessionHandler.Default().Logger?.Invoke($"[**] Removed {loadedServiceSession.Count} services for session {entry.Key}");
                    loadedServiceSession.Clear();
                }

                notificationService.NotifyUser("The session has been closed. Refresh the page!", UserSessionNotification.SessionNotificationType.SessionClosed);

            }
        }

        public void ForceCollection()
        {
            Collect();
        }

    }
}
