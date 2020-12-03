using Sanchez.Processing.Models.Angles;

namespace Sanchez.Processing.Models
{
    public class EquirectangularRenderOptions
    {
        public EquirectangularRenderOptions(bool autoCrop,
            bool noCrop,
            bool stitchImages,
            Range? latitudeRange = null,
            Range? longitudeRange = null, 
            Angle? startLongitude = null)
        {
            AutoCrop = autoCrop;
            NoCrop = noCrop;
            StitchImages = stitchImages;
            LatitudeRange = latitudeRange;
            LongitudeRange = longitudeRange;
            StartLongitude = startLongitude;
        }

        public bool AutoCrop { get; }
        public bool NoCrop { get; }

        /// <summary>
        ///     Whether multiple source files are to be stitched together or rendered individually.
        /// </summary>
        public bool StitchImages { get; }

        /// <summary>
        ///     Optional latitude crop.
        /// </summary>
        public Range? LatitudeRange { get; }

        /// <summary>
        ///     Optional longitude crop.
        /// </summary>
        public Range? LongitudeRange { get; }

        /// <summary>
        ///     Optional longitude offset to apply when not performing cropping.
        /// </summary>
        public Angle? StartLongitude { get; }

        /// <summary>
        ///     Whether the user has specified crop bounds.
        /// </summary>
        public bool ExplicitCrop => LatitudeRange != null || LongitudeRange != null;
    }
}