using System;
using NUnit.Framework;
using System.Threading;
using Eto.Forms;
using System.Threading.Tasks;

#if INCLUDE_TESTS

// Example tests for.. testing puproses

[assembly: Parallelizable(ParallelScope.All)]


namespace Eto.UnitTest.NUnit.TheTests
{
    [TestFixture]
    public class SomeTests
    {
        [Category("CategoryA")]
        [Test]
        public void Blar()
        {
            Thread.Sleep(1000);
        }

        [Category("CategoryB")]
        [Test]
        public void ShouldNotPass()
        {
            Thread.Sleep(1000);
            throw new InvalidOperationException("woo");
        }
    }

    [TestFixture]
    public class TestStatus
    {
        [Test]
        public void TestShouldError()
        {
            Thread.Sleep(200);
            throw new InvalidOperationException("this is a non-test exception");
        }

        [Test]
        public void TestShouldFail()
        {
            Thread.Sleep(200);
            Assert.AreEqual(10, 20);
        }

        [Test]
        public void TestShouldPass()
        {
            Thread.Sleep(200);
            Assert.AreEqual(10, 10);
        }

        [Test]
        [Ignore("Ignore reason")]
        public void TestShouldSkip()
        {
            Thread.Sleep(200);
            throw new InvalidOperationException("boo");
        }

        [TestCase(3)]
        [TestCase(5)]
        [TestCase(6, Ignore = "Not working")]
        [TestCase(8)]
        public void TestTheory(int value)
        {
            Thread.Sleep(200);
            Assert.True(value % 2 == 1);
        }
    }
}

namespace Eto.UnitTest.NUnit.OtherTests
{
    [TestFixture]
    public class SomeOtherTests
    {
        [Category("CategoryA")]
        [Test]
        public void Test1()
        {
            Thread.Sleep(1000);
        }

        [Test]
        public void Test2()
        {
            Thread.Sleep(1000);
            throw new InvalidOperationException("woo");
        }
    }

    [TestFixture]
    public class MoreTests
    {
        [Test]
        public void Test1()
        {
            Thread.Sleep(1000);
        }

        [Test]
        public void Test2()
        {
            Thread.Sleep(1000);
        }

        [Test]
        public void Test3()
        {
            Thread.Sleep(1000);
        }

        [Test]
        public void Test4()
        {
            Thread.Sleep(1000);
        }
    }

    [TestFixture]
    public class UITests
    {
        [Test]
        [InvokeOnUI]
        public void RunThisOnUIThread()
        {
            Thread currentThread = Thread.CurrentThread;
            Thread uiThread = null;
            Application.Instance.Invoke(() =>
            {
                uiThread = Thread.CurrentThread;
            });

            Assert.IsNotNull(uiThread, "#1. UI thread was not found");
            Assert.AreEqual(currentThread.ManagedThreadId, uiThread.ManagedThreadId, "#2. UI Thread should be the same as the test thread");
        }

        [Test]
        public void RunThisOnTestThread()
        {
            Thread currentThread = Thread.CurrentThread;
            Thread uiThread = null;
            Application.Instance.Invoke(() =>
            {
                uiThread = Thread.CurrentThread;
            });

            Assert.IsNotNull(uiThread, "#1. UI thread was not found");
            Assert.AreNotEqual(currentThread.ManagedThreadId, uiThread.ManagedThreadId, "#2. UI Thread should not be the same as the test thread");
        }
    }
}

#endif