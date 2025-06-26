using JetBrains.Annotations;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Models.Projections;
using SixLabors.ImageSharp;

namespace Sanchez.Processing.Models;

public record UnderlayProjectionData(
    ProjectionType Projection,

    // Used only to disambiguate cache entries
    [UsedImplicitly]
    InterpolationType Interpolation,

    // Used only to disambiguate cache entries
    [UsedImplicitly]
    string UnderlayPath,

    // Used only to disambiguate cache entries
    [UsedImplicitly]
    int ImageSize,
    Size? TargetSize = null,
    AngleRange? LatitudeCrop = null,
    double? MinLongitude = null,
    bool NoCrop = false);