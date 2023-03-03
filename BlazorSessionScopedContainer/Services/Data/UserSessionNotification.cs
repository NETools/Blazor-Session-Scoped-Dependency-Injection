namespace BlazorSessionScopedContainer.Services.Data
{
    public class UserSessionNotification
    {
        public enum SessionNotificationType
        {
            SessionClosed,
            SessionMessage
        }

        public string Message { get; set; }
        public SessionNotificationType NotificationType { get; set; }
    }
}
