using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Nandonalt_ColonyLeadership;

public class HediffLeader : HediffWithComps
{
    private static readonly int ticksPeriod = GenDate.TicksPerYear;

    private int counter;

    //static int ticksPeriod = GenDate.TicksPerHour;
    public int ticksLeader = ticksPeriod;

    public override string TipStringExtra
    {
        get
        {
            var stringBuilder = new StringBuilder();
            if (!Utility.isDictatorship)
            {
                stringBuilder.AppendLine("TimeLeftL".Translate() + ticksLeader.ToStringTicksToPeriod());
            }

            stringBuilder.AppendLine("----------");
            stringBuilder.Append(base.TipStringExtra);
            return stringBuilder.ToString();
        }
    }

    public override string LabelBase
    {
        get
        {
            var lab = def.label;
            var MyChar = "Leader".ToCharArray();
            var NewString = lab.TrimStart(MyChar);
            var gov = Utility.getGov();
            if (gov == null)
            {
                return def.label;
            }

            if (pawn.gender == Gender.Female)
            {
                return gov.nameMale + NewString;
            }

            return gov.nameFemale + NewString;
        }
    }

    public Need_LeaderLevel Need
    {
        get
        {
            if (pawn.Dead)
            {
                return null;
            }

            return (Need_LeaderLevel)pawn.needs.AllNeeds.Find(
                x => x.def == DefDatabase<NeedDef>.GetNamed("LeaderLevel"));
        }
    }


    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksLeader, "ticksLeader", ticksPeriod);
    }

    public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
    {
        if (!isThisExpired())
        {
            Messages.Message("LeaderDied".Translate(def.label, pawn.Name.ToStringFull),
                MessageTypeDefOf.PawnDeath);
            foreach (var p in IncidentWorker_SetLeadership.getAllColonists())
            {
                var num2 = p.relations.OpinionOf(pawn);
                p.needs.mood.thoughts.memories.TryGainMemory(
                    num2 <= -20 ? ThoughtDef.Named("LeaderDiedRival") : ThoughtDef.Named("LeaderDied"), pawn);
            }

            if (Utility.isDictatorship || Utility.isMonarchy)
            {
                Find.WindowStack.Add(new Dialog_ChooseLeader());
            }
        }

        pawn.health.RemoveHediff(this);
    }


    public bool isThisExpired()
    {
        return def.defName == "leaderExpired";
    }


    public override void Tick()
    {
        base.Tick();
        ticksLeader--;
        if (ticksLeader == 0 && !Utility.isDictatorship)
        {
            pawn.health.RemoveHediff(this);
            //This used to be: LetterDefOf.BadNonUrgent (or something like that, no longer exists as a def in rimworld). changed to NeutralEvent until we know what it does. 
            Find.LetterStack.ReceiveLetter("LeaderEndLetter", $"LeaderEndLetterDesc{pawn.Name.ToStringFull}",
                LetterDefOf.NeutralEvent, pawn);
        }

        if (isThisExpired())
        {
            return;
        }

        Severity = Mathf.Clamp(Need.CurLevel, 0.01f, 1f);

        var actor = pawn;
        if (actor.CurJob != null && actor.jobs.curDriver.asleep)
        {
            return;
        }

        if (actor.InAggroMentalState)
        {
            return;
        }

        if (!actor.Spawned && actor.CarriedBy != null)
        {
            return;
        }

        if (actor.Map == null)
        {
            return;
        }

        counter += 1;

        if (counter < 100)
        {
            return;
        }

        string label;
        switch (def.defName)
        {
            case "leader1":
                label = "leaderAura1";
                break;
            case "leader2":
                label = "leaderAura2";
                break;
            case "leader3":
                label = "leaderAura3";
                break;
            default:
                label = "leaderAura4";
                break;
        }

        var list = GenRadial.RadialDistinctThingsAround(actor.Position, actor.Map, 10f, true).ToList();
        foreach (var current in list)
        {
            if (current is not Pawn currentPawn)
            {
                continue;
            }

            if (!currentPawn.IsColonistPlayerControlled || currentPawn.Dead || currentPawn == actor)
            {
                continue;
            }

            if (currentPawn.GetRoom() != pawn.GetRoom())
            {
                continue;
            }

            var hediff = currentPawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(label));
            if (hediff != null)
            {
                //hediff.Severity = Need.CurLevel;
            }
            else
            {
                hediff = HediffMaker.MakeHediff(HediffDef.Named(label), currentPawn);
                hediff.Severity = Mathf.Clamp(Need.CurLevel, 0.01f, 1f);
                var hl = (HediffLeaderAura)hediff;
                hl.leader = pawn;
                currentPawn.health.AddHediff(hl);
            }
        }
    }
}