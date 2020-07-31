using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;

namespace Sanchez.Models
{
    /// <summary>
    ///     All images used in a composition.
    /// </summary>
    public class ImageStack
    {
        public Image? Underlay { get; set; }
        public Image? Satellite { get; set; }
        public Image? Mask { get; set; }
        public Image? Overlay { get; set; }

        public IEnumerable<Image> All => new[] { Underlay, Satellite, Mask, Overlay }.Where(image => image != null)!;
    }
}