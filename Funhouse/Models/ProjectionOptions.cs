using Funhouse.Models.Configuration;

namespace Funhouse.Models
{
    public class ProjectionOptions
    {
        public ProjectionOptions(ProjectionType projection, InterpolationType interpolation)
        {
            Projection = projection;
            Interpolation = interpolation;
        }

        public ProjectionType Projection { get; }
        public InterpolationType Interpolation { get; }
    }
}