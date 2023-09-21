using System;
using Verse;
using RimWorld;
using static UnityEngine.GraphicsBuffer;

namespace LayeredDestruction
{
    public static class RestoreWallUtility
    {

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
            return edifice != null;
        }

    }
}
