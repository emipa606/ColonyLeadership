using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Nandonalt_ColonyLeadership;

public class LordJob_Joinable_SetLeadership : LordJob_VoluntarilyJoinable
{
    private IntVec3 spot;

    private Trigger_TicksPassed timeoutTrigger;

    public LordJob_Joinable_SetLeadership()
    {
    }

    public LordJob_Joinable_SetLeadership(IntVec3 spot)
    {
        this.spot = spot;
    }

    public override StateGraph CreateGraph()
    {
        var stateGraph = new StateGraph();
        var lordToil_Party = new LordToil_SetLeadership(spot);
        stateGraph.AddToil(lordToil_Party);
        var lordToil_End = new LordToil_End();
        stateGraph.AddToil(lordToil_End);
        var transition = new Transition(lordToil_Party, lordToil_End);
        transition.AddTrigger(new Trigger_TickCondition(ShouldBeCalledOff));
        transition.AddTrigger(new Trigger_PawnLostViolently());
        transition.AddPreAction(new TransitionAction_Message("ElectionFail_Disaster".Translate(),
            MessageTypeDefOf.NegativeEvent, new TargetInfo(spot, Map)));
        stateGraph.AddTransition(transition);
        timeoutTrigger = new Trigger_TicksPassed(Rand.RangeInclusive(5000, 8000));
        var transition2 = new Transition(lordToil_Party, lordToil_End);
        transition2.AddTrigger(timeoutTrigger);

        transition2.AddPreAction(new TransitionAction_Message("ElectionFinished".Translate(),
            MessageTypeDefOf.PositiveEvent, new TargetInfo(spot, Map)));
        transition2.AddPreAction(new TransitionAction_Custom(Finished));
        stateGraph.AddTransition(transition2);
        return stateGraph;
    }

    private bool ShouldBeCalledOff()
    {
        return !GatheringsUtility.AcceptableGameConditionsToContinueGathering(Map) ||
               !spot.Roofed(Map) && !JoyUtility.EnjoyableOutsideNow(Map);
    }

    private void Finished()
    {
        var ownedPawns = lord.ownedPawns;
        var num = 0;
        foreach (var pawn in ownedPawns)
        {
            if (!GatheringsUtility.InGatheringArea(pawn.Position, spot, Map))
            {
                continue;
            }

            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("AttendedElection"));
            num++;
        }

        if (num != 0)
        {
            var parms = StorytellerUtility.DefaultParmsNow(IncidentDef.Named("SetLeadership").category, Map);
            IncidentDef.Named("SetLeadership").Worker.TryExecute(parms);
        }
        else
        {
            Messages.Message("ElectionNoAttendees".Translate(), MessageTypeDefOf.RejectInput);
        }
    }

    public override float VoluntaryJoinPriorityFor(Pawn p)
    {
        if (!IsInvited(p))
        {
            return 0f;
        }

        if (!GatheringsUtility.ShouldGuestKeepAttendingGathering(p))
        {
            return 0f;
        }

        if (!lord.ownedPawns.Contains(p) && IsPartyAboutToEnd())
        {
            return 0f;
        }

        return 20f;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref spot, "spot");
    }

    public override string GetReport(Pawn pawn)
    {
        return "AttendingElectionDesc".Translate();
    }

    private bool IsPartyAboutToEnd()
    {
        return timeoutTrigger.TicksLeft < 1200;
    }

    private bool IsInvited(Pawn p)
    {
        return p.Faction == lord.faction;
    }
}