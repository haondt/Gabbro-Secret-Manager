﻿using GabbroSecretManager.UI.Shared.Components;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace GabbroSecretManager.UI.Shared.Middlewares
{
    public class UnmappedRouteHandlerMiddleware(RequestDelegate next, IComponentFactory componentFactory)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            await next(context);

            if (context.Response.StatusCode != StatusCodes.Status404NotFound)
                return;

            if (context.Response.ContentLength != null)
                return;

            if (!string.IsNullOrEmpty(context.Response.ContentType))
                return;

            IComponent component = new Error
            {
                StatusCode = 404,
                Message = "Not found",
            };

            if (!context.Request.AsRequestData().IsHxRequest())
                component = new Page
                {
                    Content = component
                };

            var result = await componentFactory.RenderComponentAsync(component);
            await result.ExecuteAsync(context);
        }

    }
}
