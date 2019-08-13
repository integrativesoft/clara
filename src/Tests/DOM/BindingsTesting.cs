﻿/*
Copyright (c) 2019 Integrative Software LLC
Created: 8/2019
Author: Pablo Carbonell
*/

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xunit;

namespace Integrative.Lara.Tests.DOM
{
    public class BindingsTesting
    {
        [Fact]
        public void SetInnerTextSetsText()
        {
            var x = Element.Create("span");
            x.SetInnerText("hello");
            Assert.Equal(1, x.ChildCount);
            VerifyInnerText(x, "hello");
        }

        private void VerifyInnerText(Element element, string data)
        {
            var node = element.GetChildAt(0) as TextNode;
            Assert.NotNull(node);
            Assert.Equal(data, node.Data);
        }

        [Fact]
        public void InnerTextReplacesPrevious()
        {
            var x = Element.Create("span");
            x.SetInnerText("hello");
            x.SetInnerText("bye");
            VerifyInnerText(x, "bye");
        }

        [Fact]
        public void BindInnerTextUpdates()
        {
            var data = new MyData();
            var div = Element.Create("div");
            div.BindInnerText(new BindInnerTextOptions<MyData>
            {
                Object = data,
                Property = x => x.Counter.ToString()
            });
            data.Counter = 5;
            VerifyInnerText(div, "5");
        }

        [Fact]
        public void BindGenericExecutes()
        {
            var data = new MyData();
            var div = Element.Create("div");
            div.Bind(new BindHandlerOptions<MyData>
            {
                Object = data,
                ModifiedHandler = (x, y) => div.SetInnerText(data.Counter.ToString())
            });
            data.Counter = 5;
            VerifyInnerText(div, "5");
        }

        [Fact]
        public void BindActionExecutes()
        {
            var data = new MyData();
            var div = Element.Create("div");
            div.Bind(data, (x, y) => div.SetInnerText(data.Counter.ToString()));
            data.Counter = 5;
            VerifyInnerText(div, "5");
        }

        [Fact]
        public void BindAttributeExecutes()
        {
            var data = new MyData();
            var div = Element.Create("div");
            div.BindAttribute(new BindAttributeOptions<MyData>
            {
                Attribute = "data-counter",
                Object = data,
                Property = x => x.Counter.ToString()
            });
            data.Counter = 5;
            Assert.Equal("5", div.GetAttribute("data-counter"));
        }

        [Fact]
        public void BindChildrenUpdates()
        {
            var collection = new ObservableCollection<MyData>
            {
                new MyData()
            };
            var span = Element.Create("span");
            span.BindChildren(new BindChildrenOptions<MyData>
            {
                Collection = collection,
                CreateCallback = MyCreateCallback
            });
            collection.Add(new MyData());
            Assert.Equal(2, span.ChildCount);
        }

        [Fact]
        public void UnbindAllUnbinds()
        {
            var collection = new ObservableCollection<MyData>();
            var data = new MyData();
            var div = Element.Create("div");
            var span1 = Element.Create("span");
            var span2 = Element.Create("span");
            span2.Bind(data, (x, y) => span2.SetInnerText(x.Counter.ToString()));
            div.BindAttribute(new BindAttributeOptions<MyData>
            {
                Attribute = "data-counter",
                Object = data,
                Property = x => x.Counter.ToString()
            });
            div.BindChildren(new BindChildrenOptions<MyData>
            {
                Collection = collection,
                CreateCallback = x => Element.Create("div")
            });
            span1.BindInnerText(new BindInnerTextOptions<MyData>
            {
                Object = data,
                Property = x => x.Counter.ToString()
            });
            data.Counter = 5;
            div.UnbindAll();
            span1.UnbindAll();
            span2.UnbindAll();
            collection.Add(data);
            data.Counter = 10;
            VerifyInnerText(span1, "5");
            VerifyInnerText(span2, "5");
            Assert.Equal(0, div.ChildCount);
            Assert.Equal("5", div.GetAttribute("data-counter"));
        }

        [Fact]
        public void UnbindAtributeRuns()
        {
            var x = Element.Create("div");
            var data = new MyData();
            x.BindAttribute(new BindAttributeOptions<MyData>
            {
                Attribute = "data-counter",
                Object = data,
                Property = y => y.Counter.ToString()
            });
            x.BindAttribute(new BindAttributeOptions<MyData>
            {
                Attribute = "data-counter2",
                Object = data,
                Property = y => y.Counter.ToString()
            });
            data.Counter = 5;
            x.UnbindAttribute("data-counter");
            data.Counter = 10;
            Assert.Equal("5", x.GetAttribute("data-counter"));
            Assert.Equal("10", x.GetAttribute("data-counter2"));
        }

