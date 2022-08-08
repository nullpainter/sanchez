namespace Sanchez.Processing.Models.Options;

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