namespace Sanchez.Processing.Models
{
    public class EquirectangularRenderOptions
    {
        public EquirectangularRenderOptions(bool autoCrop, bool stitchImages, Extents? extents)
        {
            AutoCrop = autoCrop;
            StitchImages = stitchImages;
            Extents = extents;
        }

        public bool AutoCrop { get; }

        /// <summary>
        ///     Whether multiple source files are to be stitched together or rendered individually.
        /// </summary>
        public bool StitchImages { get; }

        /// <summary>
        ///     Manual crop extents.
        /// </summary>
        public Extents? Extents { get; }
    }
}