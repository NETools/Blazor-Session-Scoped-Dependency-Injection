using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Services.Data;
using static BlazorSessionScopedContainer.Services.Data.UserSessionNotification;

namespace BlazorSessionScopedContainer.Services
{
    internal class UserNotificationService : ISessionScoped
    {
        public event Action<UserSessionNotification> OnNotified;

        public void NotifyUser(string message, SessionNotificationType sessionNotificationType)
        {
            OnNotified?.Invoke(new UserSessionNotification() { Message = message, NotificationType = sessionNotificationType });
        }

        public void Dispose()
        {

        }
    }
}
