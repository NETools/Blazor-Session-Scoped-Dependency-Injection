using BlazorSessionScopedContainer.Core;
using Microsoft.AspNetCore.Components;

namespace BlazorSessionScopedContainer.RazorComponents
{
    public class NSessionComponentBase : ComponentBase
    {
        [Inject]
        public NSession Session { get; set; }

        /// <summary>
        /// Whenever user interacts with page, the session is refreshed.
        /// </summary>
        /// <param name="firstRender"></param>
        protected override void OnAfterRender(bool firstRender)
        {
            Session.RefreshSesion();
            base.OnAfterRender(firstRender);
        }
    }
}
