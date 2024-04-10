using BepInEx;
using BepInEx.Bootstrap;

using BUTR.CrashReport;
using BUTR.CrashReport.Extensions;
using BUTR.CrashReport.Interfaces;
using BUTR.CrashReport.Models;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Valheim.CrashReporter.Models;

namespace Valheim.CrashReporter.Utils;

public class CrashReportUtilities :
    ICrashReportMetadataProvider,
    IAssemblyUtilities,
    IModuleProvider, ILoaderPluginProvider,
    IModelConverter,
    IPathAnonymizer
{
    // Inspired by SMAPI's detection
    // Still Work In Progress for more complex capabilities
    private static readonly string[] OSFileSystemTypeReferences =
    [
        typeof(System.IO.File).FullName!,
        typeof(System.IO.FileStream).FullName!,
        typeof(System.IO.FileInfo).FullName!,
        typeof(System.IO.Directory).FullName!,
        typeof(System.IO.DirectoryInfo).FullName!,
        typeof(System.IO.DriveInfo).FullName!,
        typeof(System.IO.FileSystemWatcher).FullName!
    ];
    private static readonly string[] ShellTypeReferences =
    [
        typeof(System.Diagnostics.Process).FullName!
    ];
    private static readonly string[] HttpTypeReferences =
    [
        "System.Net*Http.*"
    ];

    public static IEnumerable<CapabilityModuleOrPluginModel> GetLoaderPluginCapabilities(ICollection<AssemblyModel> assemblies, LoaderPluginModel module)
    {
        if (module.ContainsTypeReferences(assemblies, OSFileSystemTypeReferences))
            yield return new CapabilityModuleOrPluginModel("OS File System");

        if (module.ContainsTypeReferences(assemblies, ShellTypeReferences))
            yield return new CapabilityModuleOrPluginModel("Shell");

        if (module.ContainsTypeReferences(assemblies, HttpTypeReferences))
            yield return new CapabilityModuleOrPluginModel("Http");
    }


    public ICollection<IModuleInfo> GetLoadedModules() => new List<IModuleInfo>();
    public IModuleInfo? GetModuleByType(Type? type) => null;

    public IEnumerable<Assembly> Assemblies() => AccessTools.AllAssemblies();

    public IModuleInfo? GetAssemblyModule(CrashReportInfo crashReport, Assembly assembly) => null;

    public ILoaderPluginInfo? GetAssemblyPlugin(CrashReportInfo crashReport, Assembly assembly)
    {
        if (assembly.IsDynamic)
            return null;

        if (assembly.GetName() is { Name: "BepInEx.Preloader" })
            return new BepInExPreloader();

        var directory = Path.GetDirectoryName(assembly.Location);

        foreach (var kv in Chainloader.PluginInfos)
        {
            var pluginInfo = kv.Value;

            var pluginDirectory = Path.GetDirectoryName(pluginInfo.Location);

            if (assembly.Location == pluginInfo.Location || directory == pluginDirectory)
                return new BepInExPlugin(pluginInfo);
        }

        return null;
    }

    public AssemblyModelType GetAssemblyType(AssemblyModelType type, CrashReportInfo crashReport, Assembly assembly)
    {
        var beinExLocation = Path.GetDirectoryName(typeof(PluginInfo).Assembly.Location);
        if (!assembly.IsDynamic && Path.GetDirectoryName(assembly.Location) == beinExLocation)
            return AssemblyModelType.Loader;

        return type & AssemblyModelType.System;
    }

    public ICollection<ILoaderPluginInfo> GetLoadedLoaderPlugins() => Chainloader.PluginInfos.Values
        .Select(x => (ILoaderPluginInfo) new BepInExPlugin(x))
        .Concat(new ILoaderPluginInfo[] { new BepInExPreloader() })
        .ToList();

    public ILoaderPluginInfo? GetLoaderPluginByType(Type? type)
    {
        if (type is null)
            return null;
        if (type.Assembly.IsDynamic)
            return null;

        if (type.Assembly.GetName() is { Name: "BepInEx.Preloader" })
            return new BepInExPreloader();

        var directory = Path.GetDirectoryName(type.Assembly.Location);

        foreach (var kv in Chainloader.PluginInfos)
        {
            var pluginInfo = kv.Value;

            var pluginDirectory = Path.GetDirectoryName(pluginInfo.Location);

            if (type.Assembly.Location == pluginInfo.Location || directory == pluginDirectory)
                return new BepInExPlugin(pluginInfo);
        }

        return null;
    }

    public List<ModuleModel> ToModuleModels(ICollection<IModuleInfo> loadedModules, ICollection<AssemblyModel> assemblies) => [];

    public List<LoaderPluginModel> ToLoaderPluginModels(ICollection<ILoaderPluginInfo> loadedLoaderPlugins, ICollection<AssemblyModel> assemblies) => loadedLoaderPlugins.OfType<BepInExPlugin>().Select(x =>
    {
        var capabilities = new List<CapabilityModuleOrPluginModel>();
        var model = new LoaderPluginModel
        {
            Id = x.PluginInfo.Metadata.GUID,
            Name = x.PluginInfo.Metadata.Name,
            Version = x.PluginInfo.Metadata.Version.ToString(2),
            UpdateInfo = null,
            Dependencies = x.PluginInfo.Dependencies.Select(y => new DependencyMetadataModel
            {
                ModuleOrPluginId = y.DependencyGUID,
                Version = y.MinimumVersion.ToString(2),
                VersionRange = null,
                Type = DependencyMetadataModelType.LoadBefore,
                IsOptional = y.Flags.HasFlag(BepInDependency.DependencyFlags.SoftDependency),
                AdditionalMetadata = new List<MetadataModel>
                {
                    new() {Key = "IsHardDependency", Value = y.Flags.HasFlag(BepInDependency.DependencyFlags.HardDependency).ToString()},
                    new() {Key = "IsSoftDependency", Value = y.Flags.HasFlag(BepInDependency.DependencyFlags.SoftDependency).ToString()},
                },
            }).Concat(x.PluginInfo.Incompatibilities.Select(y => new DependencyMetadataModel
            {
                ModuleOrPluginId = y.IncompatibilityGUID,
                Version = null,
                VersionRange = null,
                Type = DependencyMetadataModelType.Incompatible,
                IsOptional = false,
                AdditionalMetadata = Array.Empty<MetadataModel>(),
            })).ToList(),
            Capabilities = capabilities,
            AdditionalMetadata = new[]
            {
                //new MetadataModel { Key = "TargettedBepInExVersion", Value = x.PluginInfo.TargettedBepInExVersion.ToString() },
                new MetadataModel {Key = "Location", Value = x.PluginInfo.Location},
                //new MetadataModel { Key = "TypeName", Value = x.PluginInfo.TypeName },
                new MetadataModel {Key = "Processes", Value = string.Join("; ", x.PluginInfo.Processes.Select(y => y.ProcessName))},
            },
        };
        capabilities.AddRange(GetLoaderPluginCapabilities(assemblies, model));
        return model;
    }).Concat(loadedLoaderPlugins.OfType<BepInExPreloader>().Select(x =>
    {
        var capabilities = new List<CapabilityModuleOrPluginModel>();
        var model = new LoaderPluginModel
        {
            Id = x.Id,
            Name = x.Id,
            Version = x.Version,
            UpdateInfo = null,
            Dependencies = Array.Empty<DependencyMetadataModel>(),
            Capabilities = capabilities,
            AdditionalMetadata = Array.Empty<MetadataModel>(),
        };
        capabilities.AddRange(GetLoaderPluginCapabilities(assemblies, model));
        return model;
    })).ToList();

    public bool TryHandlePath(string path, out string anonymizedPath)
    {
        anonymizedPath = path;
        return false;
    }

    public CrashReportMetadataModel GetCrashReportMetadataModel(CrashReportInfo crashReport) => new()
    {
        GameVersion = "v1.0.0",
        GameName = "Valheim",
        LoaderPluginProviderName = "BepInEx",
        LoaderPluginProviderVersion = "v1.0.1",
        LauncherType = "",
        LauncherVersion = "",
        Runtime = "",
        AdditionalMetadata = new List<MetadataModel>(),
    };
}