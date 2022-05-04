using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Jasily.ViewModel.Extensions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jasily.ViewModel.Tests.Extensions
{
    [TestClass]
    public class TestNotifyPropertyChanged
    {
        class Simple1 : NotifyPropertyChangedObject
        {
            private int _value;

            public int PropValue { get => this._value; set => this.ChangeModelProperty(ref this._value, value); }
        }

        class Simple2 : NotifyPropertyChangedObject
        {
            private Simple1 _value;

            public Simple1 Simple1Value { get => this._value; set => this.ChangeModelProperty(ref this._value, value); }
        }

        class Simple3 : NotifyPropertyChangedObject
        {
            private Simple2 _value;

            public Simple2 Simple2Value { get => this._value; set => this.ChangeModelProperty(ref this._value, value); }
        }

        [TestMethod]
        public void TestListenOnRootProperty()
        {
            var values = new List<object>();
            var s = new Simple1();
            var d = s.ListenPropertyChanged(x => x.PropValue, values.Add);
            s.PropValue = 1;
            s.PropValue = 2;
            s.PropValue = 3;
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, values);
            d.Dispose();
            s.PropValue = 1;
            s.PropValue = 2;
            s.PropValue = 3;
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, values);
        }

        [TestMethod]
        public void TestListenOnDeepProperty()
        {
            var values = new List<object>();
            var s = new Simple3
            {
                Simple2Value = new Simple2 
                { 
                    Simple1Value = new Simple1
                    {
                        PropValue = 15
                    }
                }
            };
            using (s.ListenPropertyChanged(x => x.Simple2Value.Simple1Value.PropValue, values.Add))
            {
                RunMethods(s);
                CollectionAssert.AreEqual(
                    new object[]
                    {
                        77,
                        null,
                        7,
                        1,
                        2,
                        3,
                        null,
                        6,
                        null,
                        0,
                        1,
                        2,
                        3
                    },
                    values);
            }

            var len = values.Count;
            RunMethods(s);
            Assert.AreEqual(len, values.Count);

            static void RunMethods(Simple3 s)
            {
                s.Simple2Value.Simple1Value.PropValue = 77;
                s.Simple2Value = new Simple2 { };
                s.Simple2Value.Simple1Value = new Simple1 { PropValue = 7 };
                s.Simple2Value.Simple1Value.PropValue = 1;
                s.Simple2Value.Simple1Value.PropValue = 2;
                s.Simple2Value.Simple1Value.PropValue = 3;
                s.Simple2Value = new Simple2 { };
                s.Simple2Value = new Simple2
                {
                    Simple1Value = new Simple1 { PropValue = 6 }
                };
                s.Simple2Value.Simple1Value = null;
                s.Simple2Value.Simple1Value = new Simple1 { };
                s.Simple2Value.Simple1Value.PropValue = 1;
                s.Simple2Value.Simple1Value.PropValue = 2;
                s.Simple2Value.Simple1Value.PropValue = 3;
            }
        }
    }
}
