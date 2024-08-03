using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nandonalt_ColonyLeadership.Config;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Nandonalt_ColonyLeadership;

public class IncidentWorker_SetLeadership : IncidentWorker
{
    private readonly Random r = new Random();

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var pawns = new List<Pawn>();
        pawns.AddRange(getAllColonists());
        currentLeaders(out var count);

        if (pawns.Count >= 5)
        {
            switch (count)
            {
                case < 1:
                    ElectLeader([]);
                    ElectLeader([]);
                    break;
                case 1:
                    ElectLeader([]);
                    break;
            }
        }
        else
        {
            if (count < 1)
            {
                ElectLeader([]);
            }
        }

        return true;
    }

    public List<Pawn> currentLeaders(out int count)
    {
        var pawns = new List<Pawn>();
        var c = 0;
        //Go through each pawn, if the pawn is currently a leader, add him to our list leaders. 
        foreach (var current in getAllColonists())
        {
            var h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            var h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            var h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            var h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
            var h5 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader5"));

            if (h1 == null && h2 == null && h3 == null && h4 == null && h5 == null)
            {
                continue;
            }

            pawns.Add(current);
            c++;
        }

        count = c;
        return pawns;
    }

    public static List<Pawn> getAllColonists()
    {
        var pawns = new List<Pawn>();
        pawns.AddRange(PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists);
        return pawns;
    }

    public void ElectLeader(List<Pawn> toBeIgnored)
    {
        var pawns = new List<Pawn>();
        var canBeVoted = new List<Pawn>();
        var votes = new List<Pawn>();
        pawns.AddRange(getAllColonists());
        canBeVoted.AddRange(getAllColonists());
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

        foreach (var current in toBeIgnored)
        {
            canBeVoted.Remove(current);
        }

        var bestOf = new List<Pawn>();
        var bestBotanist = getBestOf(canBeVoted, getBotanistScore);
        var bestWarrior = getBestOf(canBeVoted, getWarriorScore);
        var bestCarpenter = getBestOf(canBeVoted, getCarpenterScore);
        var bestScientist = getBestOf(canBeVoted, getScientistScore);

        if (bestBotanist != null)
        {
            bestOf.Add(bestBotanist);
        }

        if (bestWarrior != null)
        {
            bestOf.Add(bestWarrior);
        }

        if (bestCarpenter != null)
        {
            bestOf.Add(bestCarpenter);
        }

        if (bestScientist != null)
        {
            bestOf.Add(bestScientist);
        }


        string targetLeader = null;

        if (bestOf.NullOrEmpty() || canBeVoted.NullOrEmpty())
        {
            Messages.Message("NoColonistAbleLeader".Translate(), MessageTypeDefOf.NegativeEvent);

            return;
        }

        foreach (var current in pawns)
        {
            var votedForPawn = current;
            var lastopinion = 0;

            foreach (var bestPawn in bestOf)
            {
                if (bestPawn == current)
                {
                    continue;
                }

                var opinion = current.relations.OpinionOf(bestPawn);
                if (opinion <= lastopinion)
                {
                    continue;
                }

                lastopinion = opinion;
                votedForPawn = bestPawn;
            }

            votes.Add(votedForPawn);
        }

        var most = (from i in votes
            group i by i
            into grp
            orderby grp.Count() descending
            select grp.Key).First();

        float[] leaderAptitudes =
            [getBotanistScore(most), getWarriorScore(most), getCarpenterScore(most), getScientistScore(most)];
        Array.Sort(leaderAptitudes);
        var maxValue = leaderAptitudes[3];
        var secondMax = leaderAptitudes[2];
        var diff = maxValue - secondMax;
        if (diff < 3)
        {
            var factor = rnd(0, 1);
            if (factor > 0.60)
            {
                maxValue = secondMax; //This will cause the leaders second most skilled proficiency to be their leadership type this election. 
            }
        }

        if (maxValue == getBotanistScore(most))
        {
            targetLeader = "leader1";
        }

        if (maxValue == getWarriorScore(most))
        {
            targetLeader = "leader2";
        }

        if (maxValue == getCarpenterScore(most))
        {
            targetLeader = "leader3";
        }

        if (maxValue == getScientistScore(most))
        {
            targetLeader = sciLeaderDeffName;
        }

        var hediff = HediffMaker.MakeHediff(HediffDef.Named(targetLeader), most);


        if (toBeIgnored.Contains(most))
        {
            Messages.Message(
                "SomethingBadHappenedWithElection".Translate(),
                MessageTypeDefOf.NegativeEvent);

            return;
        }

        foreach (var p in currentLeaders(out _))
        {
            if (p.health.hediffSet.GetFirstHediffOfDef(hediff.def) == null)
            {
                continue;
            }

            toBeIgnored.Add(most);
            ElectLeader(toBeIgnored);
            return;
        }

        doElect(most, hediff);
    }

    private double rnd(double a, double b)
    {
        return a + (r.NextDouble() * (b - a));
    }

    public static void doElect(Pawn pawn, Hediff hediff, bool forced = false)
    {
        hediff.Severity = 0.1f;
        pawn.health.AddHediff(hediff);
        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("ElectedLeader"));


        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(
            forced
                ? "LeaderChosen".Translate(pawn.Name.ToStringFull, hediff.LabelBase)
                : "LeaderElected".Translate(pawn.Name.ToStringFull, hediff.LabelBase));

        if (Prefs.DevMode)
        {
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("--DEBUG-DEV---");
            stringBuilder.AppendLine($"Botanist Score: {getBotanistScore(pawn)}");
            stringBuilder.AppendLine($"Warrior Score: {getWarriorScore(pawn)}");
            stringBuilder.AppendLine($"Carpenter Score: {getCarpenterScore(pawn)}");
            stringBuilder.AppendLine($"Scientist Score: {getScientistScore(pawn)}");
        }

        if (Utility.getGov() != null)
        {
            Find.LetterStack.ReceiveLetter("NewLeaderLetterTitle".Translate(Utility.getGov().nameMale),
                stringBuilder.ToString(), LetterDefOf.PositiveEvent, pawn);
        }
        else
        {
            Find.LetterStack.ReceiveLetter("New Leader", stringBuilder.ToString(), LetterDefOf.PositiveEvent, pawn);
        }

        foreach (var p in getAllColonists())
        {
            if (p == pawn)
            {
                continue;
            }

            var num2 = p.relations.OpinionOf(pawn);
            if (num2 <= -20)
            {
                p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("RivalLeader"));
            }

            if (p.story.traits.HasTrait(TraitDef.Named("Jealous")) && TeachingUtility.leaderH(p) == null)
            {
                p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("ElectedLeaderJealous"));
            }
        }
    }

    public static bool TryStartGathering(Map map)
    {
        var organizer = GatheringsUtility.FindRandomGatheringOrganizer(Faction.OfPlayer, map, GatheringDefOf.Party);


        //Pawn = PartyUtility.FindRandomPartyOrganizer(Faction.OfPlayer, map);
        if (organizer == null)
        {
            Messages.Message("ElectionFail_ColonistsNotFound".Translate(), MessageTypeDefOf.RejectInput);

            return false;
        }

        //RCellFinder.TryFindGatheringSpot(pawn, GatheringDef.Named(""), intVec)
        if (!RCellFinder.TryFindGatheringSpot(organizer, GatheringDefOf.Party, false, out var intVec))
        {
            Messages.Message("NoSafeSpotElection".Translate(), MessageTypeDefOf.RejectInput);

            return false;
        }

        LordMaker.MakeNewLord(organizer.Faction, new LordJob_Joinable_SetLeadership(intVec), map);
        Find.LetterStack.ReceiveLetter("Election".Translate(), "ElectionGathering".Translate(),
            LetterDefOf.PositiveEvent,
            new TargetInfo(intVec, map));
        return true;
    }


    public Pawn getBestOf(List<Pawn> pawns, Func<Pawn, float> getScore)
    {
        Pawn selected = null;
        float lastScore = 0;
        foreach (var current in pawns)
        {
            var score = getScore(current);
            if (!(score > lastScore))
            {
                continue;
            }

            selected = current;
            lastScore = score;
        }

        return lastScore == 0 ? null : selected;
    }

    public static float getBotanistScore(Pawn pawn)
    {
        if (pawn.WorkTagIsDisabled(WorkTags.PlantWork))
        {
            return 0;
        }

        if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Growing))
        {
            return 0;
        }

        var a = pawn.skills.GetSkill(SkillDefOf.Plants).Level *
                getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Plants)) * 0.4f;
        var b = pawn.skills.GetSkill(SkillDefOf.Medicine).Level *
                getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Medicine)) * 0.3f;
        var c = pawn.skills.GetSkill(SkillDefOf.Animals).Level *
                getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Animals)) * 0.3f;
        return a + b + c;
    }

    public static float getWarriorScore(Pawn pawn)
    {
        if (pawn.WorkTagIsDisabled(WorkTags.Violent))
        {
            return 0;
        }

        var a = pawn.skills.GetSkill(SkillDefOf.Shooting).Level *
                getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Shooting));
        var b = pawn.skills.GetSkill(SkillDefOf.Melee).Level * getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Melee));
        return (a + b) / 2;
    }

    public static float getScientistScore(Pawn pawn)
    {
        if (pawn.WorkTagIsDisabled(WorkTags.Intellectual))
        {
            return 0;
        }

        var a = pawn.skills.GetSkill(SkillDefOf.Intellectual).Level *
                getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Intellectual)) * 1.5f;
        return a;
    }

    public static float getCarpenterScore(Pawn pawn)
    {
        if (pawn.WorkTagIsDisabled(WorkTags.Crafting))
        {
            return 0;
        }

        if (pawn.WorkTagIsDisabled(WorkTags.ManualSkilled))
        {
            return 0;
        }

        var a = pawn.skills.GetSkill(SkillDefOf.Construction).Level *
                getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Construction)) * 0.4f;
        var b = pawn.skills.GetSkill(SkillDefOf.Crafting).Level *
                getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Crafting)) * 0.4f;
        var c = pawn.skills.GetSkill(SkillDefOf.Artistic).Level *
                getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Artistic)) * 0.2f;
        return a + b + c;
    }

    public static float getOverallScore(Pawn pawn)
    {
        return getBotanistScore(pawn) + getWarriorScore(pawn) + getScientistScore(pawn) + getCarpenterScore(pawn);
    }

    public static float getPassionFactor(SkillRecord skill)
    {
        if (skill.passion == Passion.Major)
        {
            return 1.5f;
        }

        return skill.passion == Passion.Minor ? 1.25f : 1f;
    }
}