using System;

namespace Sanchez.Processing.Models
{
    public class UnderlayMetadata
    {
        /// <remarks>
        ///    This property is named by Dapper mapping convention and cannot be renamed.
        /// </remarks>
        public string Filename { get; }

        /// <remarks>
        ///    This property is named by Dapper mapping convention and cannot be renamed.
        /// </remarks>
        public DateTimeOffset Timestamp { get; }

        public UnderlayMetadata(string filename, string timestamp)
        {
            Filename = filename;
            Timestamp = DateTimeOffset.Parse(timestamp);
        }
    }
}