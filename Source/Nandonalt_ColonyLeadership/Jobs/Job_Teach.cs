using System.Collections.Generic;
using System.Linq;
using Nandonalt_ColonyLeadership.Config;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Nandonalt_ColonyLeadership;

internal class Job_Teach : JobDriver
{
    private const TargetIndex SpotIndex = TargetIndex.A;


    public List<Building_Chalkboard> chalkboards = [];


    private string report = "";
    public int tickC;

    protected Building_TeachingSpot Spot => (Building_TeachingSpot)job.GetTarget(TargetIndex.A).Thing;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref chalkboards, "chalkboards", LookMode.Reference);
        Scribe_Values.Look(ref tickC, "tickC");
    }


    public static List<Texture2D> iconList(Pawn current)
    {
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        var h1 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
        var h2 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
        var h3 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
        var h4 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));


        if (h1 != null)
        {
            return ModTextures.icons_leader1;
        }

        if (h2 != null)
        {
            return ModTextures.icons_leader2;
        }

        if (h3 != null)
        {
            return ModTextures.icons_leader3;
        }

        return h4 != null ? ModTextures.icons_leader4 : ModTextures.icons_leader1;
    }

    public override string GetReport()
    {
        return report != "" ? base.ReportStringProcessed(report) : base.GetReport();
    }


    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);

        // yield return Toils_Reserve.Reserve(SpotIndex, 1, -1, null);

        yield return new Toil
        {
            initAction = delegate
            {
                Spot.ChangeState(Building_TeachingSpot.State.lesson, Building_TeachingSpot.LessonState.gathering);
            }
        };

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

        var waitingTime = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Delay
        };
        TeachingUtility.remainingDuration = TeachingUtility.ritualDuration;
        waitingTime.defaultDuration = TeachingUtility.remainingDuration - 360;
        waitingTime.initAction = delegate
        {
            // Spot.lastLessonTick = Find.TickManager.TicksGame;
            // HediffLeader hediff = TeachingUtility.leaderH(this.pawn);
            //if (hediff != null) hediff.lastLessonTick = Find.TickManager.TicksGame;
            report = "WaitingDesc".Translate();
            MoteMaker.MakeInteractionBubble(pawn, null, ThingDefOf.Mote_Speech, ModTextures.waiting);
            Spot.ChangeState(Building_TeachingSpot.State.lesson, Building_TeachingSpot.LessonState.teaching);

            var list = GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, 10f, true).ToList();
            foreach (var current in list)
            {
                if (current is not Building_Chalkboard chalk)
                {
                    continue;
                }

                if (current.def.defName != "ChalkboardCL" || current.Faction != pawn.Faction)
                {
                    continue;
                }

                if (current.GetRoom() != pawn.GetRoom())
                {
                    continue;
                }

                _ = TeachingUtility.getLeaderType(pawn);
                chalkboards.Add(chalk);
                chalk.frame = -1;
                Map.mapDrawer.MapMeshDirty(chalk.Position, MapMeshFlagDefOf.Things, true, false);
            }
        };

        yield return waitingTime;


        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        for (var i = 0; i < 3; i++)
        {
            var teachingTime = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay
            };
            TeachingUtility.remainingDuration = TeachingUtility.ritualDuration;
            teachingTime.defaultDuration = TeachingUtility.remainingDuration - 120;
            teachingTime.initAction = delegate
            {
                var s = TeachingUtility.getLeaderType(pawn);
                foreach (var chalk in chalkboards)
                {
                    if (chalk == null)
                    {
                        continue;
                    }

                    switch (s)
                    {
                        case "leader1":
                            chalk.state = 1;
                            break;
                        case "leader2":
                            chalk.state = 2;
                            break;
                        case "leader3":
                            chalk.state = 3;
                            break;
                        default:
                        {
                            if (s == sciLeaderDeffName)
                            {
                                chalk.state = 4;
                            }

                            break;
                        }
                    }

                    chalk.frame++;
                    Map.mapDrawer.MapMeshDirty(chalk.Position, MapMeshFlagDefOf.Things, true, false);
                }

                report = "TeachingDesc".Translate();

                MoteMaker.MakeInteractionBubble(pawn, null, ThingDefOf.Mote_Speech, iconList(pawn).RandomElement());
            };

            teachingTime.tickAction = delegate
            {
                var actor = pawn;
                actor.skills.Learn(SkillDefOf.Social, 0.25f);
                actor.GainComfortFromCellIfPossible();
            };

            yield return teachingTime;
        }


        var finishingTime = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Delay
        };
        TeachingUtility.remainingDuration = TeachingUtility.ritualDuration;
        finishingTime.defaultDuration = TeachingUtility.remainingDuration - 360;
        finishingTime.WithProgressBarToilDelay(TargetIndex.A);
        finishingTime.initAction = delegate
        {
            report = "FinishLessonDesc".Translate();
            MoteMaker.MakeInteractionBubble(pawn, null, ThingDefOf.Mote_Speech, iconList(pawn).RandomElement());
        };

        finishingTime.tickAction = delegate
        {
            if (tickC is 120 or 240 or 360)
            {
                foreach (var chalk in chalkboards)
                {
                    if (chalk == null)
                    {
                        continue;
                    }

                    if (chalk.frame > -1)
                    {
                        chalk.frame--;
                    }

                    Map.mapDrawer.MapMeshDirty(chalk.Position, MapMeshFlagDefOf.Things, true, false);
                }
            }

            var actor = pawn;
            actor.skills.Learn(SkillDefOf.Social, 0.25f);
            actor.GainComfortFromCellIfPossible();
            tickC++;
        };

        yield return finishingTime;

        yield return new Toil
        {
            initAction = delegate
            {
                tickC = 0;
                TeachingUtility.TeachingComplete(pawn, Spot);
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };

        yield return new Toil
        {
            initAction = delegate
            {
                if (Spot == null)
                {
                    return;
                }

                if (Spot.currentLessonState != Building_TeachingSpot.LessonState.finished)
                {
                    Spot.ChangeState(Building_TeachingSpot.State.lesson,
                        Building_TeachingSpot.LessonState.finished);
                    //Map.GetComponent<MapComponent_SacrificeTracker>().ClearVariables();
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };


        AddFinishAction(_ =>
        {
            foreach (var chalk in chalkboards)
            {
                if (chalk == null)
                {
                    continue;
                }

                chalk.frame = 0;
                chalk.state = 0;
                Map.mapDrawer.MapMeshDirty(chalk.Position, MapMeshFlagDefOf.Things, true, false);
            }

            if (Spot.currentLessonState is Building_TeachingSpot.LessonState.finishing
                or Building_TeachingSpot.LessonState.finished)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("TaughtCL"));
            }
        });
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        Toils_Reserve.Reserve(SpotIndex);


        return true;
    }
}