using System;
using System.Threading;
using System.Threading.Tasks;
using Funhouse.Models;

namespace Funhouse.Compositors
{
    public interface ICompositor
    {
        Task ComposeAsync(CancellationToken cancellationToken);
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

        public async Task ComposeAsync(CancellationToken cancellationToken)
        {
            switch (_options.Projection)
            {
                case ProjectionType.Geostationary:
                {
                    // Add underlay to image
                    if (_options.GeostationaryRender!.Longitude == null)
                    {
                        using var activity = await _geostationaryCompositor.ComposeAsync(cancellationToken);
                        if (activity == null) return;
                    }

                    // TODO global progress bar? Probably makes sense, eh. Register as service, if we can
                    else
                    {
                        var activity = _equirectangularCompositor.CreateActivity();
                        using var projected = await _equirectangularCompositor.ComposeAsync(activity, cancellationToken);
                        if (projected == null) return;

                        await _geostationaryCompositor.RenderProjectedAsync(activity, projected);
                    }

                    break;
                }
                case ProjectionType.Equirectangular:
                {
                    using var projected = await _equirectangularCompositor.ComposeAsync(cancellationToken);
                    if (projected == null) return;

                    break;
                }
                default:
                    throw new InvalidOperationException($"Unhandled projection: {_options.Projection}");
            }
        }
    }
}