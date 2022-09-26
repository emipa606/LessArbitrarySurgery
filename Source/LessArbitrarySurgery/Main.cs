using System.Reflection;
using Verse;

namespace LessArbitrarySurgery;

[StaticConstructorOnStartup]
public static class Main
{
    static Main()
    {
        new HarmonyLib.Harmony("Mlie.LessArbitrarySurgery").PatchAll(Assembly.GetExecutingAssembly());
    }
}