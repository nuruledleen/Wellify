using Microsoft.Extensions.Logging;

namespace WellifyApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("IntroRust.otf", "introrust");
                    fonts.AddFont("RotisSansSerifBold.otf", "rotisb");
                    fonts.AddFont("RotisSansExtraB.otf", "rotiseb");
                    fonts.AddFont("etna-free-font.otf", "etna");
                });

            // --- THE FIX: Use the Mapper to modify the DatePicker globally ---
            Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("MyCustomization", (handler, view) =>
            {
#if ANDROID
                if (handler.PlatformView is Android.Widget.EditText nativeEditText)
                {
                    // 1. Center the text
                    nativeEditText.Gravity = Android.Views.GravityFlags.CenterHorizontal;

                    // 2. Remove the native underline
                    nativeEditText.Background = null;
                    nativeEditText.SetBackgroundColor(Android.Graphics.Color.Transparent);
                }
#endif
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}