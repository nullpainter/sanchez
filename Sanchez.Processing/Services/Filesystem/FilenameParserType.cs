namespace Sanchez.Processing.Services.Filesystem
{
    public enum FilenameParserType
    {
        /// <summary>
        ///     Images produced by <c>goesproc</c> for GOES-16, GOES-17 and Himawari-8 images.
        /// </summary>
        Goesproc,

        ///<summary>
        ///     Images produced by <c>xrit-rx</c> for GK-2A images.
        ///  </summary>
        Xrit,
        
        /// <summary>
        ///     Images produced for Electro-L N2 images.
        /// </summary>
        Electro
    }
}