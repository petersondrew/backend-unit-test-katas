using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Katas.Test
{
    public class SampleTests: IDisposable
    {
        private readonly IDisposable disposableThing;
        private readonly Random rand;
        private int wannaRace;

        public SampleTests()
        {
            disposableThing = new MemoryStream();
            rand = new Random(42);
        }

        [Fact]
        public void One_plus_one_is_two()
        {
            // XUnit built-in assertions
            Assert.True(1 + 1 == 2);
            // Fluent assertions
            (1 + 1).Should().Be(2);
        }

        [Theory]
        [InlineData(1, 1, 2)]
        [InlineData(2, 2, 4)]
        public void Simple_addition_should_work(int a, int b, int expected)
        {
            var actual = a + b;
            actual.Should().Be(expected, "because addition should *just work*");
        }

        public static IEnumerable<object[]> testData = new[]
        {
            new object[] {1, 1, 2},
            new object[] {2, 2, 4},
        };

        [Theory]
        [MemberData(nameof(testData))]
        public void Simple_addition_with_member_data(int a, int b, int expected)
        {
            var actual = a + b;
            actual.Should().Be(expected, "because addition should *just work*");
        }

        public static IEnumerable<object[]> GetTestData(int seed)
        {
            // Pretend we have some legit logic here
            yield return new object[] { seed, seed, seed + seed };
        }

        [Theory]
        [MemberData(nameof(GetTestData), 1)]
        [MemberData(nameof(GetTestData), 2)]
        public void Simple_addition_with_member_data_function(int a, int b, int expected)
        {
            var actual = a + b;
            actual.Should().Be(expected, "because addition should *just work*");
        }

        [Fact]
        public void Should_fail()
        {
            false.Should().BeTrue("because the world is upside-down");
        }

        [Fact]
        public async Task Async_test_work_just_fine()
        {
            await Task.Delay(10);
            Assert.True(true);
        }

        [Fact]
        public async Task No_race_conditions()
        {
            wannaRace = 10;
            await Task.Delay(rand.Next(50));
            wannaRace += 20;
            wannaRace.Should().Be(30);
        }

        [Fact]
        public async Task Seriously_no_race_conditions()
        {
            wannaRace = 20;
            await Task.Delay(rand.Next(50));
            wannaRace += 20;
            wannaRace.Should().Be(40);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    disposableThing?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
