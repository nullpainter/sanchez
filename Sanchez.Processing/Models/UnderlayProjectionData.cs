using JetBrains.Annotations;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Models.Projections;
using SixLabors.ImageSharp;

namespace Sanchez.Processing.Models
{
    public class UnderlayProjectionData
    {
        public UnderlayProjectionData(
            ProjectionType projection,
            InterpolationType interpolation,
            string underlayPath,
            int imageSize,
            Size? targetSize = null,
            Range? latitudeCrop = null,
            double? minLongitude = null,
            bool noCrop = false)
        {
            Projection = projection;
            Interpolation = interpolation;
            UnderlayPath = underlayPath;
            ImageSize = imageSize;
            TargetSize = targetSize;
            LatitudeCrop = latitudeCrop;
            MinLongitude = minLongitude;
            NoCrop = noCrop;
        }

        public ProjectionType Projection { get; }

        /// <remarks>
        ///     Used only to disambiguate cache entries.
        /// </remarks>
        public InterpolationType Interpolation { [UsedImplicitly] get; }

        /// <remarks>
        ///     Used only to disambiguate cache entries.
        /// </remarks>
        public int ImageSize { [UsedImplicitly] get; }

        /// <remarks>
        ///     Used only to disambiguate cache entries.
        /// </remarks>
        public string UnderlayPath { [UsedImplicitly] get; }

        public bool NoCrop { [UsedImplicitly] get; }

        public Size? TargetSize { get; }
        public Range? LatitudeCrop { get; }

        public double? MinLongitude { get; }
    }
}