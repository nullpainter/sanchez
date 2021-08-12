using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Newtonsoft.Json;
using NUnit.Framework;
using Sanchez.Processing.Models.Gradients;
using Sanchez.Processing.Services;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Services
{
    [TestFixture(TestOf = typeof(IGradientService))]
    public class GradientServiceTests : AbstractTests
    {
        private IGradientService Service => GetService<IGradientService>();
       
        [Test]
        public async Task MalformedGradientFile()
        {
            await CreateGradientFileAsync("Hi there");
            Assert.ThrowsAsync<JsonReaderException>(() => Service.InitialiseAsync());
        }

        [Test]
        public async Task MissingColour()
        {
            var gradient = new List<RgbColourStop>
            {
                new()
                {
                    Colour = "#ff0000",
                    Position = 0.1f
                },
                new()
                {
                    Position = 0.1f
                },
            };

            await CreateGradientFileAsync(gradient);
            
            var exception = Assert.ThrowsAsync<ValidationException>(async () => await Service.InitialiseAsync());
            Assert.NotNull(exception);
            exception.Message.Should().Be("Colour must be specified");
        }

        [Test]
        public async Task MissingPosition()
        {
            var gradient = new List<RgbColourStop>
            {
                new()
                {
                    Colour = "#ff0000",
                    Position = 0.1f
                },
                new()
                {
                    Colour = "#aa0000"
                },
            };

            await CreateGradientFileAsync(gradient);
            
            var exception = Assert.ThrowsAsync<ValidationException>(async () => await Service.InitialiseAsync());
            Assert.NotNull(exception);
            exception.Message.Should().Be("Position must be specified for all colour stops");
        }
        
                [Test]
                public async Task ValueTooLow()
                {
                    var gradient = new List<RgbColourStop>
                    {
                        new()
                        {
                            Colour = "#ff0000",
                            Position = -0.5f,
                        },
                        new()
                        {
                            Colour = "#aa0000",
                            Position = 1f
                        },
                    };
        
                    await CreateGradientFileAsync(gradient);
                    
                    var exception = Assert.ThrowsAsync<ValidationException>(async () => await Service.InitialiseAsync());
                    Assert.NotNull(exception);
                    exception.Message.Should().Be("-0.5 is an invalid position; valid values are from 0.0 to 1.0");
                }

        [Test]
        public async Task ValueTooHigh()
        {
            var gradient = new List<RgbColourStop>
            {
                new()
                {
                    Colour = "#ff0000",
                    Position = 0.1f
                },
                new()
                {
                    Colour = "#aa0000",
                    Position = 2.3f
                },
            };

            await CreateGradientFileAsync(gradient);
            
            var exception = Assert.ThrowsAsync<ValidationException>(async () => await Service.InitialiseAsync());
            Assert.NotNull(exception);
            exception.Message.Should().Be("2.3 is an invalid position; valid values are from 0.0 to 1.0");
        }

        [Test]
        public async Task InvalidColours()
        {
            var gradient = new List<RgbColourStop>
            {
                new()
                {
                    Colour = "#ff0000",
                    Position = 0.1f
                },
                new()
                {
                    Colour = "#sanchez",
                    Position = 1.0f
                },
            };

            await CreateGradientFileAsync(gradient);
            
            var exception = Assert.ThrowsAsync<ValidationException>(async () => await Service.InitialiseAsync());
            Assert.NotNull(exception);
            exception.Message.Should().Be("Unable to parse #sanchez as a hex triplet");
        }

        private async Task CreateGradientFileAsync(List<RgbColourStop> gradient) 
            => await CreateGradientFileAsync(JsonConvert.SerializeObject(gradient));

        private async Task CreateGradientFileAsync(string json)
        {
            var path = State.CreateFile("malformed.json");
            await File.WriteAllTextAsync(path, json);

            RenderOptions.Overlay.GradientPath = path;
        }
    }
}