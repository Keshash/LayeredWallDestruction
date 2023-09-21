using System;
using Verse;
using RimWorld;

namespace LayeredDestruction
{
    [DefOf]
    public static class RestoreDesignationDefOf
    {
        static RestoreDesignationDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DesignationDefOf));
        }

        public static DesignationDef RestoreWall;
    }

    [DefOf]
    public static class LayeredDestructionDefOfs
    {
        public static ThingDef Filth_LWD_Planks, Filth_LWD_Bricks, Filth_LWD_Smooth;

        public static JobDef RestoreWall;

    }

    //public class WallRestoreExtension : DefModExtension
    //{
    //    public ThingDef restoreToDef;
    //}
}
