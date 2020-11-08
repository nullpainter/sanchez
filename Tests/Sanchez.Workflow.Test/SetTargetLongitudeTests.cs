using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Models;
using Sanchez.Test.Common;
using Sanchez.Workflow.Steps.Geostationary.Reprojected;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Test
{
    [TestFixture(TestOf = typeof(SetTargetLongitude))]
    public class SetTargetLongitudeTests : AbstractTests
    {
        private SetTargetLongitude _step;
        private RenderOptions Options => GetService<RenderOptions>();

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _step = GetService<SetTargetLongitude>();
        }

        [Test]
        public void MapLongitudeNoInterval()
        {
             Options.GeostationaryRender = new GeostationaryRenderOptions(Angle.FromDegrees(100), null, false, 0, 1);
             _step.TargetTimestamp = DateTime.Now;

             _step.Run(new StepExecutionContext());

             _step.Longitude.Should().Be(Angle.FromDegrees(100).Radians);
        }

        [Test]
        public void MapLongitudeWithInterval()
        {
            Options.GeostationaryRender = new GeostationaryRenderOptions(Angle.FromDegrees(130), Angle.FromDegrees(100), false, 0, 1);

            var intervals = CreateIntervals(3);
            _step.TimeIntervals = intervals;

            Options.Timestamp = intervals[0];

            ExecuteAndVerifyLongitude(intervals[0], 130);
            ExecuteAndVerifyLongitude(intervals[1], 115);
            ExecuteAndVerifyLongitude(intervals[2], 100);
        }
        
        [Test]
        public void MapLongitudeWithIntervalInvese()
        {
            Options.GeostationaryRender = new GeostationaryRenderOptions(Angle.FromDegrees(100), Angle.FromDegrees(130), true, 0, 1);

            var intervals = CreateIntervals(3);
            _step.TimeIntervals = intervals;

            Options.Timestamp = intervals[0];

            ExecuteAndVerifyLongitude(intervals[0], 100);
            ExecuteAndVerifyLongitude(intervals[1], 115);
            ExecuteAndVerifyLongitude(intervals[2], 130);
        }

        [Test]
        [Description("Verifies correct longitude when going from positive to negative")]
        public void MapLongitudeWithIntervalWrapping()
        {
            Options.GeostationaryRender = new GeostationaryRenderOptions(Angle.FromDegrees(150), Angle.FromDegrees(-50), false, 0, 1);

            var intervals = CreateIntervals(5);
            _step.TimeIntervals = intervals;

            Options.Timestamp = intervals[0];

            ExecuteAndVerifyLongitude(intervals[0], 150);
            ExecuteAndVerifyLongitude(intervals[1], 100);
            ExecuteAndVerifyLongitude(intervals[2], 50);
            ExecuteAndVerifyLongitude(intervals[3], 0);
            ExecuteAndVerifyLongitude(intervals[4], -50);
        }
        
        [Test]
        [Description("Verifies correct longitude when going from positive to negative")]
        public void MapLongitudeWithIntervalWrappingInverse()
        {
            Options.GeostationaryRender = new GeostationaryRenderOptions(Angle.FromDegrees(150), Angle.FromDegrees(-50), true, 0, 1);

            var intervals = CreateIntervals(5);
            _step.TimeIntervals = intervals;

            Options.Timestamp = intervals[0];
       
            ExecuteAndVerifyLongitude(intervals[0], 150);
            ExecuteAndVerifyLongitude(intervals[1], -170);
            ExecuteAndVerifyLongitude(intervals[2], -130);
            ExecuteAndVerifyLongitude(intervals[3], -90);
            ExecuteAndVerifyLongitude(intervals[4], -50);
        }

        private List<DateTime> CreateIntervals(int number)
        {
            var intervals = new List<DateTime>();
            for (var i = 0; i < number; i++)
            {
                intervals.Add(DateTime.Today.AddDays(number - i));
            }

            return intervals;
        }

        private void ExecuteAndVerifyLongitude(DateTime targetTimestamp, double expectedLongitude)
        {
            _step.TargetTimestamp = targetTimestamp;
            _step.Run(new StepExecutionContext());

            Assert.NotNull(_step.Longitude);
            Angle.FromRadians(_step.Longitude!.Value).Degrees.Should().BeApproximately(expectedLongitude, Precision);
        }
    }
}