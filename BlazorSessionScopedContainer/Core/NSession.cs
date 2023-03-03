using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Services;
using Microsoft.AspNetCore.Http;

namespace BlazorSessionScopedContainer.Core
{
    public class NSession
    {
        private IHttpContextAccessor _httpContext;

        private bool _sessionNewlySet;
        private Guid _sessionGuid;

        public NSession(IHttpContextAccessor _httpContext)
        {
            this._httpContext = _httpContext;
        }

        public void InitializeServiceRoutine(Action<SessionId, NSessionHandler> routine)
        {
            var session = GetSession();
            if (!session.HasValue)
                return;

            if (!NSessionHandler.Default().InitializedServices.Contains(session.Value))
            {
                NSessionHandler.Default().InitializedServices.Add(session.Value);
                NSessionHandler.Default().SessionLastActiveTime[session.Value] = DateTime.Now;

                var handler = NSessionHandler.Default();
                var sessionId = new SessionId(session);
                handler.AddService<UserNotificationService>(sessionId);
                routine(sessionId, handler);
            }
        }

        public void RefreshSesion()
        {
            var session = GetSession();
            if (session.HasValue)
                NSessionHandler.Default().SessionLastActiveTime[session.Value] = DateTime.Now;
        }

        internal void RefreshSesion(Guid session)
        {
            NSessionHandler.Default().SessionLastActiveTime[session] = DateTime.Now;
        }

        public T GetService<T>() where T : class, ISessionScoped
        {
            var session = GetSession();
            if (!session.HasValue)
                return default;

            RefreshSesion();

            return (T)NSessionHandler.Default().ServiceInstances[session.Value][typeof(T)].Value;
        }

        public void StartSession()
        {
            if (_httpContext.HttpContext?.Request.Cookies.ContainsKey("session") == false)
            {
                _sessionGuid = Guid.NewGuid();
                _httpContext.HttpContext.Response.Cookies.Append("session", $"{_sessionGuid}");
                NSessionHandler.Default().ServiceInstances.TryAdd(_sessionGuid, new Dictionary<Type, Lazy<ISessionScoped>>());
                _sessionNewlySet = true;
            }

            if (_sessionNewlySet)
                RefreshSesion(_sessionGuid);
            else RefreshSesion();
        }

        public Guid? GetSession()
        {
            if (_httpContext.HttpContext?.Request.Cookies.ContainsKey("session") == true)
            {
                return new Guid(_httpContext.HttpContext.Request?.Cookies["session"]);
            }

            if (_sessionNewlySet)
                return _sessionGuid;

            return null;
        }
    }
}
