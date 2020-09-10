namespace Funhouse.Models
{
    public class EquirectangularRenderOptions
    {
        public EquirectangularRenderOptions(bool autoCrop)
        {
            AutoCrop = autoCrop;
        }

        public bool AutoCrop { get; }
    }
}