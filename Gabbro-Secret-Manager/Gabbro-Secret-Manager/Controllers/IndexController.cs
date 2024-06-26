﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Views;

namespace Gabbro_Secret_Manager.Controllers
{
    public class IndexController(
        IOptions<IndexSettings> options,
        IPageRegistry pageRegistry) : BaseController
    {
        private readonly IndexSettings _indexSettings = options.Value;

        [Route("/")]
        public Task<IActionResult> Redirect()
        {
            return Get(_indexSettings.HomePage);
        }


        [Route("{page}")]
        public async Task<IActionResult> Get([FromRoute] string page)
        {
            var loader = await pageRegistry.GetPageFactory("loader").Create(new LoaderModel { Location = $"/partials/{page}" });
            var index = await pageRegistry.GetPageFactory("index").Create(new IndexModel
                {
                    NavigationBar = await pageRegistry.GetPageFactory("navigationBar").Create(new NavigationBarModel()),
                    Title = _indexSettings.SiteName,
                    Content = loader
                });
            return index.CreateView(this);
        }
    }
}
