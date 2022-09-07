using System.Collections.Generic;
using Nandonalt_ColonyLeadership.Config;
using RimWorld;
using Verse;
using Verse.AI;

namespace Nandonalt_ColonyLeadership;

public class IncidentWorker_Rebellion : IncidentWorker
{
    private const int FixedPoints = 30;

    public static void removeLeadership(Pawn current)
    {
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        var h1 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
        var h2 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
        var h3 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
        var h4 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));

        if (h2 != null)
        {
            h1 = h2;
        }

        if (h3 != null)
        {
            h1 = h3;
        }

        if (h4 != null)
        {
            h1 = h4;
        }


        var hediff = HediffMaker.MakeHediff(HediffDef.Named("leaderExpired"), current);
        hediff.Severity = 1f;
        var hl = (HediffLeader)hediff;
        hl.ticksLeader = h1.ticksLeader;
        current.health.AddHediff(hl);
        current.health.RemoveHediff(h1);
        current.needs.AddOrRemoveNeedsAsAppropriate();
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        //try
        //{


        var map = (Map)parms.target;

        var pawns = IncidentWorker_SetLeadership.getAllColonists();
        var tpawns2 = new List<Pawn>();

        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        foreach (var current in pawns)
        {
            var h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            var h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            var h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            var h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));
            if (h1 == null && h2 == null && h3 == null && h4 == null && current.Map == map)
            {
                tpawns2.Add(current);
            }
        }

        var pawn = tpawns2.RandomElement();

        if (pawn is not { Downed: false, Dead: false })
        {
            return false;
        }

        if (pawn.CurJob != null && pawn.jobs.curDriver.asleep != true)
        {
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        pawn.mindState.mentalStateHandler.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("Rebelling"));
        var s = "He";
        if (pawn.gender == Gender.Female)
        {
            s = "She";
        }

        //                Find.LetterStack.ReceiveLetter("RebelLetter".Translate(), "RebelLetterDesc".Translate(new object[] { pawn.Name.ToStringShort }), LetterDefOf.ThreatBig, pawn, null);

        Find.LetterStack.ReceiveLetter("RebelLetter".Translate(),
            "RebelLetterDesc".Translate(pawn.Name.ToStringShort), LetterDefOf.NegativeEvent, pawn);

        return true;

        /**}
        catch (Exception ex)
        {
            System.IO.File.WriteAllText("C:/Logs/debugLog.txt", ex.Message.ToString());
            
            return false;
        }**/
    }
}