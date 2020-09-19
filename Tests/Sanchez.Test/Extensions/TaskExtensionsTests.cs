using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Extensions;
using TaskExtensions = Sanchez.Extensions.TaskExtensions;

namespace Sanchez.Test.Extensions
{
    [TestFixture]
    [TestOf(typeof(TaskExtensions))]
    public class TaskExtensionsTests
    {
        [Test]
        [Repeat(100)]
        public async Task ForEachAsync()
        {
            var models = new List<TestModel> { new TestModel(), new TestModel() };
            await models.ForEachAsync(async a => await Task.Run(() => a.TestValue++), 10);

            models.ForEach(m => m.TestValue.Should().Be(1, "operation should have been performed on each item"));
        }

    

        public class TestModel
        {
            public int TestValue { get; set; }
        }
    }
}