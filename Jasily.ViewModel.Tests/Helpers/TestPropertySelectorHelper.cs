using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Jasily.ViewModel.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jasily.ViewModel.Tests.Helpers
{
    [TestClass]
    public class TestPropertySelectorHelper
    {
        class X
        {
            public Y YP { get; }
        }

        class Y
        {
            public Y YA { get; }
        }

        [TestMethod]
        public void TestGetPropertyNames()
        {
            CollectionAssert.AreEqual(
                new[] { nameof(X.YP), nameof(Y.YA), nameof(Y.YA), nameof(Y.YA) }, 
                PropertySelectorHelper.GetPropertyNames<X>(x => x.YP.YA.YA.YA));

            CollectionAssert.AreEqual(
                new[] { nameof(X.YP), nameof(X.YP), nameof(Y.YA), nameof(Y.YA) },
                PropertySelectorHelper.GetPropertyNames<X>(x => (((object) x.YP) as X).YP.YA.YA));
        }
    }
}
