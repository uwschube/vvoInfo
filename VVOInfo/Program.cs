using Avalonia;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace VVOInfo;

sealed class Program
{
    private static readonly ILog log = LogManager.GetLogger(typeof(Program));

    [STAThread]
    public static void Main(string[] args)
    {
        var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        log.Info("VVOInfo App wird gestartet...");

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new X11PlatformOptions
            {
                // Definiert die erlaubten Render-Modi.
                // Durch das Weglassen von "X11RenderingMode.Software" zwingen wir die App zur GPU-Nutzung.
                RenderingMode = new[] { X11RenderingMode.Glx, X11RenderingMode.Egl }
            })
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
        /*

        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            // Ihre restliche Konfiguration...
            .LogToTrace();
        */
    }


    /*
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new X11PlatformOptions
            {
                // Definiert die erlaubten Render-Modi.
                // Durch das Weglassen von "X11RenderingMode.Software" zwingen wir die App zur GPU-Nutzung.
                RenderingMode = new[] { X11RenderingMode.Glx, X11RenderingMode.Egl }
            })
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
    */
}
/*

using Avalonia;
using System;

namespace VVOInfo;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new X11PlatformOptions
            {
                UseGpu = true,            // Aktiviert die GPU-Beschleunigung unter X11/XWayland
                EnableMultiTouch = true
            })
            .With(new AvaloniaNativePlatformOptions
            {
                UseGpu = true             // Aktiviert die GPU-Beschleunigung, falls ein natives Wayland-Backend greift
            })
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
*/

/*



using Avalonia;
using System;

namespace VVOInfo;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
*/