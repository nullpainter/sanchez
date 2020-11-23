using Sanchez.Processing.Models.Angles;

namespace Sanchez.Processing.Models.Projections
{
    public class ProjectionRange
    {
        public ProjectionRange(Range range, bool overlappingLeft = false, bool overlappingRight = false)
        {
            Range = range;
            OverlappingLeft = overlappingLeft;
            OverlappingRight = overlappingRight;
        }
        
        public Range Range { get; set; }
        public bool OverlappingLeft { get; }
        public bool OverlappingRight { get; }
    }
}