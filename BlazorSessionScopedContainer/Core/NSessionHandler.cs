using BlazorSessionScopedContainer.Attributes;
using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Contracts.GlobalScoped;
using BlazorSessionScopedContainer.Contracts.SessionsScoped;
using BlazorSessionScopedContainer.Core.Data.GlobalScoped;
using BlazorSessionScopedContainer.Core.Data.SessionScoped;
using BlazorSessionScopedContainer.Misc;
using BlazorSessionScopedContainer.Services;
using BlazorSessionScopedContainer.Services.Persistence;
using BlazorSessionScopedContainer.Services.Persistence.Json;

namespace BlazorSessionScopedContainer.Core
{
    public partial class NSessionHandler
    {
        internal Dictionary<Guid, List<IServiceEntry>> ServiceInstances { get; private set; } = new Dictionary<Guid, List<IServiceEntry>>();
        internal List<IServiceEntry> GlobalServices { get; private set; } = new List<IServiceEntry>();

        internal Dictionary<Guid, DateTime> SessionLastActiveTime { get; private set; } = new Dictionary<Guid, DateTime>();
        internal HashSet<Guid> InitializedServices { get; private set; } = new HashSet<Guid>();
        public NSessionGarbageCollector GarbageCollection { get; private set; }

        public Action<string> Logger { get; set; } = (message) =>
        {
            Console.WriteLine(message);
        };

        private NSessionHandler()
        {
            GarbageCollection = new NSessionGarbageCollector();
        }

        private static NSessionHandler _sessionHandler;
        internal static NSessionHandler Default()
        {
            if (_sessionHandler == null)
            {
                _sessionHandler = new NSessionHandler();

            }
            return _sessionHandler;
        }


        private bool PrepareServiceHandler(SessionId session)
        {
            if (session.Guid.HasValue)
            {
                if (!ServiceInstances.ContainsKey(session.Guid.Value))
                {
                    ServiceInstances.Add(session.Guid.Value, new List<IServiceEntry>());
                }

                return true;
            }

            return false;
        }

    
    }
}
