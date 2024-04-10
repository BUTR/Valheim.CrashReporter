using BUTR.CrashReport.Interfaces;
using BUTR.CrashReport.Models;

using System.Collections.Generic;

namespace Valheim.CrashReporter.Utils;

public class StacktraceFilter : IStacktraceFilter
{
    public IEnumerable<StacktraceEntry> Filter(ICollection<StacktraceEntry> stacktraceEntries) => stacktraceEntries;
}