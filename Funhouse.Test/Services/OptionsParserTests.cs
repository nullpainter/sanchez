using System;
using FluentAssertions;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Models.Configuration;
using Funhouse.Services;
using NUnit.Framework;
using SixLabors.ImageSharp;

namespace Funhouse.Test.Services
{
    [TestFixture(TestOf = typeof(OptionsParser))]
    public class OptionsParserTests
    {
        [Test]
        public void PopulateGeostationary()
        {
            var options = new GeostationaryOptions
            {
                HazeAmount = 0.5f,
                Longitude = 147,
                Tint = "ff0000",
                SpatialResolution = Constants.Satellite.SpatialResolution.TwoKm,
                InterpolationType = InterpolationOptions.N,
                Force = false,
                Verbose = false,
                Quiet = true
            };
            
            var renderOptions = OptionsParser.Populate(options);
            Assert.NotNull(renderOptions.GeostationaryRender);

            renderOptions.GeostationaryRender.Longitude.Should().Be(147);
            renderOptions.GeostationaryRender.HazeAmount.Should().Be(0.5f);
            renderOptions.InterpolationType.Should().Be(InterpolationType.NearestNeighbour);
            renderOptions.ImageSize.Should().Be(Constants.Satellite.ImageSize.TwoKm);
            renderOptions.ImageOffset.Should().Be(Constants.Satellite.Offset.TwoKm);
            renderOptions.Force.Should().BeFalse();
            renderOptions.Verbose.Should().BeFalse();
            renderOptions.Quiet.Should().BeTrue();
        }
        
        [Test]
        public void PopulateEquirectangular()
        {
            var options = new EquirectangularOptions
            {
                AutoCrop = true,
                Tint = "ff0000",
                SpatialResolution = Constants.Satellite.SpatialResolution.FourKm,
                InterpolationType = InterpolationOptions.B,
                ToleranceMinutes = 30,
                Brightness = 1.2f,
                Saturation = 0.5f,
                Force = true,
                Verbose = true
            };
            
            var renderOptions = OptionsParser.Populate(options);
            Assert.NotNull(renderOptions.EquirectangularRender);

            renderOptions.EquirectangularRender.AutoCrop.Should().BeTrue();
            renderOptions.SpatialResolution.Should().Be(Constants.Satellite.SpatialResolution.FourKm);
            renderOptions.Tint.Should().Be(Color.FromRgb(255, 0, 0));
            renderOptions.InterpolationType.Should().Be(InterpolationType.Bilinear);
            renderOptions.ImageSize.Should().Be(Constants.Satellite.ImageSize.FourKm);
            renderOptions.ImageOffset.Should().Be(Constants.Satellite.Offset.FourKm);
            renderOptions.Tolerance.Should().Be(TimeSpan.FromMinutes(30));
            renderOptions.Brightness.Should().Be(1.2f);
            renderOptions.Saturation.Should().Be(0.5f);
            renderOptions.Force.Should().BeTrue();
            renderOptions.Verbose.Should().BeTrue();
        }
    }
}