        [Fact]
        public void UnbindAttributeRemovesAllAttributes()
        {
            var x = Element.Create("div");
            var data = new MyData();
            x.BindAttribute(new BindAttributeOptions<MyData>
            {
                Attribute = "data-counter",
                Object = data,
                Property = y => y.Counter.ToString()
            });
            x.BindAttribute(new BindAttributeOptions<MyData>
            {
                Attribute = "data-counter2",
                Object = data,
                Property = y => y.Counter.ToString()
            });
            data.Counter = 5;
            x.UnbindAttributes();
            data.Counter = 10;
            Assert.Equal("5", x.GetAttribute("data-counter"));
            Assert.Equal("5", x.GetAttribute("data-counter2"));
        }

        [Fact]
        public void UnbindInnerTextWorks()
        {
            var div = Element.Create("div");
            var data = new MyData
            {
                Counter = 5
            };
            div.BindInnerText(new BindInnerTextOptions<MyData>
            {
                Object = data,
                Property = x => x.Counter.ToString()
            });
            VerifyInnerText(div, "5");
            div.UnbindInnerText();
            data.Counter = 10;
            VerifyInnerText(div, "5");
        }

        [Fact]
        public void UnbindHandlerWorks()
        {
            var div = Element.Create("div");
            var data = new MyData();
            div.Bind(data, (x, y) => div.SetInnerText(data.Counter.ToString()));
            data.Counter = 3;
            div.UnbindHandler();
            data.Counter = 8;
            VerifyInnerText(div, "3");
        }

        [Fact]
        public void UnbindChildrenWorks()
        {
            var collection = new ObservableCollection<MyData>();
            var div = Element.Create("div");
            div.BindChildren(new BindChildrenOptions<MyData>
            {
                Collection = collection,
                CreateCallback = x => Element.Create("span")
            });
            collection.Add(new MyData());
            div.UnbindChildren();
            collection.Clear();
            Assert.NotEmpty(div.Children);
        }

        [Fact]
        public void GenericBindingDetectsCycles()
        {
            var div = Element.Create("div");
            var data = new MyData();
            div.BindAttribute(new BindAttributeOptions<MyData>
            {
                Attribute = "data-counter",
                Object = data,
                Property = x => x.Counter.ToString()
            });
            div.Bind(new BindHandlerOptions<MyData>
            {
                Object = data,
                ModifiedHandler = (x, y) => data.Counter++
            });
            bool found = false;
            try
            {
                data.Counter = 3;
            }
            catch (InvalidOperationException)
            {
                found = true;
            }
            Assert.True(found);
        }

        private Element MyCreateCallback(MyData arg)
        {
            var span = Element.Create("span");
            span.BindAttribute(new BindAttributeOptions<MyData>
            {
                Object = arg,
                Attribute = "data-counter",
                Property = x => arg.Counter.ToString()
            });
            return span;
        }

        class MyData : BindableBase
        {
            int _counter;

            public MyData()
            {
            }

            public MyData(int counter)
            {
                Counter = counter;
            }

            public int Counter
            {
                get => _counter;
                set => SetProperty(ref _counter, value);
            }
        }

        [Fact]
        public void BindableBaseSkipsUnncesaryEvents()
        {
            bool raised = false;
            var data = new MyData();
            data.PropertyChanged += (sender, args) => raised = true;
            data.Counter = 0;
            Assert.False(raised);
        }

        [Fact]
        public void CollectionUpdaterMove()
        {
            var collection = new ObservableCollection<MyData>();
            var div = Element.Create("div");
            div.BindChildren(new BindChildrenOptions<MyData>
            {
                Collection = collection,
                CreateCallback = MyCreateCallback
            });
            collection.Add(new MyData(10));
            collection.Add(new MyData(20));
            collection.Add(new MyData(30));
            collection.Add(new MyData(40));
            collection.Add(new MyData(50));
            VerifyPositions(collection, div);
            collection.Move(1, 2);
            VerifyPositions(collection, div);
            collection.RemoveAt(3);
            VerifyPositions(collection, div);
            collection[2] = new MyData(77);
            VerifyPositions(collection, div);
            collection.Clear();
            VerifyPositions(collection, div);
        }

        private void VerifyPositions(ObservableCollection<MyData> collection, Element div)
        {
            Assert.Equal(collection.Count, div.ChildCount);
            for (int index = 0; index < collection.Count; index++)
            {
                var data = collection[index];
                VerifyPosition(div, index, data.Counter.ToString());
            }
        }

        private void VerifyPosition(Element div, int position, string value)
        {
            var child = (Element)div.GetChildAt(position);
            var current = child.GetAttribute("data-counter");
            Assert.Equal(value, current);
        }
    }
}
