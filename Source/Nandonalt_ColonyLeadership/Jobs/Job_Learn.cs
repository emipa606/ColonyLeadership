using System.Collections.Generic;
using Nandonalt_ColonyLeadership.Config;
using RimWorld;
using Verse;
using Verse.AI;

namespace Nandonalt_ColonyLeadership;

internal class Job_Learn : JobDriver
{
    private readonly TargetIndex Build = TargetIndex.A;
    private readonly TargetIndex Facing = TargetIndex.B;
    private readonly TargetIndex Spot = TargetIndex.C;
    private Pawn setTeacher;
    public List<SkillDef> skillPool = new List<SkillDef>();

    protected Building_TeachingSpot Spott => (Building_TeachingSpot)job.GetTarget(TargetIndex.A).Thing;

    protected Pawn TeacherPawn
    {
        get
        {
            if (setTeacher != null)
            {
                return setTeacher;
            }

            if (Spott.teacher != null)
            {
                setTeacher = Spott.teacher;
                return Spott.teacher;
            }

            foreach (var teacher in pawn.Map.mapPawns.FreeColonistsSpawned)
            {
                if (teacher.CurJob.def.defName != "TeachLesson")
                {
                    continue;
                }

                setTeacher = teacher;
                return teacher;
            }

            return null;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref setTeacher, "setTeacher");
        Scribe_Collections.Look(ref skillPool, "skillPool");
    }

    public bool setupSkills(Pawn teacher)
    {
        var leaderType = TeachingUtility.getLeaderType(teacher);
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        if (leaderType == "leader1")
        {
            if (teacher.skills.GetSkill(SkillDefOf.Plants).Level >= TeachingUtility.minSkill)
            {
                skillPool.Add(SkillDefOf.Plants);
            }

            if (teacher.skills.GetSkill(SkillDefOf.Medicine).Level >= TeachingUtility.minSkill)
            {
                skillPool.Add(SkillDefOf.Medicine);
            }

            if (teacher.skills.GetSkill(SkillDefOf.Animals).Level >= TeachingUtility.minSkill)
            {
                skillPool.Add(SkillDefOf.Animals);
            }
        }
        else if (leaderType == "leader2")
        {
            if (teacher.skills.GetSkill(SkillDefOf.Shooting).Level >= TeachingUtility.minSkill)
            {
                skillPool.Add(SkillDefOf.Shooting);
            }

            if (teacher.skills.GetSkill(SkillDefOf.Melee).Level >= TeachingUtility.minSkill)
            {
                skillPool.Add(SkillDefOf.Melee);
            }
        }
        else if (leaderType == "leader3")
        {
            if (teacher.skills.GetSkill(SkillDefOf.Construction).Level >= TeachingUtility.minSkill)
            {
                skillPool.Add(SkillDefOf.Construction);
            }

            if (teacher.skills.GetSkill(SkillDefOf.Crafting).Level >= TeachingUtility.minSkill)
            {
                skillPool.Add(SkillDefOf.Crafting);
            }

            if (teacher.skills.GetSkill(SkillDefOf.Artistic).Level >= TeachingUtility.minSkill)
            {
                skillPool.Add(SkillDefOf.Artistic);
            }
        }
        else if (leaderType == sciLeaderDeffName)
        {
            if (teacher.skills.GetSkill(SkillDefOf.Intellectual).Level >= TeachingUtility.minSkill)
            {
                skillPool.Add(SkillDefOf.Intellectual);
            }

            if (teacher.skills.GetSkill(SkillDefOf.Medicine).Level >= TeachingUtility.minSkill)
            {
                skillPool.Add(SkillDefOf.Medicine);
            }
        }

        return !skillPool.NullOrEmpty();
    }


    protected override IEnumerable<Toil> MakeNewToils()
    {
        rotateToFace = Facing;

        AddEndCondition(() =>
            TeacherPawn.CurJob.def.defName != "TeachLesson" ? JobCondition.Incompletable : JobCondition.Ongoing);
        this.EndOnDespawnedOrNull(Spot);
        this.EndOnDespawnedOrNull(Build);


        yield return Toils_Reserve.Reserve(Spot, job.def.joyMaxParticipants, 0);
        var gotoPreacher = TargetC.HasThing
            ? Toils_Goto.GotoThing(Spot, PathEndMode.OnCell)
            : Toils_Goto.GotoCell(Spot, PathEndMode.OnCell);

        yield return gotoPreacher;


        var b = setupSkills(Spott.teacher);
        var spotToil = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Delay,
            defaultDuration = 9999
        };
        spotToil.AddPreTickAction(() =>
        {
            pawn.GainComfortFromCellIfPossible();
            ticksLeftThisToil = 9999;

            //LEARN
            var actor = pawn;

            var leaderReport = Spott.teacher.jobs.curDriver.GetReport();
            if (leaderReport == "TeachingDesc".Translate() || leaderReport == "FinishLessonDesc".Translate())
            {
                actor.skills.Learn(
                    skillPool.RandomElementByWeight(d => 1f + Spott.teacher.skills.GetSkill(d).Level),
                    TeachingUtility.learningFactor * Spott.GetStatValue(StatDef.Named("LearningSpeedFactor")));
            }

            //

            if (TeacherPawn.CurJob.def.defName != "TeachLesson" || !b)
            {
                ticksLeftThisToil = -1;
            }
        });
        yield return spotToil;

        yield return Toils_Reserve.Release(Spot);

        AddFinishAction(() =>
        {
            if (Spott.currentLessonState == Building_TeachingSpot.LessonState.finishing ||
                Spott.currentLessonState == Building_TeachingSpot.LessonState.finished)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(
                    Rand.Range(0f, 1f) < 0.8f ? ThoughtDef.Named("LessonPositive") : ThoughtDef.Named("LessonNegative"),
                    Spott.teacher);
                //CultUtility.AttendWorshipTickCheckEnd(PreacherPawn, this.pawn);
                //Cthulhu.Utility.DebugReport("Called end tick check");
                //what happens to learner
            }

            if (TargetC.HasThing)
            {
                if (Map.reservationManager.IsReservedByAnyoneOf(job.targetC.Thing, Faction.OfPlayer))
                {
                    Map.reservationManager.Release(job.targetC.Thing, pawn, job);
                }
            }
            else
            {
                if (Map.reservationManager.IsReservedByAnyoneOf(job.targetC.Cell, Faction.OfPlayer))
                {
                    Map.reservationManager.Release(job.targetC.Cell, pawn, job);
                }
            }
        });
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        Toils_Reserve.Reserve(Spot, job.def.joyMaxParticipants, 0);
        return true;
    }
}