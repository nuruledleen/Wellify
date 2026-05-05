public class ChartDrawable : IDrawable
{
    public List<double> Scores { get; set; } = new List<double>();
    private readonly string[] Days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // 1. Setup Margins (Leaves space for labels)
        float leftMargin = 40;
        float bottomMargin = 30;
        float topMargin = 20;
        float rightMargin = 30;

        float width = dirtyRect.Width - leftMargin - rightMargin;
        float height = dirtyRect.Height - topMargin - bottomMargin;
        float stepX = width / 6;

        // 2. Draw Y-Axis Labels (0, 20, 40, 60, 80, 100)
        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 10;

        for (int i = 0; i <= 5; i++)
        {
            int labelValue = i * 20;
            float y = topMargin + height - (float)(labelValue / 100.0 * height);

            // Draw the number
            canvas.DrawString(labelValue.ToString(), 0, y - 10, leftMargin - 10, 20, HorizontalAlignment.Right, VerticalAlignment.Center);

            // Draw the light horizontal grid line
            canvas.StrokeColor = Colors.LightGray;
            canvas.StrokeSize = 0.5f;
            canvas.DrawLine(leftMargin, y, leftMargin + width, y);
        }

        if (Scores == null || Scores.Count == 0) return;

        // 3. Draw X-Axis Labels (Mon - Sun)
        for (int i = 0; i < 7; i++)
        {
            float x = leftMargin + (i * stepX);
            canvas.DrawString(Days[i], x - 15, topMargin + height + 5, 30, 20, HorizontalAlignment.Center, VerticalAlignment.Top);
        }

        // 4. Prepare Data Points
        var points = new List<PointF>();
        for (int i = 0; i < Scores.Count; i++)
        {
            float x = leftMargin + (i * stepX);
            float y = topMargin + height - (float)(Scores[i] / 100.0 * height);
            points.Add(new PointF(x, y));
        }

        // 5. Draw the Line
        canvas.StrokeColor = Color.FromArgb("#af006b");
        canvas.StrokeSize = 2;
        for (int i = 0; i < points.Count - 1; i++)
        {
            // Only draw line if the next day has data (score > 0)
            if (Scores[i + 1] > 0)
                canvas.DrawLine(points[i], points[i + 1]);
        }

        // 6. Draw Dots (Diamonds)
        canvas.FillColor = Color.FromArgb("#2e748f");
        foreach (var p in points)
        {
            // Drawing a small diamond shape for each point
            PathF diamond = new PathF();
            diamond.MoveTo(p.X, p.Y - 4); // Top
            diamond.LineTo(p.X + 4, p.Y); // Right
            diamond.LineTo(p.X, p.Y + 4); // Bottom
            diamond.LineTo(p.X - 4, p.Y); // Left
            diamond.Close();
            canvas.FillPath(diamond);
        }
    }
}