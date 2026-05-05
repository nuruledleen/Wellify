namespace WellifyApp
{
    public class ScoreChartDrawable : IDrawable
    {
        public double ScorePercentage { get; set; } = 0;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.Antialias = true;

            float strokeWidth = 50;
            float radius = (Math.Min(dirtyRect.Width, dirtyRect.Height) / 2) - (strokeWidth / 2);
            PointF center = new PointF(dirtyRect.Width / 2, dirtyRect.Height / 2);

            // progress
            canvas.StrokeColor = Color.FromArgb("#a6e5eb");
            canvas.StrokeSize = strokeWidth;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.DrawEllipse(center.X - radius, center.Y - radius, radius * 2, radius * 2);

            float sweepAngle = (float)(360 * (ScorePercentage / 100));

            // the progress arc
            canvas.DrawArc(center.X - radius, center.Y - radius, radius * 2, radius * 2, 90, 90 - sweepAngle, false, false);

            // the dark blue covering the remaining percentage.
            canvas.StrokeColor = Color.FromArgb("#76b7d1");
            canvas.StrokeSize = strokeWidth;
            canvas.StrokeLineCap = LineCap.Round;

            // The dark blue starts where the light blue ends
            float startAngle = 90 - sweepAngle;
            float remainingAngle = 360 - sweepAngle;

            canvas.DrawArc(center.X - radius, center.Y - radius, radius * 2, radius * 2, startAngle, startAngle - remainingAngle, false, false);
        }
    }
}