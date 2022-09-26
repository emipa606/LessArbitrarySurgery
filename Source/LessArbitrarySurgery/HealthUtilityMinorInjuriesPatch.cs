using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace LessArbitrarySurgery;

[HarmonyPatch(typeof(HealthUtility), "GiveInjuriesOperationFailureMinor")]
public static class HealthUtilityMinorInjuriesPatch
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> FailureMinorInjuries(IEnumerable<CodeInstruction> instrs,
        ILGenerator gen)
    {
        foreach (var itr in instrs)
        {
            if (itr.opcode == OpCodes.Ldc_I4_S && int.TryParse("" + itr.operand, out var intOperand) &&
                intOperand == 20)
            {
                yield return new CodeInstruction(OpCodes.Ldc_I4_S, 10);
                yield return new CodeInstruction(OpCodes.Ldc_I4_S, 20);
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(Rand), "RangeInclusive", new[] { typeof(int), typeof(int) }));
            }
            else
            {
                yield return itr;
            }
        }
    }
}