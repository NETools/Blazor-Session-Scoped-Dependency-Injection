using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Contracts.GlobalScoped;
using BlazorSessionScopedContainer.Contracts.SessionsScoped;
using BlazorSessionScopedContainer.Services;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

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

        public T? GetService<T>() where T : class, ISessionScoped
        {
            var session = GetSession();
            if (!session.HasValue)
                return default;

            RefreshSesion();

            return (T?)NSessionHandler.Default().ServiceInstances[session.Value].Find(p => p.AreServicesEqual<T>())?.GetServiceInstance();
        }

        public T? GetGlobalService<T>() where T: class, IGlobalScoped
        {
            RefreshSesion();
            return (T?)NSessionHandler.Default().GlobalServices.Find(p => p.AreServicesEqual<T>())?.GetServiceInstance();
        }

        public T? New<T>(params object[] args)
		{
			var session = GetSession();
			if (!session.HasValue)
				return default;

            return NSessionHandler.Default().GetSessionInstance<T>(session, args);
        }

        public void StartSession(Action<SessionId, NSessionHandler> initRoutine)
        {
            if (_httpContext.HttpContext?.Request.Cookies.ContainsKey("session") == false)
            {
                _sessionGuid = Guid.NewGuid();
                _httpContext.HttpContext.Response.Cookies.Append("session", $"{_sessionGuid}");
                NSessionHandler.Default().ServiceInstances.TryAdd(_sessionGuid, new List<IServiceEntry>());
                _sessionNewlySet = true;
            }

            if (_sessionNewlySet)
                RefreshSesion(_sessionGuid);
            else RefreshSesion();

            InitializeSession(initRoutine);
        }

        private void InitializeSession(Action<SessionId, NSessionHandler> routine)
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
                InitializeDefaultServices(sessionId, handler);
                routine(sessionId, handler);
            }
        }

        private void InitializeDefaultServices(SessionId sessionId, NSessionHandler handler)
        {
            handler.AddService<UserNotificationService>(sessionId);
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
