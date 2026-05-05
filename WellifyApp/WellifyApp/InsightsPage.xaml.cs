using System.Globalization;

namespace WellifyApp
{
    public partial class InsightsPage : ContentPage
    {
        private readonly FirebaseHelper _firebaseHelper = new FirebaseHelper();
        private ChartDrawable _chartDrawable = new ChartDrawable();
        private DateTime _currentStartOfWeek;

        public InsightsPage()
        {
            InitializeComponent();
            // Default to the start of the current week (Malaysia Time)
            _currentStartOfWeek = GetMonday(DateTime.UtcNow.AddHours(8));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (Preferences.Get("LastCleanupDate", "") != DateTime.Today.ToString())
            {
                await _firebaseHelper.CleanupOldData();
                Preferences.Set("LastCleanupDate", DateTime.Today.ToString());
            }

            // Set default toggle (Chart selected)
            ChartView.IsVisible = true;
            TableView.IsVisible = false;

            ChartBtn.BackgroundColor = Colors.White;
            ChartBtn.TextColor = Color.FromArgb("#2e748f");

            TableBtn.BackgroundColor = Colors.Transparent;
            TableBtn.TextColor = Colors.White;

            await LoadWeeklyData();
        }

        private DateTime GetMonday(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        public async Task LoadWeeklyData()
        {
            // 1. SET DATE IMMEDIATELY (Fixes the "Loading..." hang)
            DateTime endOfWeek = _currentStartOfWeek.AddDays(6);
            WeekRangeLabel.Text = $"{_currentStartOfWeek:dd MMM yy} - {endOfWeek:dd MMM yy}";

            try
            {
                var records = new List<WellnessRecord>();

                // Fetch 7 days from Firebase
                for (int i = 0; i < 7; i++)
                {
                    string key = _currentStartOfWeek.AddDays(i).ToString("dd-MM-yyyy");
                    var r = await _firebaseHelper.GetRecord(key);
                    if (r != null) records.Add(r);
                }

                // 2. TOGGLE VISIBILITY BASED ON DATA COUNT
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (records.Count == 7) // Need 7 days to show insights
                    {
                        SummaryContainer.IsVisible = true;
                        InsufficientDataLabel.IsVisible = false;
                        UpdateUI(records);
                    }
                    else
                    {
                        SummaryContainer.IsVisible = false;
                        InsufficientDataLabel.IsVisible = true;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fetch Error: {ex.Message}");
            }
        }

        private void OnToggleTapped(object sender, EventArgs e)
        {
            var button = sender as Button;

            bool isChart = button == ChartBtn;

            // Toggle views
            ChartView.IsVisible = isChart;
            TableView.IsVisible = !isChart;

            // Update UI styles
            ChartBtn.BackgroundColor = isChart ? Colors.White : Colors.Transparent;
            ChartBtn.TextColor = isChart ? Color.FromArgb("#2e748f") : Colors.White;

            TableBtn.BackgroundColor = isChart ? Colors.Transparent : Colors.White;
            TableBtn.TextColor = isChart ? Colors.White : Color.FromArgb("#2e748f");
        }

        private void UpdateUI(List<WellnessRecord> records)
        {
            // 1. Fix the Table Data formatting and sorting
            // We parse the string key to a real date, then create a new list for the UI
            RecordsCollectionView.ItemsSource = records
                .Select(r => new
                {
                    DateDisplay = DateTime.ParseExact(r.DateKey, "dd-MM-yyyy", null).ToString("dddd"),
                    r.TotalScore,
                    // Keep the original date for sorting purposes
                    SortDate = DateTime.ParseExact(r.DateKey, "dd-MM-yyyy", null)
                })
                .OrderBy(r => r.SortDate)
                .ToList();

            // 2. Weekly Chart logic (stays mostly the same)
            var weeklyScores = new List<double>();
            for (int i = 0; i < 7; i++)
            {
                string key = _currentStartOfWeek.AddDays(i).ToString("dd-MM-yyyy");
                var dayRecord = records.FirstOrDefault(r => r.DateKey == key);
                weeklyScores.Add(dayRecord?.TotalScore ?? 0);
            }

            _chartDrawable.Scores = weeklyScores;
            WeeklyChart.Drawable = _chartDrawable;
            WeeklyChart.Invalidate();

            // 3. Stats Labels
            double avgScore = records.Count > 0 ? records.Average(r => r.TotalScore) : 0;
            AverageScoreLabel.Text = $"{Math.Round(avgScore)}%";

            var best = records.OrderByDescending(r => r.TotalScore).FirstOrDefault();
            if (best != null && DateTime.TryParseExact(best.DateKey, "dd-MM-yyyy", null, DateTimeStyles.None, out DateTime bDate))
            {
                BestDayLabel.Text = bDate.ToString("dddd");
                // Fix Best Day date format here too
                BestDateLabel.Text = bDate.ToString("dd MMM yyyy");
            }

            UpdateHabitInsights(records);
        }

        private void UpdateHabitInsights(List<WellnessRecord> records)
        {
            double avgSleep = records.Average(r => double.TryParse(r.SleepHours, out double s) ? s : 0);
            SleepDataSpan.Text = avgSleep >= 7
                ? ": You're getting good rest 👍"
                : ": Try to get a bit more sleep 😴";

            double avgExercise = records.Average(r => double.TryParse(r.ExerciseValue, out double e) ? e : 0);
            ExerciseDataSpan.Text = avgExercise >= 30
                ? ": Great job staying active 💪"
                : ": Try to move a little more 🚶";

            double avgWater = records.Average(r => double.TryParse(r.WaterValue, out double w) ? w : 0);
            WaterDataSpan.Text = avgWater >= 2
                ? ": You're well hydrated 💧"
                : ": Drink a bit more water 💦";

            int healthyCount = records.Count(r => r.MealValue != null && r.MealValue.Contains("Healthy"));
            MealDataSpan.Text = $": You had {healthyCount} healthy day(s) 🥗";
        }

        private async void OnPrevWeek(object sender, EventArgs e)
        {
            _currentStartOfWeek = _currentStartOfWeek.AddDays(-7);
            await LoadWeeklyData();
        }

        private async void OnNextWeek(object sender, EventArgs e)
        {
            _currentStartOfWeek = _currentStartOfWeek.AddDays(7);
            await LoadWeeklyData();
        }

    }
}