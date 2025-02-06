using GabbroSecretManager.UI.Shared.Components;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Components;

namespace GabbroSecretManager.UI.Shared.Services
{
    public class LayoutComponentFactory : ILayoutComponentFactory
    {
        public Task<IComponent> GetLayoutAsync(IComponent content)
        {
            return Task.FromResult<IComponent>(new Layout
            {
                Content = content
            });
        }
    }
}
