using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace LessArbitrarySurgery;

[HarmonyPatch(typeof(Recipe_Surgery), "CheckSurgeryFail")]
public static class Recipe_SurgeryFailPatch
{
    private static readonly SimpleCurve MedicineMedicalPotencyToSurgeryChanceFactor = new SimpleCurve
    {
        new CurvePoint(0f, 0.7f),
        new CurvePoint(1f, 1f),
        new CurvePoint(2f, 1.3f)
    };

    private static float GetAverageMedicalPotency(List<Thing> ingredients, Bill bill)
    {
        ThingDef thingDef;
        if (bill is Bill_Medical bill_Medical)
        {
            thingDef = bill_Medical.consumedInitialMedicineDef;
        }
        else
        {
            thingDef = null;
        }

        var num = 0;
        var num2 = 0f;
        if (thingDef != null)
        {
            num++;
            num2 += thingDef.GetStatValueAbstract(StatDefOf.MedicalPotency);
        }

        foreach (var thing in ingredients)
        {
            if (thing is not Medicine medicine)
            {
                continue;
            }

            num += medicine.stackCount;
            num2 += medicine.GetStatValue(StatDefOf.MedicalPotency) * medicine.stackCount;
        }

        if (num == 0)
        {
            return 1f;
        }

        return num2 / num;
    }

    [HarmonyPrefix]
    public static bool CheckSurgeryFail(Recipe_Surgery __instance, ref bool __result, Pawn surgeon, Pawn patient,
        List<Thing> ingredients, BodyPartRecord part, Bill bill)
    {
        if (__instance.recipe.surgeryOutcomeEffect == null)
        {
            return false;
        }

        var num = 1f;
        if (!patient.RaceProps.IsMechanoid)
        {
            num *= surgeon.GetStatValue(StatDefOf.MedicalSurgerySuccessChance);
        }

        if (patient.InBed())
        {
            num *= patient.CurrentBed().GetStatValue(StatDefOf.SurgerySuccessChanceFactor);
        }

        num *= MedicineMedicalPotencyToSurgeryChanceFactor.Evaluate(GetAverageMedicalPotency(ingredients, bill));

        num *= __instance.recipe.surgerySuccessChanceFactor;
        if (surgeon.InspirationDef == InspirationDefOf.Inspired_Surgery && !patient.RaceProps.IsMechanoid)
        {
            num *= 2f;
            surgeon.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Surgery);
        }

        num = Mathf.Min(num, 0.98f);

        if (!Rand.Chance(num)) // Failed check
        {
            if (Rand.Chance(num)) // Successful check
            {
                //One more chance to get it right with minor injuries
                if (!Rand.Chance(Mathf.InverseLerp(0, 20, surgeon.skills.GetSkill(SkillDefOf.Medicine).Level) -
                                 (1f - surgeon.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness))))
                {
                    // Failed check
                    __instance.GiveInfection(patient, part, ingredients);
                }

                GiveNonLethalSurgeryInjuries(patient, part);
                //if (!patient.health.hediffSet.PartIsMissing(part)) //This was breaking some crap
                //{
                __result = false;
                return false;
                //}
            }

