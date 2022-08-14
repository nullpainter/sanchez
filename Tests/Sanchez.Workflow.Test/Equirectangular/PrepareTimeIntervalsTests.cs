using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Test.Common;
using Sanchez.Workflow.Steps.Equirectangular.Timelapse;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Test.Equirectangular;

[TestFixture(TestOf = typeof(PrepareTimeIntervals))]
public class PrepareTimeIntervalsTests : AbstractTests
{
    private PrepareTimeIntervals _step;
    private RenderOptions Options => GetService<RenderOptions>();

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _step = GetService<PrepareTimeIntervals>();

        Options.Timestamp = null;
        Options.EndTimestamp = null;
    }

    [Test]
    public async Task NoStartOrEndTimestamp()
    {
        _step.SourceRegistrations = CreateRegistrations();
        Options.Interval = TimeSpan.FromHours(1);

        // Run method under test
        await _step.RunAsync(new StepExecutionContext());

        _step.TimeIntervals.Should().BeEquivalentTo(new List<DateTime> { DateTime.Today.AddHours(-2), DateTime.Today.AddHours(-1) });
    }

    [Test]
    public async Task NoEndTimestamp()
    {
        _step.SourceRegistrations = CreateRegistrations();
        Options.Interval = TimeSpan.FromHours(1);
        Options.Timestamp = DateTime.Today.AddHours(-1);

        await _step.RunAsync(new StepExecutionContext());

        _step.TimeIntervals.Should().BeEquivalentTo(new List<DateTime> { DateTime.Today.AddHours(-1) });
    }

    [Test]
    public async Task StartAndEndTimestamp()
    {
        _step.SourceRegistrations = CreateRegistrations();
        Options.Interval = TimeSpan.FromHours(1);
        Options.Timestamp = DateTime.Today.AddHours(-1);
        Options.EndTimestamp = DateTime.Today.AddHours(2);

        // Run method under test
        await _step.RunAsync(new StepExecutionContext());

        _step.TimeIntervals.Should().BeEquivalentTo(new List<DateTime> { DateTime.Today.AddHours(-1), DateTime.Today.AddHours(0), DateTime.Today.AddHours(1) });
    }

    private List<Registration> CreateRegistrations()
    {
        return new()
        {
            new Registration("first.jpg", null!, null),
            new Registration("second.jpg", null!, DateTime.Today.AddHours(-1)),
            new Registration("third.jpg", null!, DateTime.Today),
            new Registration("fourth.jpg", null!, DateTime.Today.AddHours(-2))
        };
    }
}