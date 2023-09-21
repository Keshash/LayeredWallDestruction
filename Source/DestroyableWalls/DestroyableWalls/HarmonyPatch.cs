using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;

namespace LayeredDestruction
{
    [HarmonyPatch(typeof(StaticConstructorOnStartupUtility), "CallAll")]
    class StaticConstructorOnStartupUtilityPatch
    {
        public static void Postfix()
        {
            try
            {
                RuntimeHelpers.RunClassConstructor(typeof(RestoreWallUtility).TypeHandle);
            }
            catch (Exception ex)
            {
                Log.Error(string.Concat("Error in static constructor of ", typeof(RestoreWallUtility), ": ", ex));
            }
        }

    }

    [HarmonyPatch(typeof(ReverseDesignatorDatabase), "InitDesignators")]
    public class Patch
    {
        public static void Postfix(ref ReverseDesignatorDatabase __instance)
        {
            __instance.AllDesignators.Add(new Designator_RestoreWall());
        }
    }

    public class LayeredDestructionMod : Mod
    {
        public LayeredDestructionMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("LayeredDestruction.patch");
            harmony.PatchAll();
        }
    }
}
