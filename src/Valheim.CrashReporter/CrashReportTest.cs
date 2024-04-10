using BepInEx;

using BUTR.CrashReport;
using BUTR.CrashReport.Models;
using BUTR.CrashReport.Renderer.ImGui;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Valheim.CrashReporter.Utils;

namespace Valheim.CrashReporter;

[BepInPlugin(GUID, PluginName, Version)]
public class CrashReportTest : BaseUnityPlugin
{
    public const string GUID = "CrashReportTest";
    public const string PluginName = "Catch Unity Event Exceptions";
    public const string Version = "1.0";

    private bool once = false;
    private void Update()
    {
        if (once) return;
        once = true;

        try
        {
            throw new Exception("Test");
        }
        catch (Exception exception)
        {
            var crch = new CrashReportUtilities();
            var cri = CrashReportInfo.Create(exception, new Dictionary<string, string>(), new StacktraceFilter(), crch, crch, crch, new HarmonyProvider());
            var crm = CrashReportInfo.ToModel(cri, crch, crch, crch, crch, crch, crch);
            try
            {
                CrashReportImGui.ShowAndWait(crm, Array.Empty<LogSource>(), new CrashReportRendererUtilities());
            }
            catch (Exception imGuiException)
            {
                Trace.TraceError(imGuiException.ToString());
                throw;
            }
        }
    }
}