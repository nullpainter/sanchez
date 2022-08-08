namespace Sanchez.Processing.Extensions;

public static class MathExtensions
{
    public static double Limit(this double value, double min, double max) => ((value - min) % (max - min) + (max - min)) % (max - min) + min;
    public static int Limit(this int value, int min, int max) => ((value - min) % (max - min) + (max - min)) % (max - min) + min;

    public static double ClosestTo(this List<double> collection, double target)
    {
        if (!collection.Any()) throw new InvalidOperationException("Collection is empty");

        var closest = double.MaxValue;
        var minDifference = double.MaxValue;

        foreach (var element in collection)
        {
            var difference = Math.Abs(element - target);
            if (minDifference <= difference) continue;
            minDifference = difference;
            closest = element;
        }

        return closest;
    }
}