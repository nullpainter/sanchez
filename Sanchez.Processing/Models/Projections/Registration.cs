using Sanchez.Processing.Models.Configuration;

namespace Sanchez.Processing.Models.Projections;

/// <summary>
///     Wrapper around satellite imagery, containing the image itself and associated metadata used
///     for interpreting and reprojecting the image.
/// </summary>
public sealed class Registration : IDisposable
{
    public Registration(string path, SatelliteDefinition definition, DateTime? timestamp)
    {
        Path = path;
        Definition = definition;
        Timestamp = timestamp;
    }

    public SatelliteDefinition Definition { get; }

    /// <summary>
    ///     Timestamp of image, extracted from filename.
    /// </summary>
    public DateTime? Timestamp { get; }

    private Image<Rgba32>? _image;

    public Image<Rgba32>? Image
    {
        get => _image;
        private set
        {
            _image?.Dispose();
            _image = value;
        }
    }

    public int Width => Image!.Width;
    public int Height => Image!.Height;

    /// <summary>
    ///     Full path to source image.
    /// </summary>
    public string Path { get; }

    /// <summary>
    ///     Horizontal offset when rendering on an equirectangular map.
    /// </summary>
    public int OffsetX { get; set; }

    /// <summary>
    ///     Visible longitude range of satellite, minus overlapping ranges if required.
    /// </summary>
    public ProjectionRange? LongitudeRange { get; set; }

    /// <summary>
    ///     Visible latitude range of satellite.
    /// </summary>
    public ProjectionRange? LatitudeRange { get; set; }

    /// <summary>
    ///     Path to output file, if saving an image per satellite image.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    ///     Whether the horizontal crop bounds require flipping due to image alignment.
    /// </summary>
    public bool FlipHorizontalCrop { get; set; }

    public void Dispose()
    {
        Image?.Dispose();
        Image = null;
    }

    public async Task LoadAsync(CancellationToken ct)
    {
        if (Image != null) throw new InvalidOperationException("Image has already been loaded");
        
        // Prefer contiguous image buffer in order to improve performance of image operations
        SixLabors.ImageSharp.Configuration.Default.PreferContiguousImageBuffers = true;

        try
        {
            var sourceImage = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(Path, ct);
            
            // Create separate colour image to avoid the colour space of the target image being a single channel for GOES images
            Image = new Image<Rgba32>(sourceImage.Width, sourceImage.Height);
            Image.Mutate(context => context.DrawImage(sourceImage, PixelColorBlendingMode.Normal, 1));
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Unable to load image: {Path}", e);
        }
    }
}