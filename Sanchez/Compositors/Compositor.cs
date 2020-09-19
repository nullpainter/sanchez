using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sanchez.Models;

namespace Sanchez.Compositors
{
    public interface ICompositor
    {
        Task<int> ComposeAsync(CancellationToken cancellationToken);
    }

    public class Compositor : ICompositor
    {
        private readonly RenderOptions _options;
        private readonly IGeostationaryCompositor _geostationaryCompositor;
        private readonly IEquirectangularCompositor _equirectangularCompositor;

        public Compositor(
            RenderOptions options,
            IGeostationaryCompositor geostationaryCompositor,
            IEquirectangularCompositor equirectangularCompositor)
        {
            _options = options;
            _geostationaryCompositor = geostationaryCompositor;
            _equirectangularCompositor = equirectangularCompositor;
        }

        public async Task<int> ComposeAsync(CancellationToken cancellationToken)
        {
            switch (_options.Projection)
            {
                // Add underlay to image
                case ProjectionType.Geostationary when _options.GeostationaryRender!.Longitude == null:
                {
                    using var activity = await _geostationaryCompositor.ComposeAsync(cancellationToken);
                    return activity?.Rendered ?? 0;
                }

                case ProjectionType.Geostationary:
                {
                    using var activity = _equirectangularCompositor.CreateActivity();
                    using var projected = await _equirectangularCompositor.ComposeStitchedAsync(activity, false, cancellationToken);
                    if (projected == null) return activity.Rendered;

                    await _geostationaryCompositor.RenderProjectedAsync(activity, projected);
                    return activity.Rendered;
                }
                case ProjectionType.Equirectangular:
                {
                    if (_options.StitchImages)
                    {
                        using var activity = _equirectangularCompositor.CreateActivity();
                        var projected = await _equirectangularCompositor.ComposeStitchedAsync(activity, true, cancellationToken); 
                        projected?.Dispose();
                        
                        return activity.Rendered;
                    }

                    var activities = _equirectangularCompositor.CreateActivities();
                    foreach (var activity in activities)
                    {
                        await _equirectangularCompositor.ComposeAsync(activity, activities.Count, cancellationToken); 
                    }

                    return activities.Sum(a => a.Rendered);
                }
                default:
                    throw new InvalidOperationException($"Unhandled projection: {_options.Projection}");
            }
        }
    }
}