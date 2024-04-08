
namespace Gabbro_Secret_Manager.Core
{
    public class PageEntryFactory : IPageEntryFactory
    {
        public bool RequiresAuthentication { get; init; } = false;
        public required string Page { get; init; }
        public required string ViewPath { get; init; }
        public required Func<IPageRegistry, IRequestData, IPageModel> ModelFactory { get; init; }
        public Func<HxHeaderBuilder, HxHeaderBuilder>? ConfigureResponse { get; init; }

        public Task<PageEntry> Create(IPageRegistry pageRegistry, IRequestData data, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null) => Create(ModelFactory(pageRegistry, data), responseOptions);

        public Task<PageEntry> Create(IPageModel model, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
        {
            return Task.FromResult(new PageEntry
            {
                Page = Page,
                ViewPath = ViewPath,
                Model = model,
                ConfigureResponse = CombineResponseOptions(ConfigureResponse, responseOptions)
            });
        }

        /// <summary>
        /// Will be applied in order of arguments (starting with first)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        private static Func<T, T>? CombineNullableFunctions<T>(params Func<T, T>?[] funcs)
        {
            Func<T, T>? combinedFunc = null;
            foreach (var func in funcs)
            {
                if (func == null)
                    continue;
                if (combinedFunc == null)
                    combinedFunc = func;
                else
                    combinedFunc = t => func(t);
            }

            return combinedFunc;
        }

        /// <summary>
        /// Will be applied in order of arguments (starting with first)
        /// </summary>
        public static Action<IHeaderDictionary>? CombineResponseOptions(params Func<HxHeaderBuilder, HxHeaderBuilder>?[] funcs)
        {
            var finalFunc = CombineNullableFunctions(funcs);
            return finalFunc?.Invoke(new HxHeaderBuilder()).Build();

        }
    }
}
