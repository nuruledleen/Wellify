namespace WellifyApp;

public static class AppData
{
    public static DateTime? SelectedDate { get; set; } = DateTime.UtcNow.AddHours(8).Date;
    public static string SleepHours { get; set; }
    public static string MoodRating { get; set; }
    public static string ExerciseValue { get; set; }
    public static string WaterValue { get; set; }
    public static string MealValue { get; set; }

    // Events to refresh the DailyPage UI
    public static event Action OnSleepUpdated;
    public static event Action OnMoodUpdated;
    public static event Action OnExerciseUpdated;
    public static event Action OnWaterUpdated;
    public static event Action OnMealUpdated;

    public static void NotifySleepUpdated() => OnSleepUpdated?.Invoke();
    public static void NotifyMoodUpdated() => OnMoodUpdated?.Invoke();
    public static void NotifyExerciseUpdated() => OnExerciseUpdated?.Invoke();
    public static void NotifyWaterUpdated() => OnWaterUpdated?.Invoke();
    public static void NotifyMealUpdated() => OnMealUpdated?.Invoke();

    public static int CalculateScore()
    {
        int total = 0;

        // 1. Sleep (Max 20)
        if (double.TryParse(SleepHours, out double s) && s > 0)
        {
            if (s >= 7 && s <= 8) total += 20;      // Optimal
            else if (s > 8) total += 15;            // Oversleeping
            else if (s >= 5 && s < 7) total += 10;  // Moderate
            else total += 5;                        // Severe lack (but > 0)
        }

        // 2. Mood (Max 20)
        if (int.TryParse(MoodRating, out int m) && m > 0)
        {
            total += (m * 4); // 1=4, 2=8, 3=12, 4=16, 5=20
        }

        // 3. Exercise (Max 20)
        if (double.TryParse(ExerciseValue, out double e) && e > 0)
        {
            if (e > 40) total += 20;
            else if (e > 20) total += 15;
            else total += 10;
        }

        // 4. Water (Max 20)
        if (double.TryParse(WaterValue, out double w) && w > 0)
        {
            if (w > 2) total += 20;
            else if (w >= 1) total += 12;
            else total += 5; // Minimal water (but > 0)
        }

        // 5. Meals (Max 20)
        if (!string.IsNullOrWhiteSpace(MealValue) && MealValue != "None")
        {
            total += MealValue.ToLower().Trim() switch
            {
                "mostly healthy" => 20,
                "mixed/average" => 12,
                "mostly unhealthy" => 5,
                _ => 0
            };
        }

        return total;
    }
}