using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace LessArbitrarySurgery.Harmony
{
    [HarmonyPatch(typeof(HealthUtility), "GiveInjuriesOperationFailureCatastrophic")]
    public static class HealthUtilityCatastrophicInjuriesPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FailureCatastrophicInjuries(IEnumerable<CodeInstruction> instrs,
            ILGenerator gen)
        {
            foreach (var itr in instrs)
            {
                if (itr.opcode == OpCodes.Ldc_I4_S && int.TryParse("" + itr.operand, out var intOperand) &&
                    intOperand == 65)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, 30);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, 60);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(Rand), "RangeInclusive", new[] {typeof(int), typeof(int)}));
                }
                else
                {
                    yield return itr;
                }
            }
        }
    }
}