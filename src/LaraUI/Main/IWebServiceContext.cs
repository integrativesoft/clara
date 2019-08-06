﻿/*
Copyright (c) 2019 Integrative Software LLC
Created: 6/2019
Author: Pablo Carbonell
*/

using System.Net;

namespace Integrative.Lara
{
    /// <summary>
    /// Context for web service requests
    /// </summary>
    public interface IWebServiceContext : ILaraContext
    {
        /// <summary>
        /// Request's body sent by the client
        /// </summary>
        string RequestBody { get; }

        /// <summary>
        /// Status code to return
        /// </summary>
        HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets a Session object when available
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        bool TryGetSession(out Session session);
    }
}