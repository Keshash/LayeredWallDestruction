using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace LayeredDestruction
{
    // Token: 0x020015E8 RID: 5608
    public class Designator_RestoreWall : Designator_Cells
    {
        //public override int DraggableDimensions => 2;

        public override DrawStyleCategoryDef DrawStyleCategory
        {
            get
            {
                return DrawStyleCategoryDefOf.Orders;
            }
        }
        protected override DesignationDef Designation => RestoreDesignationDefOf.RestoreWall;

        public override bool DragDrawMeasurements
        {
            get
            {
                return true;
            }
        }

        public Designator_RestoreWall()
        {
            this.defaultLabel = "DesignatorSmoothSurface".Translate();
            this.defaultDesc = "DesignatorSmoothSurfaceDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/RestoreWall", true);
            this.useMouseIcon = true;
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.soundSucceeded = SoundDefOf.Designate_SmoothSurface;
            this.hotKey = KeyBindingDefOf.Misc5;
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if (base.Map.designationManager.DesignationOn(t, RestoreDesignationDefOf.RestoreWall) != null)
            {
                return "SurfaceBeingSmoothed".Translate();
            }
            var props = t.def.GetCompProperties<CompProperties_LayeredDestruction>() ?? new CompProperties_LayeredDestruction();
            if (t != null && props.ParentLayerDef != null && this.CanDesignateCell(t.Position).Accepted)
            {
                return AcceptanceReport.WasAccepted;
            }
            return false;
        }

        public override void DesignateThing(Thing t)
        {
            //this.DesignateSingleCell(t.Position);
            base.Map.designationManager.AddDesignation(new Designation(t, Designation));
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            if (c.Fogged(base.Map))
            {
                return false;
            }
            //if (base.Map.designationManager.DesignationAt(c, RestoreDesignationDefOf.RestoreWall) != null || base.Map.designationManager.DesignationOn(c, RestoreDesignationDefOf.RestoreWall) != null)
            //{
            //    return "SurfaceBeingSmoothed".Translate();
            //}
            //foreach (Thing t in c.GetThingList(base.Map))
            //{
            //    if (this.CanDesignateThing(t).Accepted)
            //    {
            //        return true;
            //    }
            //}
            if (c.InNoBuildEdgeArea(base.Map))
            {
                return "TooCloseToMapEdge".Translate();
            }
            Building edifice = c.GetEdifice(base.Map);
            if (edifice != null && edifice.def.GetCompProperties<CompProperties_LayeredDestruction>().ParentLayerDef != null)
            {
                return AcceptanceReport.WasAccepted;
            }
            return AcceptanceReport.WasAccepted;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            Building edifice = c.GetEdifice(base.Map);
            if (edifice != null && edifice.def.GetCompProperties<CompProperties_LayeredDestruction>().ParentLayerDef != null)
            {
                base.Map.designationManager.AddDesignation(new Designation(c, RestoreDesignationDefOf.RestoreWall, null));
                //base.Map.designationManager.TryRemoveDesignation(c, DesignationDefOf.Mine);
                return;
            }
            //base.Map.designationManager.AddDesignation(new Designation(c, RestoreDesignationDefOf.RestoreWall, null));
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }
    }
}
