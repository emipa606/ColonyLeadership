using Nandonalt_ColonyLeadership.Config;
using UnityEngine;
using Verse;

namespace Nandonalt_ColonyLeadership;

public class HediffLeaderAura : HediffWithComps
{
    private int counter;
    public Pawn leader;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref leader, "leader");
    }


    public override void Notify_PawnDied()
    {
        pawn.health.RemoveHediff(this);
    }

    public override void Tick()
    {
        base.Tick();

        var unused = pawn;
        counter += 1;
        if (counter < 100)
        {
            return;
        }

        if (leader.Map == null)
        {
            pawn.health.RemoveHediff(this);
        }

        var leaderNearby = false;
        if (leader != null && leader.Map == pawn.Map && pawn.IsColonistPlayerControlled)
        {
            string label;
            var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
            if (def.defName == "leaderAura1")
            {
                label = "leader1";
            }
            else if (def.defName == "leaderAura2")
            {
                label = "leader2";
            }
            else if (def.defName == "leaderAura3")
            {
                label = "leader3";
            }
            else
            {
                label = sciLeaderDeffName;
            }

            /*
                List<Thing> list = GenRadial.RadialDistinctThingsAround(actor.Position, actor.Map, 15f, true).ToList<Thing>();
                foreach (Thing current in list)
                {}*/
            var distance = (pawn.Position - leader.Position).LengthHorizontal;

            if (distance <= 15f)
            {
                if (leader.GetRoom() == pawn.GetRoom())
                {
                    var currentLeader = leader;

                    var hediff =
                        (HediffLeader)currentLeader.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(label));
                    if (hediff != null)
                    {
                        leaderNearby = true;
                        Severity = Mathf.Clamp(hediff.Need.CurLevel, 0.01f, 1f);
                        if (currentLeader.CurJob != null && currentLeader.jobs.curDriver.asleep)
                        {
                            leaderNearby = false;
                        }

                        if (currentLeader.InAggroMentalState)
                        {
                            leaderNearby = false;
                        }
                    }
                }
            }
        }

        if (!leaderNearby)
        {
            pawn.health.RemoveHediff(this);
        }
    }
}