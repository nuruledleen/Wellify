namespace WellifyApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();

        // Run cleanup in background
        RunCleanup();
    }

    private async void RunCleanup()
    {
        try
        {
            var firebase = new FirebaseHelper();
            await firebase.CleanupOldData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Startup Cleanup Error: {ex.Message}");
        }
    }
}