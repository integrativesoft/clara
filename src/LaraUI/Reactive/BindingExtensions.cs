﻿/*
Copyright (c) 2020 Integrative Software LLC
Created: 12/2020
Author: Pablo Carbonell
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Integrative.Lara
{
    /// <summary>
    /// Extensions for element binding operations
    /// </summary>
    public static class BindingExtensions
    {
        #region bind properties

        /// <summary>
        /// Executes code when a source object is modified
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="source"></param>
        /// <param name="onSourceChange"></param>
        /// <returns></returns>
        public static TNode Bind<TNode>(
            this TNode node,
            INotifyPropertyChanged source,
            Action<TNode> onSourceChange)
            where TNode : Element
        {
            node = node ?? throw new ArgumentNullException(nameof(node));
            source = source ?? throw new ArgumentNullException(nameof(source));
            node.AddSubscription(source, () => onSourceChange(node));
            return node;
        }

        /// <summary>
        /// Executes code when the node is modified
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="onChange"></param>
        /// <returns></returns>
        public static TNode BindBack<TNode>(
            this TNode node,
            Action<TNode> onChange)
            where TNode : Element
        {
            node = node ?? throw new ArgumentNullException(nameof(node));
            node.AddSubscription(node, () => onChange(node));
            return node;
        }

        #endregion

        #region bind children

        /// <summary>
        /// Bind element children with observable collection
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="element"></param>
        /// <param name="source"></param>
        /// <param name="childFactory"></param>
        /// <returns></returns>
        public static TParent BindChildren<TParent, TValue>(
            this TParent element,
            ObservableCollection<TValue> source,
            Func<TValue, Element> childFactory)
            where TParent : Element
            where TValue : class, INotifyPropertyChanged
        {
            element = element ?? throw new ArgumentNullException(nameof(element));
            source = source ?? throw new ArgumentNullException(nameof(source));
            element.ClearChildren();
            foreach (var item in source)
            {
                element.AppendChild(childFactory(item));
            }
            element.SubscribeChildren(source, (_, args) =>
            {
                var updater = new CollectionUpdater<TValue>(childFactory, element, args);
                updater.Run();
            });
            return element;
        }

        #endregion
    }
}