            if (!Rand.Chance(Mathf.InverseLerp(0, 20, surgeon.skills.GetSkill(SkillDefOf.Medicine).Level) -
                             (1f - surgeon.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness))))
            {
                // Failed check
                //The surgeon has a chance based on their skill to avoid potentially lethal failures.
                if (__instance.recipe.deathOnFailedSurgeryChance > 0 &&
                    Rand.Chance(__instance.recipe.deathOnFailedSurgeryChance - num))
                {
                    //Failed surgery death chance is influenced by the surgery success chance.

                    HealthUtility.GiveRandomSurgeryInjuries(patient, 65, part);
                    if (!patient.Dead)
                    {
                        patient.Kill(null);
                    }

                    Messages.Message(
                        "MessageMedicalOperationFailureFatal".Translate(surgeon.LabelShort, patient.LabelShort,
                            __instance.recipe.LabelCap, surgeon.Named("SURGEON"), patient.Named("PATIENT")),
                        patient, MessageTypeDefOf.NegativeHealthEvent);
                }
                else if (!Rand.Chance(num)) // Failed check
                {
                    //Instead of a 50-50 chance, it's based on how likely you were to successfully complete the surgery.
                    if (!Rand.Chance(surgeon.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness)))
                    {
                        //Only impaired surgeons will ever perform ridiculous failures.
                        Messages.Message(
                            "MessageMedicalOperationFailureRidiculous".Translate(surgeon.LabelShort,
                                patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT")), patient,
                            MessageTypeDefOf.NegativeHealthEvent);
                        HealthUtility.GiveRandomSurgeryInjuries(patient, 65, null);
                    }
                    else
                    {
                        Messages.Message(
                            "MessageMedicalOperationFailureCatastrophic".Translate(surgeon.LabelShort,
                                patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT")), patient,
                            MessageTypeDefOf.NegativeHealthEvent);
                        HealthUtility.GiveRandomSurgeryInjuries(patient, 65, part);
                    }
                }
                else
                {
                    Messages.Message(
                        "MessageMedicalOperationFailureMinor".Translate(surgeon.LabelShort, patient.LabelShort,
                            surgeon.Named("SURGEON"), patient.Named("PATIENT")), patient,
                        MessageTypeDefOf.NegativeHealthEvent);
                    RecreateIngredient(surgeon, ingredients);
                }

                __instance.GiveInfection(patient, part, ingredients);
            }
            else
            {
                //Non-lethal surgery injuries.
                Messages.Message(
                    "MessageMedicalOperationFailureMinor".Translate(surgeon.LabelShort, patient.LabelShort,
                        surgeon.Named("SURGEON"), patient.Named("PATIENT")), patient,
                    MessageTypeDefOf.NegativeHealthEvent);
                GiveNonLethalSurgeryInjuries(patient, part);
                __instance.GiveInfection(patient, part, ingredients);
                RecreateIngredient(surgeon, ingredients);
            }

            if (!patient.Dead)
            {
                if (patient.RaceProps.Humanlike && patient.needs.mood != null)
                {
                    patient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BotchedMySurgery, surgeon);
                }
            }

            __result = true;
            return false;
        }

        __result = false;
        return false;
    }

    public static void GiveNonLethalSurgeryInjuries(Pawn patient, BodyPartRecord part)
    {
        var num3 = Math.Min(Rand.RangeInclusive(10, 20),
            Mathf.RoundToInt(patient.health.hediffSet.GetPartHealth(part) - 3));
        HealthUtility.GiveRandomSurgeryInjuries(patient, num3, part);
    }

    public static void RecreateIngredient(Pawn surgeon, List<Thing> ingredients)
    {
        if (ingredients.Count <= 0)
        {
            return;
        }

        var ingredient = ingredients.Find(x => x.def.isTechHediff);
        var newIngredient = GenSpawn.Spawn(ingredient.def, surgeon.Position, surgeon.Map);
        newIngredient.HitPoints = ingredient.HitPoints -
                                  (int)(ingredient.MaxHitPoints * Mathf.Clamp(.1f, .5f, Rand.Value / 2));
        if (newIngredient.HitPoints <= 0)
        {
            newIngredient.Destroy();
        }
    }

    public static void GiveInfection(this Recipe_Surgery s, Pawn patient, BodyPartRecord part,
        List<Thing> ingredients)
    {
        Hediff infection = null;
        if (patient.health.immunity.DiseaseContractChanceFactor(HediffDefOf.WoundInfection, part) < 0.0001f)
        {
            return;
        }

        if (ingredients.Count == 0 && part.parent != null || part.parent != null)
        {
            infection = HediffMaker.MakeHediff(HediffDefOf.WoundInfection, patient, part.parent);
        }
        else if (s is Recipe_RemoveBodyPart)
        {
            infection = HediffMaker.MakeHediff(HediffDefOf.WoundInfection, patient, part);
        }

        if (infection != null)
        {
            patient.health.AddHediff(infection);
        }
    }
}