using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Filesystem;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Filesystem
{
    [TestFixture(TestOf = typeof(SingleFilenameProvider))]
    public class SingleFilenameProviderTests : AbstractTests
    {
        private SingleFilenameProvider FilenameProvider => GetService<SingleFilenameProvider>();

        [Test]
        public void OutputFilenameNonBatch()
        {
            RenderOptions.SourcePath = "source.jpg";
            RenderOptions.OutputPath = Path.Combine("test", "output.jpg");

            var registration = EmptyRegistration("source.jpg");
            var activity = new Activity(new[] { registration });

            var outputFilename = FilenameProvider.GetOutputFilename(activity, registration);
            outputFilename.Should().Be(Path.Combine("test", "output.jpg"));
        }

        [Test]
        public void OutputFilenameBatch()
        {
            RenderOptions.SourcePath = "images/**";
            RenderOptions.OutputPath = Path.Combine("test", "output");

            var registration = EmptyRegistration("source.jpg");
            var activity = new Activity(new[] { registration });

            var outputFilename = FilenameProvider.GetOutputFilename(activity, registration);

            outputFilename.Should().Be(Path.Combine("test", "output", "source-FC.jpg"));
        }

        [Test]
        public void OutputFilenameBatchWthSequence()
        {
            RenderOptions.SourcePath = "images/**";
            RenderOptions.OutputPath = Path.Combine("test", "output");
            RenderOptions.AddSequenceNumber = true;

            var registrations = new List<Registration>();
            for (var i = 0; i < 100; i++)
            {
                registrations.Add(EmptyRegistration($"IMG-{i}.jpg"));
            }

            var activity = new Activity(registrations.ToList());

            var outputFilename = FilenameProvider.GetOutputFilename(activity, registrations[0]);
            outputFilename.Should().Be(Path.Combine("test", "output", "00_IMG-0-FC.jpg"));

            outputFilename = FilenameProvider.GetOutputFilename(activity, registrations[99]);
            outputFilename.Should().Be(Path.Combine("test", "output", "99_IMG-99-FC.jpg"));
        }
    }
}