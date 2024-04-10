using BepInEx;

using BUTR.CrashReport.Models;

namespace Valheim.CrashReporter.Models;

public class BepInExPlugin : ILoaderPluginInfo
{
    public string Id => PluginInfo.Metadata.GUID;
    public string Version => PluginInfo.Metadata.Version.ToString(2);
    public string UpdateInfo => string.Empty;

    internal PluginInfo PluginInfo { get; }

    public BepInExPlugin(PluginInfo pluginInfo) => PluginInfo = pluginInfo;
}