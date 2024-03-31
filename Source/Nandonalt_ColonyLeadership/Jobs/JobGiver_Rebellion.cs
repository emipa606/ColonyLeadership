using System.Collections.Generic;
using Nandonalt_ColonyLeadership.Config;
using RimWorld;
using Verse;
using Verse.AI;

namespace Nandonalt_ColonyLeadership;

internal class JobGiver_Rebellion : ThinkNode_JobGiver
{
    private static List<Thing> potentialTargets = [];
    private IntRange waitTicks = new IntRange(80, 140);

    public override ThinkNode DeepCopy(bool resolve = true)
    {
        var JobGiver_Rebellion = (JobGiver_Rebellion)base.DeepCopy(resolve);
        JobGiver_Rebellion.waitTicks = waitTicks;
        return JobGiver_Rebellion;
    }

    protected override Job TryGiveJob(Pawn pawn)
    {
        if (pawn.mindState.nextMoveOrderIsWait)
        {
            var job = new Job(JobDefOf.Wait_Wander)
            {
                expiryInterval = waitTicks.RandomInRange
            };
            pawn.mindState.nextMoveOrderIsWait = false;
            return job;
        }

        if (Rand.Value < 0.75f)
        {
            var target = getMostHatedLeaderBy(pawn);
            if (target != null)
            {
                var dest = new LocalTargetInfo(target);


                if (!pawn.CanReach(dest, PathEndMode.OnCell, Danger.Deadly))
                {
                    pawn.mindState.nextMoveOrderIsWait = false;
                }
                else if (!pawn.CanReserve(dest.Thing))
                {
                    pawn.mindState.nextMoveOrderIsWait = false;
                }
                else
                {
                    var pTarg = (Pawn)dest.Thing;

                    var building_Bed = RestUtility.FindBedFor(pTarg, pawn, true, false, GuestStatus.Guest);
                    if (building_Bed == null)
                    {
                        return null;
                    }

                    var job = new Job(DefDatabase<JobDef>.GetNamed("ArrestLeader"), pTarg, building_Bed)
                    {
                        count = 1
                    };

                    pawn.mindState.nextMoveOrderIsWait = true;
                    return job;
                }
            }
            else
            {
                pawn.mindState.nextMoveOrderIsWait = false;
            }
        }

        var intVec = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 10f, null, Danger.Deadly);
        if (!intVec.IsValid)
        {
            return null;
        }

        pawn.mindState.nextMoveOrderIsWait = true;
        pawn.Map.pawnDestinationReservationManager.Reserve(pawn, pawn.CurJob, intVec);
        return new Job(JobDefOf.GotoWander, intVec);
    }

    public static Pawn getMostHatedLeaderBy(Pawn pawn)
    {
        var pawns = IncidentWorker_SetLeadership.getAllColonists();
        var tpawns2 = new List<Pawn>();
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        foreach (var current in pawns)
        {
            var h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            var h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            var h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            var h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));
            if (h1 != null || h2 != null || h3 != null || h4 != null)
            {
                tpawns2.Add(current);
            }
        }

        Pawn temp = null;
        float lastopinion = 100;
        foreach (var p2 in tpawns2)
        {
            if (p2 == pawn)
            {
                continue;
            }

            float opinion = pawn.relations.OpinionOf(p2);
            if (!(opinion <= lastopinion))
            {
                continue;
            }

            lastopinion = opinion;
            temp = p2;
        }

        if (temp == null)
        {
            return temp;
        }

        if (pawn.relations.OpinionOf(temp) > -60f)
        {
            temp = null;
        }

        return temp;
    }
}