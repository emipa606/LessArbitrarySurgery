using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace LessArbitrarySurgery;

[HarmonyPatch(typeof(HealthUtility), "GiveRandomSurgeryInjuries")]
public static class HealthUtilitySurgeryPatch
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ReplaceDefaultSurgeryConsequences(
        IEnumerable<CodeInstruction> instrs, ILGenerator gen)
    {
        var skipTwo = 0;
        foreach (var itr in instrs)
        {
            switch (skipTwo)
            {
                case 0 when itr.opcode == OpCodes.Ldc_R4 && (float)itr.operand == 0.5f:
                    skipTwo += 1;
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0.1f);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0.25f);
                    break;
                case 0:
                    yield return itr;
                    break;
                case < 2:
                    skipTwo += 1;
                    break;
                default:
                    yield return itr;
                    break;
            }
        }
    }
}