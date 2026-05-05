using System.Globalization;
namespace WellifyApp;

public partial class ExercisePage : ContentPage
{
    FirebaseHelper firebaseHelper = new FirebaseHelper();
    public ExercisePage() => InitializeComponent();

    private async void OnBackTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        DateTime targetDate = AppData.SelectedDate ?? DateTime.UtcNow.AddHours(8);
        DateLabel.Text = targetDate.ToString("dddd, d MMM yyyy", new CultureInfo("en-MY"));

        var record = await firebaseHelper.GetRecord(targetDate.ToString("dd-MM-yyyy"));
        if (record != null) ExerciseEntry.Text = record.ExerciseValue;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ExerciseEntry.Text))
        {
            await DisplayAlert("Missing Information", "Please enter minutes.", "OK");
            return;
        }

        DateTime targetDate = AppData.SelectedDate ?? DateTime.UtcNow.AddHours(8);
        string dateKey = targetDate.ToString("dd-MM-yyyy");

        var record = await firebaseHelper.GetRecord(dateKey) ?? new WellnessRecord { DateKey = dateKey };
        record.ExerciseValue = ExerciseEntry.Text;
        AppData.ExerciseValue = ExerciseEntry.Text;
        record.TotalScore = AppData.CalculateScore();

        try
        {
            await firebaseHelper.SaveRecord(record);
            AppData.NotifyExerciseUpdated();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }
}