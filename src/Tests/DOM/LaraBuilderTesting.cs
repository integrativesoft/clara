﻿/*
Copyright (c) 2019 Integrative Software LLC
Created: 8/2019
Author: Pablo Carbonell
*/

using System.Collections.ObjectModel;
using Xunit;

namespace Integrative.Lara.Tests.DOM
{
    public class LaraBuilderTesting
    {
        readonly Element _root;
        readonly LaraBuilder _builder;

        public LaraBuilderTesting()
        {
            _root = Element.Create("div");
            _builder = new LaraBuilder(_root);
        }

        [Fact]
        public async void OnStringAction()
        {
            int counter = 0;
            _builder.On("click", () => counter++);
            await _root.NotifyEvent("click");
            Assert.Equal(1, counter);
        }

        [Fact]
        public void ToggleClassStringBool()
        {
            _builder.ToggleClass("red", true);
            Assert.Equal("red", _root.Class);
        }

        [Fact]
        public void ToggleClassString()
        {
            _builder.ToggleClass("blue");
            Assert.Equal("blue", _root.Class);
        }

        [Fact]
        public void RemoveClass()
        {
            _root.Class = "very dark";
            _builder.RemoveClass("very");
            Assert.Equal("dark", _root.Class);
        }

        [Fact]
        public void AddClass()
        {
            _builder.AddClass("green");
            Assert.Equal("green", _root.Class);
        }

        [Fact]
        public void GetCurrent()
        {
            _builder.GetCurrent(out var element);
            Assert.Same(_root, element);
        }

        [Fact]
        public void BindOptions()
        {
            var data = new MyData();
            bool found = false;
            _builder.Bind(new BindHandlerOptions<MyData>
            {
                ModifiedHandler = (x, y) => found = true,
                Object = data
            });
            data.Counter = 15;
            Assert.True(found);
        }

        [Fact]
        public void BindValueActions()
        {
            var data = new MyData();
            bool found = false;
            _builder.Bind(data, () => found = true);
            data.Counter = 15;
            Assert.True(found);
        }

        [Fact]
        public void BindChildrenOptions()
        {
            var list = new ObservableCollection<MyData>();
            _builder.BindChildren(new BindChildrenOptions<MyData>
            {
                Collection = list,
                CreateCallback = x => Element.Create("span")
            });
            list.Add(new MyData());
            Assert.NotEmpty(_root.Children);
        }

        [Fact]
        public void BindChildrenCollectionElement()
        {
            var list = new ObservableCollection<MyData>();
            _builder.BindChildren(list, () => Element.Create("span"));
            list.Add(new MyData());
            Assert.NotEmpty(_root.Children);
        }

        [Fact]
        public void BindChildenCollectionElement()
        {
            var list = new ObservableCollection<MyData>();
            _builder.BindChildren(list, x => Element.Create("div"));
            list.Add(new MyData());
            Assert.NotEmpty(_root.Children);
        }

        [Fact]
        public void BindInnerTextOptions()
        {
            var data = new MyData();
            _builder.BindInnerText(new BindInnerTextOptions<MyData>
            {
                Object = data,
                Property = x => x.Counter.ToString()
            });
            data.Counter = 3;
            VerifyInnerText(_root, "3");
        }

        private void VerifyInnerText(Element element, string data)
        {
            Assert.NotEmpty(element.Children);
            var node = element.GetChildAt(0) as TextNode;
            Assert.NotNull(node);
            Assert.Equal(data, node.Data);
        }

        [Fact]
        public void BindInnerTextExpanded()
        {
            var data = new MyData();
            _builder.BindInnerText(data, () => data.Counter.ToString());
            data.Counter = 3;
            VerifyInnerText(_root, "3");
        }

        [Fact]
        public void BindInnerTextValueFuncString()
        {
            var data = new MyData();
            _builder.BindInnerText(data, x => x.Counter.ToString());
            data.Counter = 3;
            VerifyInnerText(_root, "3");
        }

        [Fact]
        public void BindAttributeOptions()
        {
            var data = new MyData();
            _builder.BindAttribute(new BindAttributeOptions<MyData>
            {
                Attribute = "data-counter",
                Object = data,
                Property = x => x.Counter.ToString()
            });
            data.Counter = 2;
            Assert.Equal("2", _root.GetAttribute("data-counter"));
        }

        [Fact]
        public void BindAttributeExpanded()
        {
            var data = new MyData();
            _builder.BindAttribute("data-counter", data, () => data.Counter.ToString());
            data.Counter = 2;
            Assert.Equal("2", _root.GetAttribute("data-counter"));
        }

        [Fact]
        public void BindAttributeStringFuncString()
        {
            var data = new MyData();
            _builder.BindAttribute("data-counter", data, x => x.Counter.ToString());
            data.Counter = 2;
            Assert.Equal("2", _root.GetAttribute("data-counter"));
        }

        [Fact]
        public void BindActionElement()
        {
            var data = new MyData();
            _builder.Bind(data, (x, y) => y.SetInnerText(x.Counter.ToString()));
            data.Counter = 14;
            VerifyInnerText(_root, "14");
        }

        class MyData : BindableBase
        {
            int _counter;

            public int Counter
            {
                get => _counter;
                set { SetProperty(ref _counter, value); }
            }
        }
    }
}