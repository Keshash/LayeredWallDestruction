using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace LayeredDestruction
{
    public class WorkGiver_ConstructRestoreWall : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            foreach (Designation designation in pawn.Map.designationManager.SpawnedDesignationsOfDef(RestoreDesignationDefOf.RestoreWall))
            {
                yield return designation.target.Cell;
            }
            // IEnumerator<Designation> enumerator = null;
            yield break;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(RestoreDesignationDefOf.RestoreWall);
        }

        public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
        {
            //return pawn.Map.designationManager.DesignationOn(c.GetEdifice(pawn.Map), RestoreDesignationDefOf.RestoreWall) != null;
            Building edifice = c.GetEdifice(pawn.Map);
            if (c.IsForbidden(pawn) || pawn.Map.designationManager.DesignationOn(edifice, RestoreDesignationDefOf.RestoreWall) == null || pawn.Faction != Faction.OfPlayer)
            {
                return false;
            }
            
            if (edifice == null)
            {
                Log.ErrorOnce("Failed to find valid edifice when trying to restore a wall", 58988176);
                pawn.Map.designationManager.TryRemoveDesignationOn(edifice, RestoreDesignationDefOf.RestoreWall);
                return false;
            }
            return pawn.CanReserve(edifice, 1, -1, null, forced) && pawn.CanReserve(c, 1, -1, null, forced);

        }

        public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
        {
            return JobMaker.MakeJob(LayeredDestructionDefOfs.RestoreWall, c.GetEdifice(pawn.Map));
        }
    }
}

