using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Models.Projections;
using Sanchez.Test.Common;
using Sanchez.Workflow.Steps.Equirectangular;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Test.Equirectangular
{
    [TestFixture(TestOf = typeof(GetGlobalOffset))]
    public class GetGlobalOffsetTests : AbstractTests
    {
        private GetGlobalOffset _step;
        private RenderOptions Options => GetService<RenderOptions>();

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _step = GetService<GetGlobalOffset>();
        }

        [TestCase(0, 0)]
        [TestCase(null, 180)]
        [TestCase(174, -174)]
        public async Task FullEarth(double? startLongitude, double globalOffset)
        {
            // Create projection range which is overlapping both sides
            _step.Activity = new Activity(new[] { NewRegistration(0, true, true) });

            Options.EquirectangularRender = new EquirectangularRenderOptions(
                false, false, false,
                startLongitude: startLongitude == null
                    ? (Angle?) null
                    : Angle.FromDegrees(startLongitude.Value));

            // Run method under test
            await _step.RunAsync(new StepExecutionContext());

            // Verify results
            Angle.FromRadians(_step.GlobalOffset).Degrees.Should().BeApproximately(globalOffset, Precision);
        }

        [Test]
        public async Task ExplicitLongitudeCrop()
        {
            const int startLongitude = 100;

            _step.Activity = new Activity(new[] { NewRegistration(0) });
            Options.EquirectangularRender = new EquirectangularRenderOptions(
                false, false, false, longitudeRange: new Range(Angle.FromDegrees(startLongitude), Angle.FromDegrees(startLongitude + 50)));

            // Run method under test
            await _step.RunAsync(new StepExecutionContext());

            // Verify results
            Angle.FromRadians(_step.GlobalOffset).Degrees.Should().BeApproximately(-startLongitude, Precision);
        }

        [Test]
        public async Task StitchedOffset()
        {
            _step.Activity = new Activity(new[]
            {
                // Not used as smallest longitude as left overlap
                NewRegistration(0, true),    
                NewRegistration(50),
                NewRegistration(100)
            });

            Options.EquirectangularRender = new EquirectangularRenderOptions(false, false, false);

            // Run method under test
            await _step.RunAsync(new StepExecutionContext());

            // Verify results
            Angle.FromRadians(_step.GlobalOffset).Degrees.Should().BeApproximately(-50, Precision);
        }

        private static Registration NewRegistration(double startLongitude, bool overlappingLeft = false, bool overlappingRight = false) =>
            new Registration("", new SatelliteDefinition("", null, null, false, 0, new Range(0, 0), new Range(0, 0)), null)
            {
                LongitudeRange = new ProjectionRange(new Range(Angle.FromDegrees(startLongitude).Radians, 0), overlappingLeft, overlappingRight)
            };
    }
}