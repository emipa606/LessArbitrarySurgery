using System;
using System.Collections.Generic;
using Verse;
using HarmonyLib;
using System.Reflection.Emit;

namespace LessArbitrarySurgery.Harmony
{
    [HarmonyPatch(typeof(HealthUtility), "GiveRandomSurgeryInjuries")]
    public static class HealthUtilitySurgeryPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ReplaceDefaultSurgeryConsequences(IEnumerable<CodeInstruction> instrs, ILGenerator gen)
        {
            int skip_two = 0;
            foreach (CodeInstruction itr in instrs)
            {
                if (skip_two == 0)
                {
                    if (itr.opcode == OpCodes.Ldc_R4 && (float)itr.operand == 0.5f)
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

    [HarmonyPatch(typeof(HealthUtility), "GiveInjuriesOperationFailureMinor")]
    public static class HealthUtilityMinorInjuriesPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FailureMinorInjuries(IEnumerable<CodeInstruction> instrs, ILGenerator gen)
        {
            int intOperand;
            foreach (CodeInstruction itr in instrs)
            {
                if(itr.opcode == OpCodes.Ldc_I4_S && int.TryParse("" + itr.operand, out intOperand) && intOperand == 20)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, 10);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, 20);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Rand), "RangeInclusive", new Type[] { typeof(int), typeof(int) }));
                }
                else
                {
                    yield return itr;
                }
            }
        }
    }

    [HarmonyPatch(typeof(HealthUtility), "GiveInjuriesOperationFailureCatastrophic")]
    public static class HealthUtilityCatastrophicInjuriesPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FailureCatastrophicInjuries(IEnumerable<CodeInstruction> instrs, ILGenerator gen)
        {
            int intOperand;
            foreach (CodeInstruction itr in instrs)
            {
                if (itr.opcode == OpCodes.Ldc_I4_S && int.TryParse("" + itr.operand, out intOperand) && intOperand == 65)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, 30);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, 60);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Rand), "RangeInclusive", new Type[] { typeof(int), typeof(int) }));
                }
                else
                {
                    yield return itr;
                }
            }
        }
    }
}
