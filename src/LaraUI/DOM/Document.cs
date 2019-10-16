﻿/*
Copyright (c) 2019 Integrative Software LLC
Created: 5/2019
Author: Pablo Carbonell
*/

using Integrative.Lara.Delta;
using Integrative.Lara.DOM;
using Integrative.Lara.Main;
using Integrative.Lara.Middleware;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Integrative.Lara
{
    /// <summary>
    /// Status options for server-side events
    /// </summary>
    public enum ServerEventsStatus
    {
        /// <summary>
        /// Server-side events have not been enabled
        /// </summary>
        Disabled,

        /// <summary>
        /// The server is waiting for the client to listen to server-side events
        /// </summary>
        Connecting,

        /// <summary>
        /// Server-side events are enabled
        /// </summary>
        Enabled
    }

    /// <summary>
    /// An HTML5 document.
    /// </summary>
    public class Document
    {
        internal IPage Page { get; }

        /// <summary>
        /// Global unique identifier for the document
        /// </summary>
        public Guid VirtualId { get; }

        private readonly DocumentIdMap _map;
        private readonly Queue<BaseDelta> _queue;
        internal SemaphoreSlim Semaphore { get; }

        readonly ServerEventsController _serverEvents;
        readonly MessageRegistry _messageRegistry;

        int _serializer;

        /// <summary>
        /// Occurs when the document is unloaded
        /// </summary>
        public event EventHandler OnUnload;

        /// <summary>
        /// Asynchronous unload event
        /// </summary>
        public AsyncEvent OnUnloadAsync { get; } = new AsyncEvent();

        internal event EventHandler AfterUnload;

        /// <summary>
        /// Gets or sets the language. See 'lang' property for HTML5 documents.
        /// </summary>
        /// <value>
        /// The language.
        /// </value>
        public string Lang { get; set; }

        /// <summary>
        /// The document's Head element.
        /// </summary>
        /// <value>
        /// The head.
        /// </value>
        public Element Head { get; }

        /// <summary>
        /// The document's Body element.
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        public Element Body { get; }

        internal DateTime LastUTC { get; private set; }

        internal Queue<BaseDelta> GetQueue() => _queue;

        internal Document(IPage page)
            : this(page, Connections.CreateCryptographicallySecureGuid())
        {
        }

        internal Document(IPage page, Guid virtualId)
        {
            VirtualId = virtualId;
            Page = page;
            _map = new DocumentIdMap();
            _queue = new Queue<BaseDelta>();
            Semaphore = new SemaphoreSlim(1);
            Head = Element.Create("head");
            Head.Document = this;
            Head.IsSlotted = true;
            Body = Element.Create("body");
            Body.Document = this;
            Body.IsSlotted = true;
            UpdateTimestamp();
            TemplateBuilder.Build(this);
            _serverEvents = new ServerEventsController(this);
            _messageRegistry = new MessageRegistry(this);
        }

        /// <summary>
        /// Creates an HTML element.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>Element created</returns>
        public static Element CreateElement(string tagName) => Element.Create(tagName);

        /// <summary>
        /// Creates a text node.
        /// </summary>
        /// <param name="data">The node's data.</param>
        /// <returns>Text node created</returns>
        public static TextNode CreateTextNode(string data) => new TextNode(data);

        internal void UpdateTimestamp()
        {
            LastUTC = DateTime.UtcNow;
        }

        internal void ModifyLastUtcForTesting(DateTime value)
        {
            LastUTC = value;
        }

        internal string GenerateElementId()
        {
            _serializer++;
            return "_e" + _serializer.ToString(CultureInfo.InvariantCulture);
        }

        internal void OnElementAdded(Element element)
            => _map.NotifyAdded(element);

        internal void OnElementRemoved(Element element)
            => _map.NotifyRemoved(element);

        internal void NotifyChangeId(Element element, string before, string after)
            => _map.NotifyChangeId(element, before, after);

        internal bool QueueingEvents { get; private set; }

        internal void OpenEventQueue()
        {
            if (_queue.Count > 0)
            {
                var json = FlushQueue();
                Head.SetAttribute("data-lara-initialdelta", json);
            }
            QueueingEvents = true;
        }

        internal string FlushQueue()
        {
            var list = new List<BaseDelta>();
            while (_queue.Count > 0)
            {
                var step = _queue.Dequeue();
                list.Add(step);
            }
            var result = new EventResult(list);
            return result.ToJSON();
        }

        internal void Enqueue(BaseDelta delta)
        {
            _queue.Enqueue(delta);
        }

        /// <summary>
        /// Retrieves the element with the given ID.
        /// </summary>
        /// <param name="id">Element ID</param>
        /// <param name="element">The element.</param>
        /// <returns>True when the element was found.</returns>
        public bool TryGetElementById(string id, out Element element)
            => _map.TryGetElementById(id, out element);

        /// <summary>
        /// Retrieves the element with the given ID.
        /// </summary>
        /// <param name="id">Element ID</param>
        /// <returns>The element</returns>
        public Element GetElementById(string id)
        {
            _map.TryGetElementById(id, out var element);
            return element;
        }

        /// <summary>
        /// Returns true when there are UI changes pending to be flushed to the client
        /// </summary>
        public bool HasPendingChanges => _queue.Count > 0;

        internal void OnMessage(string key, Func<Task> handler)
        {
            Head.On("_" + key, handler);
        }

        internal void AddMessageListener(string messageId, Func<Task> handler)
        {
            _messageRegistry.Add(messageId, handler);
        }

        internal void RemoveMessageListener(string messageId, Func<Task> handler)
        {
            _messageRegistry.Remove(messageId, handler);
        }

        internal async Task NotifyUnload()
        {
            await _serverEvents.NotifyUnload();
            var args = new EventArgs();
            OnUnload?.Invoke(this, args);
            await OnUnloadAsync.InvokeAsync(this, args);
            AfterUnload?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Returns the current status of server-side events
        /// </summary>
        public ServerEventsStatus ServerEventsStatus
            => _serverEvents.ServerEventsStatus;

        /// <summary>
        /// Starts a server event. Call with 'using' and dispose the class returned.
        /// </summary>
        /// <returns>Disposable token</returns>
        public ServerEvent StartServerEvent()
            => _serverEvents.StartServerEvent();

        internal void ServerEventsOn()
            => _serverEvents.ServerEventsOn();

        internal Task ServerEventsOff()
            => _serverEvents.ServerEventsOff();

        internal bool SocketRemainsOpen(string eventName)
            => _serverEvents.SocketRemainsOpen(eventName);

        internal virtual Task<TaskCompletionSource<bool>> GetSocketCompletion(WebSocket socket)
            => _serverEvents.GetSocketCompletion(socket);

        internal Task ServerEventFlush()
            => _serverEvents.ServerEventFlush();

        internal ServerEventsController GetServerEventsController()
            => _serverEvents;

    }
}
