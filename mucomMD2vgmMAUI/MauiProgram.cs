using Microsoft.Extensions.Logging;

namespace mucomMD2vgmMAUI
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
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.ConfigureMauiHandlers(handlers =>
            {
                Microsoft.Maui.Handlers.ImageHandler.Mapper.AppendToMapping("NoDpi", (handler, view) =>
                {
#if ANDROID
                    handler.PlatformView.SetAdjustViewBounds(true);
#endif
                });
            });
            
            return builder.Build();
        }
    }
}
