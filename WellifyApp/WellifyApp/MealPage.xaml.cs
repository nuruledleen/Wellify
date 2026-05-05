using System.Globalization;
namespace WellifyApp;

public partial class MealPage : ContentPage
{
    FirebaseHelper firebaseHelper = new FirebaseHelper();
    string selectedMeal = "";
    public MealPage() => InitializeComponent();

    private async void OnBackTapped(object sender, EventArgs e) => await Shell.Current.GoToAsync("..");

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        DateTime targetDate = AppData.SelectedDate ?? DateTime.UtcNow.AddHours(8);
        DateLabel.Text = targetDate.ToString("dddd, d MMM yyyy", new CultureInfo("en-MY"));

        var record = await firebaseHelper.GetRecord(targetDate.ToString("dd-MM-yyyy"));
        if (record != null && !string.IsNullOrEmpty(record.MealValue))
        {
            selectedMeal = record.MealValue;
            ResetCards();
            if (selectedMeal == "Mostly Healthy") HighlightCard(HealthyCard);
            else if (selectedMeal == "Mixed/Average") HighlightCard(AverageCard);
            else if (selectedMeal == "Mostly Unhealthy") HighlightCard(UnhealthyCard);
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(selectedMeal))
        {
            await DisplayAlert("Missing Input", "Please select a meal.", "OK");
            return;
        }

        DateTime targetDate = AppData.SelectedDate ?? DateTime.UtcNow.AddHours(8);
        string dateKey = targetDate.ToString("dd-MM-yyyy");

        var record = await firebaseHelper.GetRecord(dateKey) ?? new WellnessRecord { DateKey = dateKey };
        record.MealValue = selectedMeal;
        AppData.MealValue = selectedMeal;
        record.TotalScore = AppData.CalculateScore();

        try
        {
            await firebaseHelper.SaveRecord(record);
            AppData.NotifyMealUpdated();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }

    private void OnMealCardTapped(object sender, EventArgs e)
    {
        ResetCards();
        if (sender is Border tappedCard && tappedCard.Content is Label label)
        {
            HighlightCard(tappedCard);
            selectedMeal = label.Text.Replace(" Meals", "");
        }
    }

    private void HighlightCard(Border card)
    {
        card.Stroke = Color.FromArgb("#af006b");
        card.BackgroundColor = Color.FromArgb("#f7e8f1");
        if (card.Content is Label label) label.FontAttributes = FontAttributes.Bold;
    }

    private void ResetCards() { ResetCard(HealthyCard); ResetCard(AverageCard); ResetCard(UnhealthyCard); }
    private void ResetCard(Border card)
    {
        card.Stroke = Colors.Gray;
        card.BackgroundColor = Colors.Transparent;
        if (card.Content is Label label) label.FontAttributes = FontAttributes.None;
    }
}