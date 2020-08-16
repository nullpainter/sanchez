using System;
using Funhouse.Extensions;
using Funhouse.Models.Angles;
using Funhouse.Models.Configuration;
using Funhouse.Projections;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.ImageProcessing.Projection
{
    public readonly struct ReprojectRowOperation : IRowOperation
    {
        private readonly Image<Rgba32> _source;
        private readonly Image<Rgba32> _target;
        private readonly SatelliteDefinition _definition;
        private readonly RenderOptions _options;
        private readonly PointF _minX;
        
        private static readonly Rgba32 Transparent = new Rgba32(0, 0, 0, 0);

        public ReprojectRowOperation(
            Image<Rgba32> source,
            Image<Rgba32> target,
            SatelliteDefinition definition, 
            PointF minX, 
            RenderOptions options)
        {
            _source = source;
            _target = target;
            _definition = definition;
            _minX = minX;
            _options = options;
        }

        public void Invoke(int y)
        {
            var span = _target.GetPixelRowSpan(y);

            for (var x = 0; x < span.Length; x++)
            {
                // Convert image x,y to Mercator projection angle
                
                // TODO not entirely sure why we need to flip it vertically
                var projectionAngle = ProjectionAngle.FromPixelCoordinates(new Point(
                    (int) Math.Round(x + _minX.X), _source.Height - y), 
                    _source.Width * 2, _target.Height);
                
                span[x] = ProjectPixel(_source, _definition, projectionAngle);
            }
        }
        
        private Rgba32 ProjectPixel(Image<Rgba32> source, SatelliteDefinition definition, ProjectionAngle projectionAngle)
        {
            // Convert to latitude/longitude
            var geodeticAngle = MercatorProjection.ToGeodetic(projectionAngle);

            // TODO why do we need to do this? Is the previous calculation wrong?
            geodeticAngle.Longitude *= Constants.Earth.RadiusEquator;
            geodeticAngle.Latitude *= Constants.Earth.RadiusPolar;

            return GetTargetColour(source, definition, geodeticAngle);
        }

        private Rgba32 GetTargetColour(Image<Rgba32> source, SatelliteDefinition definition, GeodeticAngle geodeticAngle)
        {
            // Convert latitude/longitude to geostationary scanning angle
            var scanningAngle = GeostationaryProjection.FromGeodetic(geodeticAngle, definition);

            // Ignore pixels outside of disc
            if (scanningAngle == null)
            {
                return _options.Debug ? Constants.DebugColours.OutsideDisc : Transparent;
            }

            // Convert geostationary scanning angle to image coordinates
            var targetPosition = GeostationaryProjection.ToImageCoordinates(scanningAngle.Value, definition);

            // Interpolate fractional source pixel to target image
            return _options.InterpolationType switch
            {
                InterpolationType.NearestNeighbour => source.NearestNeighbour(targetPosition),
                InterpolationType.Bilinear => source.Bilinear(targetPosition),
                _ => throw new ArgumentOutOfRangeException($"Unhandled interpolation type: {_options.InterpolationType}")
            };
        }
    }
}