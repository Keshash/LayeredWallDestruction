using LayeredDestruction;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;


namespace LayeredDestruction
{
    internal class JobDriver_RestoreWall : JobDriver
    {

        protected DesignationDef DesDef
        {
            get
            {
                return RestoreDesignationDefOf.RestoreWall;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed) && this.pawn.Reserve(this.job.targetA.Cell, this.job, 1, -1, null, errorOnFailed);

        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var props = this.TargetA.Thing.def.GetCompProperties<CompProperties_LayeredDestruction>();
            this.FailOn(() => !this.job.ignoreDesignations && this.Map.designationManager.DesignationOn(this.TargetA.Thing, this.DesDef) == null);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            Toil doWork = ToilMaker.MakeToil("MakeNewToils");
            doWork.initAction = delegate ()
            {
                this.workLeft = (float)props.RestoreToParentWorkAmount;
            };
            doWork.tickAction = delegate ()
            {
                float num = doWork.actor.GetStatValue(StatDefOf.SmoothingSpeed, true, -1) * 1.7f;
                this.workLeft -= num;
                if (doWork.actor.skills != null)
                {
                    doWork.actor.skills.Learn(SkillDefOf.Construction, 0.1f, false);
                }
                if (this.workLeft <= 0f)
                {
                    this.DoEffect();
                    Designation designation = this.Map.designationManager.DesignationOn(this.TargetA.Thing, this.DesDef);
                    if (designation != null)
                    {
                        designation.Delete();
                    }
                    this.ReadyForNextToil();
                    return;
                }
            };
            doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            doWork.WithEffect(this.TargetA.Thing.def.repairEffect, TargetIndex.A, null);
            doWork.WithProgressBar(TargetIndex.A, () => 1f - this.workLeft / (float)props.RestoreToParentWorkAmount, false, -0.5f, false);

            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            doWork.activeSkill = (() => SkillDefOf.Construction);
            yield return doWork;
            yield break;
        }

        protected void DoEffect()
        {
            RestoreWallUtility.RestoreWall(base.TargetA.Thing, this.pawn);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0f, false);
        }

        private float workLeft = -1000f;
    }
}