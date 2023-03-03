using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Core
{
    public sealed class SessionId
    {
        internal Guid? Guid { get; private set; }
        internal SessionId(Guid? guid)
        {
            Guid = guid;
        }

    }
}
