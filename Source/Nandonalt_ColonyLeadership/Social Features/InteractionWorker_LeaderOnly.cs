using Nandonalt_ColonyLeadership.Config;
using RimWorld;
using Verse;

namespace Nandonalt_ColonyLeadership;

public class InteractionWorker_LeaderOnly : InteractionWorker
{
    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        var h1 = initiator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
        var h2 = initiator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
        var h3 = initiator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
        var h4 = initiator.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));
        if (h1 != null || h2 != null || h3 != null || h4 != null)
        {
            return 0.3f;
        }

        return 0f;
    }
}