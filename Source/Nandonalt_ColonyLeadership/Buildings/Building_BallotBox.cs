using System.Collections.Generic;
using Nandonalt_ColonyLeadership.Config;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Nandonalt_ColonyLeadership;

public class Building_BallotBox : Building
{
    //public List<int> allowedDays = 
    public bool allowElection = true;
    private int electionFreq = 3;
    public int lastElectionTick = -99999;

    protected virtual List<Pawn> getLeaders()
    {
        var pawns = new List<Pawn>();
        pawns.AddRange(IncidentWorker_SetLeadership.getAllColonists());
        var tpawns = new List<Pawn>();
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        foreach (var current in pawns)
        {
            var h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            var h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            var h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            var h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));

            if (h1 != null || h2 != null || h3 != null || h4 != null)
            {
            }
            else
            {
                tpawns.Add(current);
            }
        }

        foreach (var current in tpawns)
        {
            pawns.Remove(current);
        }

        return pawns;
    }

    public override void TickRare()
    {
        base.TickRare();

        if (!Utility.isDemocracy)
        {
            return;
        }

        var allowedDays = ConfigManager.getElectionDays();
        if (!allowedDays.Contains(GenLocalDate.DayOfSeason(Map)))
        {
            return;
        }

        if (!allowElection || Find.TickManager.TicksGame <= lastElectionTick + GenDate.TicksPerDay ||
            !Rand.MTBEventOccurs(0.03f, 60000f, 150f) || !AcceptableMapConditionsToStartElection(Map))
        {
            return;
        }

        var leaders = getLeaders();
        var pawnCount = new List<Pawn>();
        var needNewLeaders = false;
        pawnCount.AddRange(IncidentWorker_SetLeadership.getAllColonists());
        if (leaders.NullOrEmpty())
        {
            needNewLeaders = true;
        }

        if (leaders.Count <= 1 && pawnCount.Count >= 5)
        {
            needNewLeaders = true;
        }


        var canBeVoted = new List<Pawn>();
        canBeVoted.AddRange(IncidentWorker_SetLeadership.getAllColonists());
        var tpawns2 = new List<Pawn>();
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        foreach (var current in canBeVoted)
        {
            var h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            var h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            var h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            var h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));
            var h5 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leaderExpired"));
            if (h1 != null || h2 != null || h3 != null || h4 != null || h5 != null)
            {
                tpawns2.Add(current);
            }

            if (current.WorkTagIsDisabled(WorkTags.Social))
            {
                tpawns2.Add(current);
            }
        }

        foreach (var current in tpawns2)
        {
            canBeVoted.Remove(current);
        }

        if (canBeVoted.NullOrEmpty())
        {
            Messages.Message("ElectionFail_NoAbleLeader".Translate(), MessageTypeDefOf.RejectInput);
        }
        else
        {
            if (needNewLeaders)
            {
                TryStartGathering(Map);
            }
        }
    }


    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref allowElection, "allowElection", true);
        Scribe_Values.Look(ref lastElectionTick, "lastElectionTick", -99999);
    }

    public void TryStartGathering(Map map)
    {
        var pawn = GatheringsUtility.FindRandomGatheringOrganizer(Faction.OfPlayer, map, GatheringDefOf.Party);

        if (pawn == null)
        {
            Messages.Message("ElectionFail_ColonistsNotFound".Translate(), MessageTypeDefOf.RejectInput);

            return;
        }

        lastElectionTick = Find.TickManager.TicksGame;
        allowElection = false;
        LordMaker.MakeNewLord(pawn.Faction, new LordJob_Joinable_SetLeadership(Position), map);
        Find.LetterStack.ReceiveLetter("Election".Translate(), "ElectionGathering".Translate(),
            LetterDefOf.PositiveEvent, new TargetInfo(Position, map));
    }

    public override string GetInspectString()
    {
        var inspectString = base.GetInspectString();
        var str = "-/-";

        if (lastElectionTick > 0)
        {
            str = (Find.TickManager.TicksGame - lastElectionTick).ToStringTicksToPeriodVague() + " ago.";
        }

        if (!Utility.isDemocracy)
        {
            return inspectString + "ElectionFail_NoDemocracy".Translate();
        }

        if (!allowElection)
        {
            return inspectString + "BallotDescriptionDisabled".Translate() + str;
        }

        return inspectString + "BallotDescription".Translate() + str;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var c in base.GetGizmos())
        {
            yield return c;
        }

        if (Faction == Faction.OfPlayer)
        {
            yield return new Command_Toggle
            {
                hotKey = KeyBindingDefOf.Command_TogglePower,
                icon = TexCommand.ForbidOn,
                defaultLabel = "EnableElections".Translate(),
                defaultDesc = "EnableElectionsDesc".Translate(),
                isActive = () => allowElection,
                toggleAction = delegate { allowElection = !allowElection; }
            };
        }
    }


    public bool AcceptableMapConditionsToStartElection(Map map)
    {
        if (!GatheringsUtility.AcceptableGameConditionsToContinueGathering(map) ||
            !Position.Roofed(map) && !JoyUtility.EnjoyableOutsideNow(map))
        {
            return false;
        }

        if (GenLocalDate.HourInteger(map) < 8 || GenLocalDate.HourInteger(map) > 21)
        {
            return false;
        }

        var lords = map.lordManager.lords;
        foreach (var lord in lords)
        {
            if (lord.LordJob is LordJob_Joinable_Party || lord.LordJob is LordJob_Joinable_MarriageCeremony ||
                lord.LordJob is LordJob_Joinable_SetLeadership)
            {
                return false;
            }
        }

        if (map.dangerWatcher.DangerRating != StoryDanger.None)
        {
            return false;
        }

        var num2 = Mathf.RoundToInt(map.mapPawns.FreeColonistsSpawnedCount * 0.65f);
        num2 = Mathf.Clamp(num2, 2, 10);
        var num3 = 0;
        foreach (var current2 in map.mapPawns.FreeColonistsSpawned)
        {
            if (GatheringsUtility.ShouldGuestKeepAttendingGathering(current2))
            {
                num3++;
            }
        }

        return num3 >= num2;
    }
}