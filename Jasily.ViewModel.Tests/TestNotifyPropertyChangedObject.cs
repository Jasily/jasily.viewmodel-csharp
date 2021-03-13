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

        [TestMethod]
        public void TestBlockNotifyPropertyChanged()
        {
            using var counter = new PropertyChangedEventsCounter(this);

            using (this.BlockNotifyPropertyChanged())
            {
                this.NotifyPropertyChanged("ABC");
                CollectionAssert.AreEqual(Array.Empty<string>(), counter.Events);

                this.NotifyPropertyChanged("123", "456");
                CollectionAssert.AreEqual(Array.Empty<string>(), counter.Events);
            }

            CollectionAssert.AreEqual(new[] { "ABC", "123", "456" }, counter.Events);
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
