using System.Globalization;
namespace WellifyApp;

public partial class MoodPage : ContentPage
{
    FirebaseHelper firebaseHelper = new FirebaseHelper();
    int selectedRating = 0;
    public MoodPage() => InitializeComponent();

    private async void OnBackTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        DateTime targetDate = AppData.SelectedDate ?? DateTime.UtcNow.AddHours(8);
        DateLabel.Text = targetDate.ToString("dddd, d MMM yyyy", new CultureInfo("en-MY"));

        var record = await firebaseHelper.GetRecord(targetDate.ToString("dd-MM-yyyy"));
        if (record != null && int.TryParse(record.MoodRating, out int rating))
        {
            selectedRating = rating;
            UpdateStars(selectedRating);
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (selectedRating == 0)
        {
            await DisplayAlert("No Rating", "Please select a star.", "OK");
            return;
        }

        DateTime targetDate = AppData.SelectedDate ?? DateTime.UtcNow.AddHours(8);
        string dateKey = targetDate.ToString("dd-MM-yyyy");

        var record = await firebaseHelper.GetRecord(dateKey) ?? new WellnessRecord { DateKey = dateKey };
        record.MoodRating = selectedRating.ToString();
        AppData.MoodRating = record.MoodRating;
        record.TotalScore = AppData.CalculateScore();

        try
        {
            await firebaseHelper.SaveRecord(record);
            AppData.NotifyMoodUpdated();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }

    private void OnStarTapped(object sender, EventArgs e)
    {
        var tappedStar = (Label)sender;
        if (tappedStar == star1) selectedRating = 1;
        else if (tappedStar == star2) selectedRating = 2;
        else if (tappedStar == star3) selectedRating = 3;
        else if (tappedStar == star4) selectedRating = 4;
        else if (tappedStar == star5) selectedRating = 5;
        UpdateStars(selectedRating);
    }

    private void UpdateStars(int rating)
    {
        for (int i = 0; i < StarsLayout.Children.Count; i++)
            if (StarsLayout.Children[i] is Label star)
                star.TextColor = i < rating ? Colors.Gold : Colors.Gray;
    }
}