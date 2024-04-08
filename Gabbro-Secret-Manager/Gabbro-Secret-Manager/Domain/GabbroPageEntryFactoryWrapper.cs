using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Domain
{
    public class GabbroPageEntryFactoryWrapper(IPageEntryFactory inner, bool requiresEncryptionKey = false) : IGabbroPageEntryFactory
    {
        public bool RequiresEncryptionKey => requiresEncryptionKey;

        public bool RequiresAuthentication => inner.RequiresAuthentication;

        public string Page => inner.Page;

        public string ViewPath => inner.ViewPath;

        public Task<PageEntry> Create(IPageRegistry pageRegistry, IRequestData data, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
            => inner.Create(pageRegistry, data, responseOptions);

        public Task<PageEntry> Create(IPageModel model, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
            => inner.Create(model, responseOptions);
    }
}
