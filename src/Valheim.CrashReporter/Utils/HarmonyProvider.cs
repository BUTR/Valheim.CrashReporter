using BUTR.CrashReport.Interfaces;
using BUTR.CrashReport.Models;

using HarmonyLib;

using MonoMod.RuntimeDetour;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Valheim.CrashReporter.Utils;

public sealed class HarmonyProvider : IHarmonyProvider
{
    public IEnumerable<MethodBase> GetAllPatchedMethods() => Harmony.GetAllPatchedMethods();

    public HarmonyPatches GetPatchInfo(MethodBase originalMethod)
    {
        static global::BUTR.CrashReport.Models.HarmonyPatch Convert(Patch patch, BUTR.CrashReport.Models.HarmonyPatchType type) => new()
        {
            Owner = patch.owner,
            Index = patch.index,
            Priority = patch.priority,
            Before = patch.before,
            After = patch.after,
            PatchMethod = patch.PatchMethod,
            Type = type,
        };

        var patches = Harmony.GetPatchInfo(originalMethod);
        return new()
        {
            Prefixes = patches.Prefixes.Select(x => Convert(x, BUTR.CrashReport.Models.HarmonyPatchType.Prefix)).ToArray(),
            Postfixes = patches.Postfixes.Select(x => Convert(x, BUTR.CrashReport.Models.HarmonyPatchType.Postfix)).ToArray(),
            Finalizers = patches.Finalizers.Select(x => Convert(x, BUTR.CrashReport.Models.HarmonyPatchType.Finalizer)).ToArray(),
            Transpilers = patches.Transpilers.Select(x => Convert(x, BUTR.CrashReport.Models.HarmonyPatchType.Transpiler)).ToArray(),
        };
    }

    public MethodBase GetOriginalMethod(MethodInfo replacement) => Harmony.GetOriginalMethod(replacement);

    public MethodBase GetMethodFromStackframe(StackFrame frame) => Harmony.GetMethodFromStackframe(frame);

    public MethodBase? GetIdentifiable(MethodBase method) => DetourHelper.Runtime.GetIdentifiable(method);

    public IntPtr GetNativeMethodBody(MethodBase method) => DetourHelper.Runtime.GetNativeStart(method);
}