using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace LessArbitrarySurgery.Harmony
{
    [HarmonyPatch(typeof(HealthUtility), "GiveRandomSurgeryInjuries")]
    public static class HealthUtilitySurgeryPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ReplaceDefaultSurgeryConsequences(
            IEnumerable<CodeInstruction> instrs, ILGenerator gen)
        {
            var skip_two = 0;
            foreach (var itr in instrs)
            {
                if (skip_two == 0)
                {
                    if (itr.opcode == OpCodes.Ldc_R4 && (float) itr.operand == 0.5f)
                    {
                        skip_two += 1;
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 0.1f);
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 0.25f);
                    }
                    else
                    {
                        yield return itr;
                    }
                }
                else
                {
                    if (skip_two < 2)
                    {
                        skip_two += 1;
                    }
                    else
                    {
                        yield return itr;
                    }
                }
            }
        }
    }
}