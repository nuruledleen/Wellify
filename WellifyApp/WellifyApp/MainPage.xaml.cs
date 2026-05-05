using System.Globalization;

namespace WellifyApp
{
    public partial class MainPage : ContentPage
    {
        private readonly AiService _aiService = new AiService();
        private ScoreChartDrawable _scoreDrawable = new ScoreChartDrawable();
        private readonly FirebaseHelper _firebase = new FirebaseHelper();

        public MainPage()
        {
            InitializeComponent();

            // 1. Setup Drawable
            ScoreCircle.Drawable = _scoreDrawable;

            // 2. Set Malaysia Time 
            DateTime malaysiaTime = DateTime.UtcNow.AddHours(8);
            DateLabel.Text = malaysiaTime.ToString("dddd, d MMM yyyy", new CultureInfo("en-MY"));

            // 3. Initial UI state
            SetActive(HomeIcon);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Set greeting every time the page appears
            SetGreeting();

            // Load the data now that the UI is guaranteed to be ready
            await LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            WellnessRecord record = null;

            try
            {
                string todayKey = DateTime.UtcNow.AddHours(8).ToString("dd-MM-yyyy");
                record = await _firebase.GetRecord(todayKey);

                if (record != null)
                {
                    // Sync local data
                    AppData.SleepHours = record.SleepHours ?? "0";
                    AppData.WaterValue = record.WaterValue ?? "0";
                    AppData.ExerciseValue = record.ExerciseValue ?? "0";
                    AppData.MealValue = record.MealValue ?? "None";
                    AppData.MoodRating = record.MoodRating ?? "0";

                    // Use the score from Firebase directly
                    UpdateScoreUI(record.TotalScore);
                }
                else
                {
                    UpdateScoreUI(AppData.CalculateScore());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load Error: {ex.Message}");
                UpdateScoreUI(AppData.CalculateScore());
            }

            double score = record?.TotalScore ?? AppData.CalculateScore();
            UpdateScoreUI(score);

            // Call the REAL AI
            AiMessageLabel.Text = "Loading...";
            string aiInsight = await _aiService.GetWellnessInsight(
                score,
                AppData.SleepHours,
                AppData.WaterValue,
                AppData.ExerciseValue,
                AppData.MealValue,
                AppData.MoodRating
            );

            MainThread.BeginInvokeOnMainThread(() =>
            {
                AiMessageLabel.Text = aiInsight.Trim();
            });
        }

        private void UpdateScoreUI(double score)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ScoreLabel.Text = $"{Math.Round(score)}%";
                _scoreDrawable.ScorePercentage = score;
                ScoreCircle.Invalidate();
            });
        }

        private void SetGreeting()
        {
            int hour = DateTime.UtcNow.AddHours(8).Hour;

            if (hour < 12)
                GreetingLabel.Text = "Good Morning! ☀️";
            else if (hour < 18)
                GreetingLabel.Text = "Good Afternoon! 🌤️";
            else
                GreetingLabel.Text = "Good Evening! 🌙";
        }

        void ShowHome()
        {
            HomeContent.IsVisible = true;
            OtherContent.IsVisible = false;
            SetGreeting();

            // Calculate the score but store it, so it's consistent
            UpdateScoreUI(AppData.CalculateScore());
        }

        async void ShowPage(ContentPage page)
        {
            HomeContent.IsVisible = false;
            OtherContent.IsVisible = true;

            OtherContent.Content = new ContentView
            {
                Content = page.Content
            };

            // MANUAL LIFECYCLE TRIGGER
            if (page is DailyPage daily)
            {
                DateTime malaysiaToday = DateTime.UtcNow.AddHours(8);
                daily.RefreshData(malaysiaToday.Date);
            }
            else if (page is InsightsPage insights)
            {
                // anually telling the summary page to load
                await insights.LoadWeeklyData();
            }
        }

        #region Navigation Methods

        void ResetIcons()
        {
            HomeIcon.Opacity = 0.45;
            RecordIcon.Opacity = 0.45;
            DailyIcon.Opacity = 0.45;
            InsightsIcon.Opacity = 0.45;
            AboutIcon.Opacity = 0.45;
        }

        void SetActive(Image icon)
        {
            ResetIcons();
            icon.Opacity = 1;
        }

        void GoHome(object sender, EventArgs e)
        {
            ShowHome();
            SetActive(HomeIcon);
            PageTitle.Text = "Home";
        }

        void GoRecord(object sender, EventArgs e)
        {
            ShowPage(new RecordPage());
            SetActive(RecordIcon);
            PageTitle.Text = "Record";
        }

        void GoDaily(object sender, EventArgs e)
        {
            ShowPage(new DailyPage());
            SetActive(DailyIcon);
            PageTitle.Text = "Daily Entry";
        }

        void GoInsights(object sender, EventArgs e)
        {
            var insightsPage = new InsightsPage();
            ShowPage(insightsPage);

            SetActive(InsightsIcon);
            PageTitle.Text = "Weekly Summary";
        }

        void GoAbout(object sender, EventArgs e)
        {
            ShowPage(new AboutPage());
            SetActive(AboutIcon);
            PageTitle.Text = "About";
        }

        #endregion
    }
}