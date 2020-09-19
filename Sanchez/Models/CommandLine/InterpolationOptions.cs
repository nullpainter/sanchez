namespace Sanchez.Models.CommandLine
{
    public enum InterpolationOptions
    {
        /// <summary>
        /// Nearest-neighbour. Fastest, but more artifacts.
        /// </summary>
        N,
        
        /// <summary>
        ///     Bilinear. Smoother, but fewer artifacts.
        /// </summary>
        B
    }
}