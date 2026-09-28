namespace PixiEditor.Helpers.UI;

public class Easing
{
    public static Easing CubicEaseOut { get; } =
        new(t => 1 - Math.Pow(1 - t, 3));

    public static Easing CubicEaseIn { get; } =
        new(t => t * t * t);

    public static Easing Linear { get; } =
        new(t => t);

    public static Easing BackEaseOut { get; } =
        new(t =>
        {
            const double c1 = 1.70158;
            const double c3 = c1 + 1;

            var x = t - 1;

            return 1 + c3 * x * x * x + c1 * x * x;
        });

    public static Easing BounceEaseOut { get; } =
        new(t =>
        {
            const double n1 = 7.5625;
            const double d1 = 2.75;

            if (t < 1 / d1)
                return n1 * t * t;

            if (t < 2 / d1)
            {
                var x = t - 1.5 / d1;
                return n1 * x * x + 0.75;
            }

            if (t < 2.5 / d1)
            {
                var x = t - 2.25 / d1;
                return n1 * x * x + 0.9375;
            }

            var y = t - 2.625 / d1;

            return n1 * y * y + 0.984375;
        });

    private readonly Func<double, double> _func;

    public Easing(Func<double, double> func)
    {
        _func = func;
    }

    public double Ease(double t) =>
        _func(Math.Clamp(t, 0, 1));
}
