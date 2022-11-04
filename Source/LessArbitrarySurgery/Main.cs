using System.Reflection;
using HarmonyLib;
using Verse;

namespace LessArbitrarySurgery;

[StaticConstructorOnStartup]
public static class Main
{
    static Main()
    {
        new Harmony("Mlie.LessArbitrarySurgery").PatchAll(Assembly.GetExecutingAssembly());
    }
}