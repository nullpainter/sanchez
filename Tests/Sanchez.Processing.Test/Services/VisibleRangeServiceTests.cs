using System;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Models;
using Sanchez.Processing.Services;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Services
{
    [TestFixture(TestOf = typeof(VisibleRangeService))]
    public class VisibleRangeServiceTests : AbstractTests
    {
        private IVisibleRangeService VisibleRangeService => GetService<IVisibleRangeService>();

        [Test]
        public void GetVisibleRangeGoes17()
        {
            const double longitude = -137.2;
            var range = VisibleRangeService.GetVisibleRange(Angle.FromDegrees(longitude));

            range.Start.Should().BeApproximately(Angle.FromDegrees(142.088718).Radians, Precision);
            range.End.Should().BeApproximately(Angle.FromDegrees(-56.48871).Radians, Precision);
        }
    }
}