﻿/*
Copyright (c) 2019 Integrative Software LLC
Created: 6/2019
Author: Pablo Carbonell
*/

using Integrative.Lara.Main;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Integrative.Lara.Middleware
{
    sealed class PublishedItemHandler : BaseHandler
    {
        readonly LaraOptions _options;
        readonly Application _app;

        public PublishedItemHandler(RequestDelegate next, Application app, LaraOptions options) : base(next)
        {
            _options = options;
            _app = app;
        }

        internal override async Task<bool> ProcessRequest(HttpContext http)
        {
            var combined = Published.CombinePathMethod(http.Request.Path, http.Request.Method);
            if (_app.TryGetNode(combined, out var item))
            {
                await item.Run(_app, http, _options);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
