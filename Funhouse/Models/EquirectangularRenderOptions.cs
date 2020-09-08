namespace Funhouse.Models
{
    public class EquirectangularRenderOptions
    {
        public EquirectangularRenderOptions(bool autocrop)
        {
            AutoCrop = autocrop;
        }

        public bool AutoCrop { get; }
    }
}