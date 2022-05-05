﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Jasily.ViewModel.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jasily.ViewModel.Tests.Helpers
{
    [TestClass]
    public class TestGetPropertyValueHelper
    {
        class SimpleClass
        {
            public string Value { get; set; }
        }

        [TestMethod]
        public void TestGetPropertyValueDefault()
        {
            Assert.AreEqual("14", GetPropertyValueHelper.GetPropertyValue(new SimpleClass
            {
                Value = "14"
            }, nameof(SimpleClass.Value)));

            Assert.AreEqual("14", GetPropertyValueHelper.GetPropertyValue(new SimpleClass
            {
                Value = "14"
            }, nameof(SimpleClass.Value)));

            Assert.AreEqual("14", GetPropertyValueHelper.GetPropertyValue(new SimpleClass
            {
                Value = "14"
            }, nameof(SimpleClass.Value)));

            Assert.AreEqual("14", GetPropertyValueHelper.GetPropertyValue(new SimpleClass
            {
                Value = "14"
            }, nameof(SimpleClass.Value)));
        }
    }
}
