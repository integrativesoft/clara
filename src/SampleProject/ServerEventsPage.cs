﻿/*
Copyright (c) 2019 Integrative Software LLC
Created: 8/2019
Author: Pablo Carbonell
*/

using Integrative.Lara;
using System.Threading.Tasks;

namespace SampleProject
{
    [LaraPage(Address = "/server")]
    class ServerEventsPage : IPage
    {
        readonly Button _button;

        public ServerEventsPage()
        {
            _button = new Button()
            {
                Disabled = true
            };
            _button.AppendText("before");
            System.Console.WriteLine("instance created");
        }

        public Task OnGet(IPageContext context)
        {
            context.JSBridge.ServerEventsOn();
            context.Document.Body.AppendChild(_button);
            Task.Run(DelayedTask);
            return Task.CompletedTask;
        }

        private async void DelayedTask()
        {
            await Task.Delay(4000);
            using (var access = _button.Document.StartServerEvent())
            {
                _button.ClearChildren();
                _button.AppendText("after");
            }
        }
    }
}