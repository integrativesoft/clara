﻿/*
Copyright (c) 2019 Integrative Software LLC
Created: 5/2019
Author: Pablo Carbonell
*/

using Integrative.Lara.DOM;
using Integrative.Lara.Middleware;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Integrative.Lara.Main
{
    sealed class PagePublished : IPublishedItem
    {
        readonly Func<IPage> _factory;

        public PagePublished(Func<IPage> factory)
        {
            _factory = factory;
        }

        public async Task Run(HttpContext http, LaraOptions options)
        {
            var page =_factory();
            await RunGetHandler(http, page, options);
        }

        private static async Task RunGetHandler(HttpContext http, IPage page, LaraOptions options)
        {
            var connection = GetConnection(http);
            var document = connection.CreateDocument(page, options);
            var execution = new PageContext(http, connection, document);
            if (await MiddlewareCommon.RunHandler(http, async () => await page.OnGet(execution)))
            {
                await ProcessGetResult(http, document, execution);
            }
        }

        internal static async Task ProcessGetResult(HttpContext http, Document document, PageContext execution)
        {
            if (!string.IsNullOrEmpty(execution.RedirectLocation))
            {
                http.Response.Redirect(execution.RedirectLocation);
            }
            else
            {
                document.OpenEventQueue();
                string html = WriteDocument(execution.Document);
                await ReplyDocument(http, html);
            }
        }

        private static Connection GetConnection(HttpContext http)
        {
            if (MiddlewareCommon.TryFindConnection(http, out var connection))
            {
                return connection;
            }
            else
            {
                return CreateConnection(http);
            }
        }

        private static Connection CreateConnection(HttpContext http)
        {
            var connection = LaraUI.CreateConnection(http.Connection.RemoteIpAddress);
            http.Response.Cookies.Append(GlobalConstants.CookieSessionId,
                connection.Id.ToString(GlobalConstants.GuidFormat));
            return connection;
        }

        private static string WriteDocument(Document document)
        {
            var writer = new DocumentWriter(document);
            writer.Print();
            return writer.ToString();
        }

        private static async Task ReplyDocument(HttpContext http, string html)
        {
            MiddlewareCommon.SetStatusCode(http, HttpStatusCode.OK);
            MiddlewareCommon.AddHeaderTextHtml(http);
            MiddlewareCommon.AddHeaderPreventCaching(http);
            await MiddlewareCommon.WriteUtf8Buffer(http, html);
        }
    }
}
