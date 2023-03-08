using BlazorSessionScopedContainer.Core;
using BlazorSessionScopedContainer.Services;
using BlazorSessionScopedContainer.Services.Data;
using Microsoft.AspNetCore.Components;

namespace BlazorSessionScopedContainer.RazorComponents
{
    public abstract class NSessionMainBase : LayoutComponentBase, IDisposable
    {
        private UserNotificationService _sessionNotification;

        [Inject]
        public NSession Session { get; set; }

        public bool IsSessionClosed { get; private set; }

        protected virtual void NotificationReceived(UserSessionNotification sessionNotification)
        {
            switch (sessionNotification.NotificationType)
            {
                case UserSessionNotification.SessionNotificationType.SessionClosed:
                    IsSessionClosed = true;
                    break;
            }
            StateHasChanged();
        }

        private void HandleNotification(UserSessionNotification sessionNotification)
        {
            InvokeAsync(() =>
            {
                NotificationReceived(sessionNotification);
            });
        }

        protected override void OnInitialized()
        {
            _sessionNotification = Session.GetService<UserNotificationService>();
            if (_sessionNotification != null)
            {
                _sessionNotification.OnNotified += HandleNotification;
            }
            base.OnInitialized();
        }

        public void Dispose()
        {
            if (_sessionNotification != null)
                _sessionNotification.OnNotified -= HandleNotification;
        }
    }
}
