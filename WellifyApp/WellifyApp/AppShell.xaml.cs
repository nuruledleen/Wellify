namespace WellifyApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(DailyPage), typeof(DailyPage));

            Routing.RegisterRoute(nameof(SleepPage), typeof(SleepPage));
            Routing.RegisterRoute(nameof(MoodPage), typeof(MoodPage));
            Routing.RegisterRoute(nameof(ExercisePage), typeof(ExercisePage));
            Routing.RegisterRoute(nameof(WaterPage), typeof(WaterPage));
            Routing.RegisterRoute(nameof(MealPage), typeof(MealPage));
        }
    }
}
