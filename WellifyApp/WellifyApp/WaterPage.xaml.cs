using System.Globalization;
namespace WellifyApp;

public partial class WaterPage : ContentPage
{
    FirebaseHelper firebaseHelper = new FirebaseHelper();
    public WaterPage() => InitializeComponent();

    private async void OnBackTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        DateTime targetDate = AppData.SelectedDate ?? DateTime.UtcNow.AddHours(8);
        DateLabel.Text = targetDate.ToString("dddd, d MMM yyyy", new CultureInfo("en-MY"));

        var record = await firebaseHelper.GetRecord(targetDate.ToString("dd-MM-yyyy"));
        if (record != null) WaterEntry.Text = record.WaterValue;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(WaterEntry.Text))
        {
            await DisplayAlert("Hydration", "Please enter Liters.", "OK");
            return;
        }

        DateTime targetDate = AppData.SelectedDate ?? DateTime.UtcNow.AddHours(8);
        string dateKey = targetDate.ToString("dd-MM-yyyy");

        var record = await firebaseHelper.GetRecord(dateKey) ?? new WellnessRecord { DateKey = dateKey };
        record.WaterValue = WaterEntry.Text;
        AppData.WaterValue = WaterEntry.Text;
        record.TotalScore = AppData.CalculateScore();

        try
        {
            await firebaseHelper.SaveRecord(record);
            AppData.NotifyWaterUpdated();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }
}