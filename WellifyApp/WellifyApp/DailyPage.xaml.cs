namespace WellifyApp;

public partial class DailyPage : ContentPage
{
    FirebaseHelper firebaseHelper = new FirebaseHelper();

    public DailyPage()
    {
        InitializeComponent();

        // 1. Set default date (Malaysia Time)
        DateTime malaysiaTime = DateTime.UtcNow.AddHours(8);
        RecordDatePicker.Date = malaysiaTime.Date;
        RecordDatePicker.MaximumDate = malaysiaTime.Date;

        // Subscriptions
        AppData.OnSleepUpdated += UpdateSleepUI;
        AppData.OnMoodUpdated += UpdateMoodUI;
        AppData.OnExerciseUpdated += UpdateExerciseUI;
        AppData.OnWaterUpdated += UpdateWaterUI;
        AppData.OnMealUpdated += UpdateMealUI;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Fix for Line 29: Explicitly cast to DateTime 
        // to resolve the 'DateTime?' to 'DateTime' mismatch
        RefreshData((DateTime)RecordDatePicker.Date);
    }

    private void OnDateChanged(object sender, DateChangedEventArgs e)
    {
        DateTime malaysiaToday = DateTime.UtcNow.AddHours(8).Date;

        if (e.NewDate > malaysiaToday)
        {
            RecordDatePicker.Date = malaysiaToday;
            return;
        }

        if (e.NewDate.HasValue) // ✅ SAFE CHECK
        {
            AppData.SelectedDate = e.NewDate.Value;
            RefreshData(e.NewDate.Value);
        }
    }

    // Ensure this signature remains as standard DateTime
    public async void RefreshData(DateTime targetDate)
    {
        string dateKey = targetDate.ToString("dd-MM-yyyy");

        try
        {
            var record = await firebaseHelper.GetRecord(dateKey);

            if (record != null)
            {
                AppData.SleepHours = record.SleepHours;
                AppData.MoodRating = record.MoodRating;
                AppData.ExerciseValue = record.ExerciseValue;
                AppData.WaterValue = record.WaterValue;
                AppData.MealValue = record.MealValue;
            }
            else
            {
                AppData.SleepHours = null;
                AppData.MoodRating = null;
                AppData.ExerciseValue = null;
                AppData.WaterValue = null;
                AppData.MealValue = null;
            }
        }
        catch { }

        UpdateSleepUI();
        UpdateMoodUI();
        UpdateExerciseUI();
        UpdateWaterUI();
        UpdateMealUI();
    }

    #region Navigation
    private async void GoSleep(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(SleepPage));
    private async void GoMood(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(MoodPage));
    private async void GoExercise(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(ExercisePage));
    private async void GoWater(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(WaterPage));
    private async void GoMeal(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(MealPage));
    #endregion

    #region Visual Feedback
    private void OnArrowPressed(object sender, EventArgs e) => ((Button)sender).Scale = 0.9;
    private void OnArrowReleased(object sender, EventArgs e) => ((Button)sender).Scale = 1;
    #endregion

    #region UI Update Methods
    public void UpdateSleepUI()
    {
        if (SleepStatusLabel == null) return;
        if (!string.IsNullOrEmpty(AppData.SleepHours))
        {
            SleepStatusLabel.Text = $"✔ {AppData.SleepHours} Hour(s)";
            SleepStatusLabel.FontAttributes = FontAttributes.Bold;
            SleepStatusLabel.TextColor = Colors.DarkGreen;
        }
        else
        {
            SleepStatusLabel.Text = "Not entered yet";
            SleepStatusLabel.FontAttributes = FontAttributes.None;
            SleepStatusLabel.TextColor = Colors.Gray;
        }
    }

    public void UpdateMoodUI()
    {
        if (MoodStarsLayout == null) return;
        int rating = 0;
        int.TryParse(AppData.MoodRating, out rating);

        for (int i = 0; i < MoodStarsLayout.Children.Count; i++)
        {
            if (MoodStarsLayout.Children[i] is Label star)
                star.TextColor = i < rating ? Colors.DarkGreen : Colors.Gray;
        }
    }

    public void UpdateExerciseUI()
    {
        if (ExerciseStatusLabel == null) return;
        if (!string.IsNullOrEmpty(AppData.ExerciseValue))
        {
            ExerciseStatusLabel.Text = $"✔ {AppData.ExerciseValue} Minute(s)";
            ExerciseStatusLabel.FontAttributes = FontAttributes.Bold;
            ExerciseStatusLabel.TextColor = Colors.DarkGreen;
        }
        else
        {
            ExerciseStatusLabel.Text = "Not entered yet";
            ExerciseStatusLabel.TextColor = Colors.Gray;
        }
    }

    public void UpdateWaterUI()
    {
        if (WaterStatusLabel == null) return;
        if (!string.IsNullOrEmpty(AppData.WaterValue))
        {
            WaterStatusLabel.Text = $"✔ {AppData.WaterValue} Liter(s)";
            WaterStatusLabel.FontAttributes = FontAttributes.Bold;
            WaterStatusLabel.TextColor = Colors.DarkGreen;
        }
        else
        {
            WaterStatusLabel.Text = "Not entered yet";
            WaterStatusLabel.TextColor = Colors.Gray;
        }
    }

    public void UpdateMealUI()
    {
        if (MealStatusLabel == null) return;
        if (!string.IsNullOrEmpty(AppData.MealValue))
        {
            MealStatusLabel.Text = $"✔ {AppData.MealValue}";
            MealStatusLabel.FontAttributes = FontAttributes.Bold;
            MealStatusLabel.TextColor = Colors.DarkGreen;
        }
        else
        {
            MealStatusLabel.Text = "Not entered yet";
            MealStatusLabel.TextColor = Colors.Gray;
        }
    }
    #endregion
}