using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Models.Projections;
using Sanchez.Test.Common;
using Sanchez.Workflow.Steps.Equirectangular;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WorkflowCore.Models;
using Range = Sanchez.Processing.Models.Angles.Range;

namespace Sanchez.Workflow.Test.Equirectangular
{
    [TestFixture(TestOf = typeof(OffsetImage))]
    public class OffsetImageTests : AbstractTests
    {
        private OffsetImage _step;
        private RenderOptions Options => GetService<RenderOptions>();
        private Image<Rgba32> _image;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _step = GetService<OffsetImage>();

            // Create a sample image, filling a region so we can verify offset
            _image = new Image<Rgba32>(100, 100);
            _image.Mutate(c => c.Fill(Color.Red, new Rectangle(0, 0, 50, 50)));
        }

        public override void TearDown()
        {
            base.TearDown();
            _image.Dispose();
        }

        [Test]
        public async Task OffsetFullCoverageWithCrop()
        {
            Options.EquirectangularRender = new EquirectangularRenderOptions(false, false, false, null, new Range(0, 1));

            _step.TargetImage = _image;
            _step.GlobalOffset = 0; // 180 degrees

            var registration = new Registration("", new SatelliteDefinition("", null, null, false, 0, new Range(0, 0), new Range(0, 0)), null)
            {
                // Create projection range which is overlapping both sides
                LongitudeRange = new ProjectionRange(new Range(0, 0), true, true)
            };

            _step.Activity = new Activity(new[] { registration });

            // Sanity check
            _step.Activity.IsFullEarthCoverage().Should().BeTrue();

            VerifyInitialImageState();

            // Run method under test
            await _step.RunAsync(new StepExecutionContext());
            
            // Verify red square has been offset to the top right quadrant
            _image[0, 0].R.Should().Be(0);
            _image[99, 0].R.Should().Be(255);
            _image[0, 99].R.Should().Be(0);
            _image[99, 99].R.Should().Be(0);
            _image[49, 0].R.Should().Be(0);
            _image[50, 0].R.Should().Be(255);
        }

        [Test]
        public async Task OffsetImage()
        {
            Options.EquirectangularRender = new EquirectangularRenderOptions(false, false, false);

            _step.TargetImage = _image;
            _step.GlobalOffset = 0; // 180 degrees

            var registration = new Registration("", new SatelliteDefinition("", null, null, false, 0, new Range(0, 0), new Range(0, 0)), null)
            {
                LongitudeRange = new ProjectionRange(new Range(0, 0))
            };

            _step.Activity = new Activity(new[] { registration });

            VerifyInitialImageState();

            // Run method under test
            await _step.RunAsync(new StepExecutionContext());

            // Verify red square has been offset to the top right quadrant
            _image[0, 0].R.Should().Be(0);
            _image[99, 0].R.Should().Be(255);
            _image[0, 99].R.Should().Be(0);
            _image[99, 99].R.Should().Be(0);
            _image[49, 0].R.Should().Be(0);
            _image[50, 0].R.Should().Be(255);
        }

        private void VerifyInitialImageState()
        {
            //  Red rectangle is rendered in top left quadrant
            _image[0, 0].R.Should().Be(255);
            _image[99, 0].R.Should().Be(0);
            _image[0, 99].R.Should().Be(0);
            _image[99, 99].R.Should().Be(0);
            _image[49, 0].R.Should().Be(255);
            _image[50, 0].R.Should().Be(0);
        }
    }
}