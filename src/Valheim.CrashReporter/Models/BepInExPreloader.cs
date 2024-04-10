using BepInEx;

using BUTR.CrashReport.Models;

namespace Valheim.CrashReporter.Models;

public class BepInExPreloader : ILoaderPluginInfo
{
    public string Id => "BepInEx.Preloader";

    public string? Version => typeof(PluginInfo).Assembly.GetName().Version?.ToString(4);

    public string? UpdateInfo => null;
}