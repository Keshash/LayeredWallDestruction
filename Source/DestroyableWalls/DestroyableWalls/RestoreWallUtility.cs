using System;
using Verse;
using RimWorld;
using static UnityEngine.GraphicsBuffer;

namespace LayeredDestruction
{
    public static class RestoreWallUtility
    {

        //public static void Notify_SmoothedByPawn(Thing t, Pawn p)
        //{
        //    for (int i = 0; i < GenAdj.CardinalDirections.Length; i++)
        //    {
        //        IntVec3 c = t.Position + GenAdj.CardinalDirections[i];
        //        Log.Message("Got directions");
        //        if (c.InBounds(t.Map))
        //        {
        //            Log.Message("Checking cell" + c.ToString() + " on map " + t.Map.ToString() + " for thing " + t.ToString());

        //            Building edifice = null;

        //            edifice = c.GetEdifice(t.Map) != null ? c.GetEdifice(t.Map) : null;

        //            Log.Message("Edifice: " + edifice.ToString());
        //            var modExt = edifice.def.GetModExtension<WallRestoreExtension>() ?? new WallRestoreExtension();
        //            Log.Message("Mod ext");
        //            if (edifice != null && modExt.restoreToDef != null)
        //            {
        //                bool flag = true;
        //                int num = 0;
        //                for (int j = 0; j < GenAdj.CardinalDirections.Length; j++)
        //                {
        //                    IntVec3 intVec = edifice.Position + GenAdj.CardinalDirections[j];
        //                    Log.Message("checking pos blocked");
        //                    if (!RestoreWallUtility.IsBlocked(intVec, t.Map))
        //                    {
        //                        Log.Message("blocked");
        //                        flag = false;
        //                        break;
        //                    }

        //                    Building edifice2 = intVec.GetEdifice(t.Map);
        //                    if (edifice2 != null && edifice2.def.IsSmoothed)
        //                    {
        //                        Log.Message("Could break here1?");
        //                        num++;
        //                    }
        //                }
        //                if (flag && num >= 2)
        //                {
        //                    for (int k = 0; k < GenAdj.DiagonalDirections.Length; k++)
        //                    {
        //                        if (!RestoreWallUtility.IsBlocked(edifice.Position + GenAdj.DiagonalDirections[k], t.Map))
        //                        {
        //                            Log.Message("success");
        //                            RestoreWallUtility.RestoreWall(edifice, p);
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        public static void Notify_BuildingDestroying(Thing t, DestroyMode mode)
        {
            if (mode != DestroyMode.KillFinalize && mode != DestroyMode.Deconstruct)
            {
                return;
            }
            if (!t.def.IsSmoothed)
            {
                return;
            }
            for (int i = 0; i < GenAdj.CardinalDirections.Length; i++)
            {
                IntVec3 c = t.Position + GenAdj.CardinalDirections[i];
                if (c.InBounds(t.Map))
                {
                    Building edifice = c.GetEdifice(t.Map);
                    if (edifice != null && edifice.def.IsSmoothed)
                    {
                        bool flag = true;
                        for (int j = 0; j < GenAdj.CardinalDirections.Length; j++)
                        {
                            if (!RestoreWallUtility.IsBlocked(edifice.Position + GenAdj.CardinalDirections[j], t.Map))
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            edifice.Destroy(DestroyMode.WillReplace);
                            GenSpawn.Spawn(ThingMaker.MakeThing(edifice.def.building.unsmoothedThing, edifice.Stuff), edifice.Position, t.Map, edifice.Rotation, WipeMode.Vanish, false);
                        }
                    }
                }
            }
        }

        public static Thing RestoreWall(Thing target, Pawn smoother)
        {
            var props = target.def.GetCompProperties<CompProperties_LayeredDestruction>() ?? new CompProperties_LayeredDestruction();
            if (props.ParentLayerDef != null)
            {
                Map map = target.Map;
                target.Destroy(DestroyMode.WillReplace);
                Thing thing;
                if (target.Stuff != null)
                {
                    thing = ThingMaker.MakeThing(props.ParentLayerDef, target.Stuff);
                }
                else
                {
                    thing = ThingMaker.MakeThing(props.ParentLayerDef);
                }
                thing.SetFaction(Faction.OfPlayer, null);
                thing.HitPoints = thing.MaxHitPoints / 10;
                if (target.HasComp<CompColorable>())
                {
                    thing.SetColor(target.DrawColor);
                }
                else
                {
                    Building building = thing as Building;
                    building.ChangePaint((target as Building).PaintColorDef);
                }
                GenSpawn.Spawn(thing, target.Position, map, target.Rotation, WipeMode.Vanish, false);
                map.designationManager.TryRemoveDesignationOn(target, RestoreDesignationDefOf.RestoreWall);
                return thing;
            }
            Log.Error("Failed to restore" + target.ToString());
            return target;
        }

        private static bool IsBlocked(IntVec3 pos, Map map)
        {
            if (!pos.InBounds(map))
            {
                return false;
            }
            if (pos.Walkable(map))
            {
                return false;
            }
            Building edifice = pos.GetEdifice(map);
            return edifice != null; //&& (edifice.def.IsSmoothed || edifice.def.building.isNaturalRock);
        }

    }
}
