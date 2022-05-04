using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jasily.ViewModel.Tests
{
    [TestClass]
    public class TestNotifyPropertyChangedObject : NotifyPropertyChangedObject
    {
        [TestMethod]
        public void TestNotifyPropertyChanged()
        {
            using var counter = new PropertyChangedEventsCounter(this);

            this.NotifyPropertyChanged("ABC");
            CollectionAssert.AreEqual(new[] { "ABC" }, counter.Events);

            this.NotifyPropertyChanged("123", "456");
            CollectionAssert.AreEqual(new[] { "ABC", "123", "456" }, counter.Events);
        }

        class SubClass_1 : NotifyPropertyChangedObject
        {
            private int _val;

            public int Val { get => this._val; set => this.ChangeModelProperty(ref this._val, value); }
        }

        [TestMethod]
        public void TestBlockNotifyPropertyChanged()
        {
            var obj = new SubClass_1();
            using var counter = new PropertyChangedEventsCounter(obj);

            obj.Val = 10;
            CollectionAssert.AreEqual(new[] { "Val" }, counter.Events);

            using (obj.BlockNotifyPropertyChanged())
            {
                obj.Val = 11;
                CollectionAssert.AreEqual(new[] { "Val" }, counter.Events);

                obj.Val = 12;
                CollectionAssert.AreEqual(new[] { "Val" }, counter.Events);

                obj.Val = 13;
                CollectionAssert.AreEqual(new[] { "Val" }, counter.Events);
            }

            CollectionAssert.AreEqual(new[] { "Val", "Val", "Val", "Val" }, counter.Events);
        }
    }

    public class PropertyChangedEventsCounter : IDisposable
    {
        private readonly NotifyPropertyChangedObject _obj;

        public PropertyChangedEventsCounter(NotifyPropertyChangedObject obj)
        {
            obj.PropertyChanged += this.OnPropertyChanged;
            this._obj = obj;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Events.Add(e.PropertyName);
        }

        public void Dispose() => this._obj.PropertyChanged -= this.OnPropertyChanged;

        public List<string> Events { get; } = new ();
    }
}
