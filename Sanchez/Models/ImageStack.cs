using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;

namespace Sanchez.Models
{
    /// <summary>
    ///     All images used in a composition.
    /// </summary>
    public class ImageStack : IDisposable
    {
        private bool isDisposed;

        public Image? Underlay { get; set; }
        public Image? Satellite { get; set; }
        public Image? Mask { get; set; }
        public Image? Overlay { get; set; }

        public IEnumerable<Image> All
            => new[] { Underlay, Satellite, Mask, Overlay }
            .Where(image => image != null)!;

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;
            
            if (disposing)
            {
                Underlay?.Dispose();
                Satellite?.Dispose();
                Mask?.Dispose();
                Overlay?.Dispose();
            }

            Underlay = null;
            Satellite = null;
            Mask = null;
            Overlay = null;
            
            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}