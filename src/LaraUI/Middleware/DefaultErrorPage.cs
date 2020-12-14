﻿/*
Copyright (c) 2019-2020 Integrative Software LLC
Created: 9/2019
Author: Pablo Carbonell
*/

using System.Threading.Tasks;

namespace Integrative.Lara
{
    internal class DefaultErrorPage : IPage
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public Task OnGet()
        {
            LoadBootstrap();
            ShowContent();
            return Task.CompletedTask;
        }

        private static void LoadBootstrap()
        {
            var head = LaraUI.Page.Document.Head;
            head.AppendChild(new HtmlLinkElement
            {
                Rel = "stylesheet",
                HRef = "https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css"
            });
            head.AppendChild(new HtmlScriptElement
            {
                Src = "https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js",
                Defer = true
            });
            head.AppendChild(new HtmlScriptElement
            {
                Src = "https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js",
                Defer = true
            });
        }

        private void ShowContent()
        {
            var builder = new LaraBuilder(LaraUI.Page.Document.Body);
            builder.Push("div", "container mt-2")
                .Push("div", "jumbotron")
                    .Push("img")
                        .Attribute("src", ServerLauncher.ErrorAddress + ".svg")
                        .Attribute("height", "100px")
                    .Pop()
                    .Push("h1", "display-4")
                        .InnerText(Title)
                    .Pop()
                    .Push("p", "lead")
                        .InnerText(Message)
                    .Pop()
                .Pop()
            .Pop();
        }
    }
}
