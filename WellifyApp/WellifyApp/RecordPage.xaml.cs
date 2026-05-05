using System.Collections.ObjectModel;

namespace WellifyApp
{
    public partial class RecordPage : ContentPage
    {
        private readonly FirebaseHelper _firebaseHelper = new FirebaseHelper();
        public ObservableCollection<CalendarDay> Days { get; set; } = new();
        private DateTime _displayDate;
        private DateTime _malaysiaToday;
        private CalendarDay _selectedDay;

        public RecordPage()
        {
            InitializeComponent();

            // Setup Malaysia Time (UTC+8)
            var myTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            _malaysiaToday = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, myTimeZone);

            _displayDate = new DateTime(_malaysiaToday.Year, _malaysiaToday.Month, 1);

            CalendarCollection.ItemsSource = Days;
            GenerateCalendar();
        }

        private async void GenerateCalendar()
        {
            Days.Clear();
            _selectedDay = null;
            MonthYearLabel.Text = _displayDate.ToString("MMMM yyyy");

            // Get all records once
            var allRecords = await _firebaseHelper.GetAllRecords();
            var recordKeys = allRecords.Select(r => r.DateKey).ToHashSet();

            // Calculate offset for Monday start (Monday=0, Sunday=6)
            int dayOffset = ((int)_displayDate.DayOfWeek + 6) % 7;

            // Add Padding
            for (int i = 0; i < dayOffset; i++)
                Days.Add(new CalendarDay { IsEmpty = true });

            // Add Actual Days
            int daysInMonth = DateTime.DaysInMonth(_displayDate.Year, _displayDate.Month);
            for (int i = 1; i <= daysInMonth; i++)
            {
                var date = new DateTime(_displayDate.Year, _displayDate.Month, i);

                string key = date.ToString("dd-MM-yyyy");
                var record = allRecords.FirstOrDefault(r => r.DateKey == key);

                bool hasRecord = record != null;
                bool isComplete = false;

                if (record != null)
                {
                    isComplete =
                        !string.IsNullOrWhiteSpace(record.SleepHours) &&
                        !string.IsNullOrWhiteSpace(record.MoodRating) &&
                        !string.IsNullOrWhiteSpace(record.ExerciseValue) &&
                        !string.IsNullOrWhiteSpace(record.WaterValue) &&
                        !string.IsNullOrWhiteSpace(record.MealValue);
                }
                var newDay = new CalendarDay
                {
                    Date = date,
                    IsEmpty = false,
                    IsToday = date.Date == _malaysiaToday.Date,
                    HasRecord = hasRecord,
                    IsComplete = isComplete
                };

                // Auto-select today
                // Inside your GenerateCalendar loop...
                if (newDay.IsToday)
                {
                    newDay.IsSelected = true;
                    _selectedDay = newDay;

                    // Trigger initial fetch for today
                    // We use a Task.Run or call an async method here
                    _ = FetchAndDisplay(newDay);
                }

                Days.Add(newDay);
            }
        }

        private async void OnDateTapped(object sender, EventArgs e)
        {
            var layout = (BindableObject)sender;
            var tappedDay = layout.BindingContext as CalendarDay;

            if (tappedDay == null || tappedDay.IsEmpty) return;

            // UI selection logic
            if (_selectedDay != null)
                _selectedDay.IsSelected = false;

            tappedDay.IsSelected = true;
            _selectedDay = tappedDay;

            // ADD THIS LINE to fetch the data
            await FetchAndDisplay(tappedDay);
        }

        private async Task FetchAndDisplay(CalendarDay day)
        {
            string key = day.Date.ToString("dd-MM-yyyy");
            NoDataLabel.Text = "Loading...";

            var record = await _firebaseHelper.GetRecord(key);

            if (record != null)
            {
                DetailsView.IsVisible = true;
                NoDataLabel.IsVisible = false;
                SelectedDateDisplay.Text = day.Date.ToString("dd MMM yyyy");
                ScoreLabel.Text = record.TotalScore.ToString("F1");
                SleepLabel.Text = $"{record.SleepHours} Hour(s)";
                MoodLabel.Text = $"{record.MoodRating}/5 ★";
                ExerciseLabel.Text = $"{record.ExerciseValue} Minute(s)";
                WaterLabel.Text = $"{record.WaterValue} Liter(s)";
                MealLabel.Text = $"{record.MealValue} Meals";
            }
            else
            {
                DetailsView.IsVisible = false;
                NoDataLabel.IsVisible = true;
                NoDataLabel.Text = $"No record for {day.Date.ToString("dd MMM yyyy")}";
            }
        }

        private void OnPrevMonthClicked(object sender, EventArgs e)
        {
            _displayDate = _displayDate.AddMonths(-1);
            GenerateCalendar();
        }

        private void OnNextMonthClicked(object sender, EventArgs e)
        {
            _displayDate = _displayDate.AddMonths(1);
            GenerateCalendar();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Run cleanup once per day (prevents repeated calls)
            if (Preferences.Get("LastCleanupDate", "") != DateTime.Today.ToString())
            {
                await _firebaseHelper.CleanupOldData();
                Preferences.Set("LastCleanupDate", DateTime.Today.ToString());
            }
        }
    }
}