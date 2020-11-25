namespace Sanchez.Processing.Models
{
    public class EquirectangularRenderOptions
    {
        public EquirectangularRenderOptions(
            bool autoCrop, 
            bool noCrop,
            bool stitchImages, 
            Extents? extents)
        {
            AutoCrop = autoCrop;
            NoCrop = noCrop;
            StitchImages = stitchImages;
            Extents = extents;
        }

        public bool AutoCrop { get; }
        public bool NoCrop { get; }

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