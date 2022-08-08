using Sanchez.Processing.Models.Angles;

namespace Sanchez.Processing.Models.Projections;

public record struct ProjectionRange(AngleRange Range, bool OverlappingLeft = false, bool OverlappingRight = false);