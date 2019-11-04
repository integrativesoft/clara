﻿/*
Copyright (c) 2019 Integrative Software LLC
Created: 11/2019
Author: Pablo Carbonell
*/

using Integrative.Lara.Main;
using Integrative.Lara.Tools;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Threading.Tasks;

namespace Integrative.Lara.Middleware
{
    interface IModeController
    {
        Task<IWebHost> Start(Application app, StartServerOptions options);
        Connection CreateConnection(IPAddress remoteIp);
        double KeepAliveInterval { get; }
        ApplicationMode Mode { get; }
    }

    static class ModeControllerFactory
    {
        public static IModeController Create(Application app, ApplicationMode mode)
        {
            if (mode == ApplicationMode.BrowserApp)
            {
                return new BrowserAppController(app);
            }
            else
            {
                return new BaseModeController(app, ApplicationMode.Default);
            }
        }
    }

    class BaseModeController : IModeController
    {
        public const double DefaultKeepAliveInterval
            = StaleConnectionsCollector.DefaultExpireInterval / 2.5;  // at least 2 message attempts per expire period

        protected readonly Application _app;

        public BaseModeController(Application app, ApplicationMode mode)
        {
            _app = app;
            Mode = mode;
        }

        public virtual double KeepAliveInterval => DefaultKeepAliveInterval;

        public virtual ApplicationMode Mode { get; }

        public virtual Connection CreateConnection(IPAddress remoteIp)
        {
            var connections = _app.GetPublished().Connections;
            return connections.CreateConnection(remoteIp);
        }

        public virtual Task<IWebHost> Start(Application app, StartServerOptions options)
        {
            return ServerLauncher.StartServer(app, options);
        }
    }
}