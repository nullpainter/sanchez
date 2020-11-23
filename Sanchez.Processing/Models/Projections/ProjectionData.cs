using JetBrains.Annotations;
using Sanchez.Processing.Models.Configuration;

namespace Sanchez.Processing.Models
{
    public class ProjectionData
    {
        public ProjectionData(ProjectionType projection, InterpolationType interpolation, int imageSize, string underlayPath)
        {
            Projection = projection;
            Interpolation = interpolation;
            ImageSize = imageSize;
            UnderlayPath = underlayPath;
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
    }
}