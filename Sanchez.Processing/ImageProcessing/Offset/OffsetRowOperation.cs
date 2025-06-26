using System.Numerics;
using Sanchez.Processing.Extensions;

namespace Sanchez.Processing.ImageProcessing.Offset;

public class OffsetRowOperation(int amount)
{
    public void Invoke(Span<Vector4> row)
    {
        var buffer = new Vector4[row.Length];
        row.CopyTo(buffer);

        for (var x = 0; x < row.Length; x++)
        {
            var targetOffset = (x - amount).Limit(0, buffer.Length);
            row[x] = buffer[targetOffset];
        }
    }
}