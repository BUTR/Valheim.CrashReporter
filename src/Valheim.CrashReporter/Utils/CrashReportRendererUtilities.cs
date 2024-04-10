using BUTR.CrashReport.Models;
using BUTR.CrashReport.Renderer.ImGui;

using System.Collections.Generic;
using System.IO;

namespace Valheim.CrashReporter.Utils;

public class CrashReportRendererUtilities : ICrashReportRendererUtilities
{
    public IEnumerable<string> GetNativeLibrariesFolderPath()
    {
        yield return Path.GetDirectoryName(typeof(CrashReportRendererUtilities).Assembly.Location)!;
    }

    public void Upload(CrashReportModel crashReport, ICollection<LogSource> logSources)
    {
    }

    public void CopyAsHtml(CrashReportModel crashReport, ICollection<LogSource> logSources)
    {
    }

    public void SaveCrashReportAsHtml(CrashReportModel crashReport, ICollection<LogSource> logSources, bool addMiniDump, bool addLatestSave, bool addScreenshots)
    {
    }

    public void SaveCrashReportAsZip(CrashReportModel crashReport, ICollection<LogSource> logSources)
    {
    }
}