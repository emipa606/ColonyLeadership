using System.Text;
using Nandonalt_ColonyLeadership.Config;
using RimWorld;
using UnityEngine;
using Verse;

namespace Nandonalt_ColonyLeadership;

public class Need_LeaderLevel : Need
{
    public bool chosenToStay;
    private int lastGainTick = -999;
    private int lastLoseTick = -999;

    public float opinion;
    private int ticks;

    public Need_LeaderLevel(Pawn pawn) : base(pawn)
    {
        threshPercents =
        [
            0.33f,
            0.66f
        ];
    }

    public override int GUIChangeArrow
    {
        get
        {
            if (Find.TickManager.TicksGame < lastGainTick + 20)
            {
                return 1;
            }

            if (Find.TickManager.TicksGame < lastLoseTick + 20)
            {
                return -1;
            }

            return 0;
        }
    }

    public override float CurInstantLevel => curLevelInt;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref chosenToStay, "chosenToStay");
    }

    public override void SetInitialLevel()
    {
        CurLevel = Rand.Range(0.5f, 0.8f);
    }


    public float getSkillFactor(Pawn current)
    {
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        var h1 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
        var h2 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
        var h3 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
        var h4 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));
        var score = IncidentWorker_SetLeadership.getBotanistScore(current);

        if (h2 != null)
        {
            h1 = h2;
            score = IncidentWorker_SetLeadership.getWarriorScore(current);
        }

        if (h3 != null)
        {
            h1 = h3;
            score = IncidentWorker_SetLeadership.getCarpenterScore(current);
        }

        if (h4 == null)
        {
            return (score / 20f * 0.3f) + 1f;
        }

        h1 = h4;
        score = IncidentWorker_SetLeadership.getScientistScore(current);

        return (score / 20f * 0.3f) + 1f;
    }


    public override void NeedInterval()
    {
        if (ticks >= 5)
        {
            float totalOpinion = 0;
            var totalPawns = 0;
            foreach (var p in IncidentWorker_SetLeadership.getAllColonists())
            {
                if (p == pawn || p.Dead)
                {
                    continue;
                }

                totalOpinion += p.relations.OpinionOf(pawn);
                totalPawns++;
            }

            var medium = totalOpinion / totalPawns;

            opinion = medium;
            var newlevel = Mathf.Clamp(medium, 0f, 100f) / 100f * getSkillFactor(pawn);


            if (curLevelInt < newlevel)
            {
                lastGainTick = Find.TickManager.TicksGame;
            }
            else if (curLevelInt > newlevel)
            {
                lastLoseTick = Find.TickManager.TicksGame;
            }

            if (newlevel > 1f)
            {
                newlevel = 1f;
            }

            curLevelInt = newlevel;


            if (totalPawns == 0)
            {
                curLevelInt = 0.5f * getSkillFactor(pawn);
            }

            ticks = 0;
            var eventOccurs = false;
            if (!Utility.isDictatorship)
            {
                if (!chosenToStay && opinion is < -40 and > -80)
                {
                    if (Rand.MTBEventOccurs(1.25f, 60000f, 150f))
                    {
                        eventOccurs = true;
                        var str = new StringBuilder();
                        var s = "she";
                        if (pawn.gender == Gender.Male)
                        {
                            s = "he";
                        }

                        var b = "her";
                        if (pawn.gender == Gender.Male)
                        {
                            b = "his";
                        }

                        str.AppendLine("UnpopularM".Translate(pawn.Name.ToStringFull));
                        str.AppendLine("");
                        str.AppendLine("ResignLetterText".Translate(pawn.Name.ToStringFull));
                        str.AppendLine("");
                        str.AppendLine("OnlyOnce".Translate());

                        void RemoveLeader()
                        {
                            Find.LetterStack.ReceiveLetter("LeaderEndLetter".Translate(),
                                "LeaderEndLetterDesc".Translate(pawn.Name.ToStringFull), LetterDefOf.NegativeEvent,
                                pawn);
                            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("LeaderEnd"));
                            IncidentWorker_Rebellion.removeLeadership(pawn);
                        }

                        void KeepLeader()
                        {
                            chosenToStay = true;
                        }


                        var window = new Dialog_MessageBox(str.ToString(), "Resign".Translate(), RemoveLeader,
                            "DontTesign".Translate(), KeepLeader, "ResignationProposal".Translate());
                        Find.WindowStack.Add(window);
                    }
                }

                if (!eventOccurs && opinion < -60)
                {
                    var mtb = 0.50f;
                    if (opinion < -70)
                    {
                        mtb = 0.35f;
                    }

                    if (opinion < -80)
                    {
                        mtb = 0.20f;
                    }

                    if (opinion < -90)
                    {
                        mtb = 0.10f;
                    }

                    if (Rand.MTBEventOccurs(mtb, 60000f, 150f))
                    {
                        var isRebelling = false;
                        foreach (var pa in IncidentWorker_SetLeadership.getAllColonists())
                        {
                            if (pa is { MentalState: not null } &&
                                pa.MentalState.def == DefDatabase<MentalStateDef>.GetNamed("Rebelling"))
                            {
                                isRebelling = true;
                            }
                        }

                        if (!isRebelling)
                        {
                            var parms = StorytellerUtility.DefaultParmsNow(IncidentDef.Named("RebellionL").category,
                                pawn.Map);
                            if (IncidentDef.Named("RebellionL").Worker.TryExecute(parms))
                            {
                                var s = "her";
                                if (pawn.gender == Gender.Male)
                                {
                                    s = "his";
                                }

                                Find.LetterStack.ReceiveLetter("RebellionLetter".Translate(),
                                    "RebellionLetterDesc".Translate(pawn.Name.ToStringFull),
                                    LetterDefOf.NegativeEvent, pawn);
                            }
                        }
                    }
                }
            }
        }

        ticks++;
    }


    public int getLeaderLevel()
    {
        if (curLevelInt <= 0.33f)
        {
            return 1;
        }

        return curLevelInt <= 0.66 ? 2 : 3;
    }


    public override string GetTipString()
    {
        return $"{base.GetTipString()}\n" + "LeaderLevel".Translate() + getLeaderLevel().ToString() + "\n" +
               "AverageOpinion".Translate() + opinion.ToString();
    }

    public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f,
        bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null, bool drawLabel = true)
    {
        if (rect.height > 70f)
        {
            var num = (rect.height - 70f) / 2f;
            rect.height = 70f;
            rect.y += num;
        }

        if (Mouse.IsOver(rect))
        {
            Widgets.DrawHighlight(rect);
        }

        if (doTooltip)
        {
            TooltipHandler.TipRegion(rect, new TipSignal(GetTipString, rect.GetHashCode()));
        }

        var num2 = 14f;
        var num3 = customMargin < 0f ? num2 + 15f : customMargin;
        if (rect.height < 50f)
        {
            num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
        }

        Text.Font = rect.height <= 55f ? GameFont.Tiny : GameFont.Small;
        Text.Anchor = TextAnchor.LowerLeft;
        var rect2 = new Rect(rect.x + num3 + (rect.width * 0.1f), rect.y, rect.width - num3 - (rect.width * 0.1f),
            rect.height / 2f);
        Widgets.Label(rect2, LabelCap);
        Text.Anchor = TextAnchor.UpperLeft;
        var rect3 = new Rect(rect.x, rect.y + (rect.height / 2f), rect.width, rect.height / 2f);
        rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - (num3 * 2f), rect3.height - num2);
        Widgets.FillableBar(rect3, CurLevelPercentage, ModTextures.GreenTex);
        if (drawArrows)
        {
            Widgets.FillableBarChangeArrows(rect3, GUIChangeArrow);
        }

        if (threshPercents != null)
        {
            for (var i = 0; i < Mathf.Min(threshPercents.Count, maxThresholdMarkers); i++)
            {
                DrawBarThreshold(rect3, threshPercents[i]);
            }
        }

        var curInstantLevelPercentage = CurInstantLevelPercentage;
        if (curInstantLevelPercentage >= 0f)
        {
            DrawBarInstantMarkerAt(rect3, curInstantLevelPercentage);
        }

        if (!def.tutorHighlightTag.NullOrEmpty())
        {
            UIHighlighter.HighlightOpportunity(rect, def.tutorHighlightTag);
        }

        Text.Font = GameFont.Small;
    }

    private new void DrawBarThreshold(Rect barRect, float threshPct)
    {
        float num = barRect.width <= 60f ? 1 : 2;
        var position = new Rect(barRect.x + (barRect.width * threshPct) - (num - 1f), barRect.y + (barRect.height / 2f),
            num, barRect.height / 2f);
        Texture2D image;
        if (threshPct < CurLevelPercentage)
        {
            image = BaseContent.BlackTex;
            GUI.color = new Color(1f, 1f, 1f, 0.9f);
        }
        else
        {
            image = BaseContent.GreyTex;
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
        }

        GUI.DrawTexture(position, image);
        GUI.color = Color.white;
    }
}