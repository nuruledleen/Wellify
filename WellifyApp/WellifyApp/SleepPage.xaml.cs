using System.Globalization;
namespace WellifyApp;

public partial class SleepPage : ContentPage
{
    FirebaseHelper firebaseHelper = new FirebaseHelper();
    public SleepPage() => InitializeComponent();

    private async void OnBackTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        DateTime targetDate = AppData.SelectedDate ?? DateTime.UtcNow.AddHours(8);
        DateLabel.Text = targetDate.ToString("dddd, d MMM yyyy", new CultureInfo("en-MY"));

        // Fetch existing data for this specific day
        var record = await firebaseHelper.GetRecord(targetDate.ToString("dd-MM-yyyy"));
        if (record != null) SleepEntry.Text = record.SleepHours;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SleepEntry.Text))
        {
            await DisplayAlert("Empty Input", "Please enter hours.", "OK");
            return;
        }

        DateTime targetDate = AppData.SelectedDate ?? DateTime.UtcNow.AddHours(8);
        string dateKey = targetDate.ToString("dd-MM-yyyy");

        var record = await firebaseHelper.GetRecord(dateKey) ?? new WellnessRecord { DateKey = dateKey };
        record.SleepHours = SleepEntry.Text;
        AppData.SleepHours = SleepEntry.Text; // Update local for score calculation
        record.TotalScore = AppData.CalculateScore();

        try
        {
            await firebaseHelper.SaveRecord(record);
            AppData.NotifySleepUpdated();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }
}