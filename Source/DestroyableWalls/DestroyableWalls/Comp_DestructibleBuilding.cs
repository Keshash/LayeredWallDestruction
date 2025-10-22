using RimWorld;
using UnityEngine;
using Verse;

namespace LayeredDestruction
{
    public class CompProperties_LayeredDestruction : CompProperties
    {
        public ThingDef NextLayerDef;
        public ThingDef NextLayerDef_Alternative;
        public ThingDef ParentLayerDef;
        public int RestoreToParentWorkAmount = 600;
        public float InstantFullDestructionChance = 0.25f;
        public float alternativeDefChance = 0.50f;
        public float doubleDowngradeChance = 0.25f;

        public CompProperties_LayeredDestruction()
        {
            compClass = typeof(DestructibleBuilding);
        }
    }

    public class DestructibleBuilding : ThingComp
    {
        private readonly int fleckCount = 3;
        private DamageDef lastDamageTaken;
        private DestroyMode lastDestroyMode = DestroyMode.Vanish;

        public void ThrowFlecks(Map map, Vector3 loc, FleckDef fleckDef)
        {
            for (var i = 0; i < fleckCount; i++)
            {
                var creationData = FleckMaker.GetDataStatic(loc, map, fleckDef);
                creationData.rotation = Rand.Range(0, 360);
                creationData.rotationRate = Rand.Range(1f, 4f);
                creationData.airTimeLeft = Rand.Range(0.5f, 1f);
                creationData.velocityAngle = Rand.Range(0, 360);
                creationData.velocitySpeed = Rand.Range(1.5f, 4f);
                creationData.scale = Rand.Range(0.5f, 1f);
                creationData.spawnPosition = loc;
                map.flecks.CreateFleck(creationData);
            }
        }

        public void MakeFilth(Map map, IntVec3 position, Vector3 center)
        {
            //creates filth on adjacent 8 tiles based on wall material
            var stuff = parent.Stuff;
            if (stuff != null)
            {
                var stuffcategory = stuff.stuffProps.categories;

                ThingDef stuffFilthDef = null;
                FleckDef stuffFleckDef = null;
                if (stuffcategory.Contains(StuffCategoryDefOf.Woody))
                {
                    stuffFilthDef = LayeredDestructionDefOfs.Filth_LWD_Planks;
                    stuffFleckDef = LayeredDestructionDefOfs.Fleck_LWD_Planks;
                }
                else if (stuffcategory.Contains(StuffCategoryDefOf.Stony))
                {
                    stuffFilthDef = LayeredDestructionDefOfs.Filth_LWD_Bricks;
                    stuffFleckDef = LayeredDestructionDefOfs.Fleck_LWD_Brick;
                }
                else if (stuffcategory.Contains(StuffCategoryDefOf.Metallic))
                {
                    stuffFilthDef = LayeredDestructionDefOfs.Filth_LWD_Smooth;
                    stuffFleckDef = LayeredDestructionDefOfs.Fleck_LWD_Smooth;
                }

                if (stuffFleckDef != null) ThrowFlecks(map, center, stuffFleckDef);

                var list = GenAdj.AdjacentCells8WayRandomized();
                for (var i = 0; i < 8; i++)
                {
                    var cell = position + list[i];
                    if (cell.InBounds(map) && Rand.Chance(0.40f) && stuffFilthDef != null)
                        FilthMaker.TryMakeFilth(cell, map, stuffFilthDef);
                }
            }
        }

        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            if (dinfo != null) lastDamageTaken = dinfo.Value.Def;
            base.Notify_Killed(prevMap, dinfo);
            if (lastDestroyMode == DestroyMode.KillFinalize && lastDamageTaken != DamageDefOf.Mining &&
                lastDamageTaken != DamageDefOf.Flame)
            {
                //Only apply if wall is shot/burned/meleed/exploded/self-destructs
                var props = parent.def.GetCompProperties<CompProperties_LayeredDestruction>();

                if (!Rand.Chance(props.InstantFullDestructionChance) && props.NextLayerDef != null)
                {
                    //random chance that this whole mechanism doesn't apply per defs
                    // if this is not the last stage of wall
                    var nextStage = props.NextLayerDef;
                    if (props.NextLayerDef_Alternative != null && Rand.Chance(props.alternativeDefChance))
                    {
                        //chance to put alt stage
                        nextStage = props.NextLayerDef_Alternative;
                        if (Rand.Chance(props.doubleDowngradeChance) &&
                            nextStage.GetCompProperties<CompProperties_LayeredDestruction>().NextLayerDef != null)
                            nextStage = nextStage.GetCompProperties<CompProperties_LayeredDestruction>().NextLayerDef;
                    }

                    Thing WallLayer;
                    //inherit properties of parent wall
                    if (parent.Stuff != null)
                        WallLayer = ThingMaker.MakeThing(nextStage, parent.Stuff);
                    else
                        WallLayer = ThingMaker.MakeThing(nextStage);
                    //do this before setting faction
                    var isHomeArea = prevMap.areaManager.Home[parent.Position];
                    //if it's not player's wall, then neutral to not have issues with ownership
                    WallLayer.SetFaction(parent.Faction == Faction.OfPlayer ? Faction.OfPlayer : null);
                    if (parent.HasComp<CompColorable>())
                    {
                        WallLayer.SetColor(parent.DrawColor);
                    }
                    else
                    {
                        var building = WallLayer as Building;
                        building.ChangePaint((parent as Building)?.PaintColorDef);
                    }

                    //create trash according to material
                    MakeFilth(prevMap, parent.Position, parent.TrueCenter());
                    GenPlace.TryPlaceThing(WallLayer, parent.Position, prevMap, ThingPlaceMode.Direct);
                    if (WallLayer.Faction == Faction.OfPlayer && prevMap.areaManager.Home[WallLayer.Position] &&
                        isHomeArea)
                        //add designation for pawns to repair it
                        prevMap.designationManager.AddDesignation(new Designation(WallLayer,
                            RestoreDesignationDefOf.RestoreWall));
                }
                else if (props.NextLayerDef == null && props.ParentLayerDef != null && Find.PlaySettings.autoRebuild &&
                         parent.Faction == Faction.OfPlayer && props.ParentLayerDef.blueprintDef != null &&
                         props.ParentLayerDef.IsResearchFinished && prevMap.areaManager.Home[parent.Position] &&
                         GenConstruct.CanPlaceBlueprintAt(props.ParentLayerDef, parent.Position, parent.Rotation, prevMap))
                {
                    //when rubble is destroyed, place blueprints for parent when AutoRebuild is on
                    GenConstruct.PlaceBlueprintForBuild(props.ParentLayerDef, parent.Position, prevMap, parent.Rotation,
                        Faction.OfPlayer, parent.Stuff, parent.StyleSourcePrecept, parent.StyleDef);
                }
            }
        }

        public override void PostDestroy(DestroyMode mode, Map map)
        {
            lastDestroyMode = mode;

            base.PostDestroy(mode, map);
        }
    }
}