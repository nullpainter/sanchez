namespace Funhouse.Models.Configuration
{
    public class RenderOptions
    {
        public InterpolationType InterpolationType { get; }
        public bool Debug { get; }

        public RenderOptions(InterpolationType interpolationType, bool debug)
        {
            InterpolationType = interpolationType;
            Debug = debug;
        }
    }
}