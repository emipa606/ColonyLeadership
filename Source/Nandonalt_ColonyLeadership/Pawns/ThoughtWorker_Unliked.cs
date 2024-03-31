using RimWorld;
using Verse;

namespace Nandonalt_ColonyLeadership;

public class ThoughtWorker_Unliked : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        var need = (Need_LeaderLevel)p.needs.TryGetNeed(DefDatabase<NeedDef>.GetNamed("LeaderLevel"));
        if (need == null)
        {
            return ThoughtState.Inactive;
        }

        if (need.opinion <= -50)
        {
            return ThoughtState.ActiveAtStage(1);
        }

        return need.opinion <= -10 ? ThoughtState.ActiveAtStage(0) : ThoughtState.Inactive;
    }
}