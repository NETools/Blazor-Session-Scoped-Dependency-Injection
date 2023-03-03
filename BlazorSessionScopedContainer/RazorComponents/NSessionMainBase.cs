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
        public NSession Container { get; set; }

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

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                _sessionNotification = Container.GetService<UserNotificationService>();
                _sessionNotification.OnNotified += HandleNotification;
            }

            base.OnAfterRender(firstRender);
        }

        public void Dispose()
        {
            if (_sessionNotification != null)
                _sessionNotification.OnNotified -= HandleNotification;
        }
    }
}
