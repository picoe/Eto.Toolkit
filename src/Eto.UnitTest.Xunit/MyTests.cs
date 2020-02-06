using System;
using Xunit;
using System.Threading;

#if INCLUDE_TESTS

// Example tests for.. testing puproses

namespace Eto.UnitTest.Xunit.TheTests
{
    public class SomeTests
    {
        [Trait("Category", "CategoryA")]
        [Fact]
        public void Blar()
        {
            Thread.Sleep(1000);
        }

        [Trait("Category", "CategoryB")]
        [Fact]
        public void ShouldNotPass()
        {
            Thread.Sleep(1000);
            throw new InvalidOperationException("woo");
        }
    }

    public class TestStatus
    {
        [Fact]
        public void TestShouldError()
        {
            Thread.Sleep(200);
            throw new InvalidOperationException("this is a non-test exception");
        }

        [Fact]
        public void TestShouldFail()
        {
            Thread.Sleep(200);
            Assert.Equal(10, 20);
        }

        [Fact]
        public void TestShouldPass()
        {
            Thread.Sleep(200);
            Assert.Equal(10, 10);
        }

        [Fact(Skip = "Skip reason")]
        public void TestShouldSkip()
        {
            Thread.Sleep(200);
            throw new InvalidOperationException("boo");
        }

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(6, Skip = "Not working")]
        [InlineData(8)]
        public void TestTheory(int value)
        {
            Thread.Sleep(200);
            Assert.True(value % 2 == 1);
        }
    }
}

namespace Eto.UnitTest.Xunit.OtherTests
{
    public class SomeOtherTests
    {
        [Trait("Category", "CategoryA")]
        [Fact]
        public void Test1()
        {
            Thread.Sleep(1000);
        }

        [Fact]
        public void Test2()
        {
            Thread.Sleep(1000);
            throw new InvalidOperationException("woo");
        }
    }

    public class MoreTests
    {
        [Fact]
        public void Test1()
        {
            Thread.Sleep(1000);
        }

        [Fact]
        public void Test2()
        {
            Thread.Sleep(1000);
        }

        [Fact]
        public void Test3()
        {
            Thread.Sleep(1000);
        }

        [Fact]
        public void Test4()
        {
            Thread.Sleep(1000);
        }
    }
}

#endif