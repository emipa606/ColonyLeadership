using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Nandonalt_ColonyLeadership;

public class LordToil_SetLeadership : LordToil
{
    private readonly IntVec3 spot;

    public LordToil_SetLeadership(IntVec3 spot)
    {
        this.spot = spot;
    }

    public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
    {
        return DefDatabase<DutyDef>.GetNamed("GatherLeader").hook;
    }

    public override void UpdateAllDuties()
    {
        foreach (var pawn in lord.ownedPawns)
        {
            pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("GatherLeader"), spot);
        }
    }
}