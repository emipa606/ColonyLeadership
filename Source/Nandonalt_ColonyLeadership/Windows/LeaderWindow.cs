using System.Collections.Generic;
using System.Text;
using Nandonalt_ColonyLeadership.Config;
using RimWorld;
using UnityEngine;
using Verse;

namespace Nandonalt_ColonyLeadership;

public class LeaderWindow : MainTabWindow
{
    protected readonly List<Pawn> pawns = [];
    private bool pawnListDirty;


    private Vector2 scrollPosition = Vector2.zero;

    private float scrollViewHeight;

    public override Vector2 RequestedTabSize => Prefs.DevMode ? new Vector2(700f, 300f) : new Vector2(500f, 250f);

    public override void PreOpen()
    {
        base.PreOpen();
        BuildPawnList();
    }

    public static List<Pawn> getAllColonists()
    {
        var pawns = new List<Pawn>();
        pawns.AddRange(PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists);
        return pawns;
    }

    public static void purgeLeadership(Pawn current)
    {
        _ = ConfigManager.getScientistLeaderDeffName();
        var h1 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
        var h2 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
        var h3 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
        //HediffLeader h4 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));
        var h4 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(
            HediffDef.Named("leader4")); //Sci leader classic
        var h5 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(
            HediffDef.Named("leader5")); //Sci leader no psycic

        var h6 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leaderExpired"));

        if (h1 != null)
        {
            current.health.RemoveHediff(h1);
        }

        if (h2 != null)
        {
            current.health.RemoveHediff(h2);
        }

        if (h3 != null)
        {
            current.health.RemoveHediff(h3);
        }

        if (h4 != null)
        {
            current.health.RemoveHediff(h4);
        }

        if (h5 != null)
        {
            current.health.RemoveHediff(h5);
        }

        if (h6 != null)
        {
            current.health.RemoveHediff(h6);
        }

        current.needs.AddOrRemoveNeedsAsAppropriate();
    }

    protected virtual void BuildPawnList()
    {
        pawns.Clear();
        pawns.AddRange(getAllColonists());
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

        pawnListDirty = false;
    }

    public void Notify_PawnsChanged()
    {
        pawnListDirty = true;
    }

    private Dialog_MessageBox getHelpWindow()
    {
        return new Dialog_MessageBox(ColonyLeadership.helpNotes, "Done");
    }

    public override void DoWindowContents(Rect fillRect)
    {
        if (Prefs.DevMode) //Used to accomodate RequestedTabSize (Not sure if this is needed yet).
        {
            fillRect.width += 200f;
            fillRect.height += 50f;
        }

        BuildPawnList();
        var position = new Rect(0f, 0f, fillRect.width, fillRect.height);
        GUI.BeginGroup(position);
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        var outRect = new Rect(0f, 50f, position.width, position.height - 50f);
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(5f, 5f, 140f, 30f), "Leaders");
        Text.Font = GameFont.Small;
        var questionMarkBtn = new Rect(fillRect.width - 50f, 5f, 40f, 40f);
        if (Widgets.ButtonText(questionMarkBtn, "?", true, false))
        {
            Find.WindowStack.Add(getHelpWindow());
        }

        if (Prefs.DevMode)
        {
            var button = new Rect(90f, 5f, 200f, 40f);
            if (Widgets.ButtonText(button, "DEV: Reset Leadership Type", true, false))
            {
                Find.WindowStack.Add(new Dialog_ChooseRules());
            }
        }


        var button3 = new Rect(300f, 5f, 150f, 40f);
        var stg = "DEV: Add Leader";
        if (Utility.isDictatorship)
        {
            stg = "SetDictator".Translate();
        }

        if ((Prefs.DevMode || Utility.isDictatorship && pawns.Count <= 0) &&
            Widgets.ButtonText(button3, stg, true, false))
        {
            Find.WindowStack.Add(new Dialog_ChooseLeader());
        }

        var button2 = new Rect(460f, 5f, 150f, 40f);
        if (Prefs.DevMode && Widgets.ButtonText(button2, "DEV: Purge Leaders", true, false))
        {
            foreach (var p in getAllColonists())
            {
                purgeLeadership(p);
                BuildPawnList();
            }
        }

        if (pawns.NullOrEmpty())
        {
            if (Utility.isDemocracy)
            {
                Widgets.Label(new Rect(5f, 50f, 200f, 200f), "BuildBallotBox".Translate());
            }
        }

        var rect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);
        Widgets.BeginScrollView(outRect, ref scrollPosition, rect);
        var num = 0f;
        foreach (var current in pawns)
        {
            GUI.color = new Color(1f, 1f, 1f, 0.2f);
            Widgets.DrawLineHorizontal(0f, num, rect.width);
            GUI.color = Color.white;

            num += DrawLeaderRow(current, num, rect);
        }

        if (Event.current.type == EventType.Layout)
        {
            scrollViewHeight = num;
        }

        Widgets.EndScrollView();
        GUI.EndGroup();
    }

    public static void changeLeaderType(string currentType, string newType)
    {
        foreach (var p in getAllColonists())
        {
            var type = leaderType(p);
            if (type != currentType)
            {
                continue;
            }

            var h1 = (HediffLeader)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(currentType));
            if (h1 != null)
            {
                p.health.RemoveHediff(h1);
            }

            p.needs.AddOrRemoveNeedsAsAppropriate();

            var hediff = HediffMaker.MakeHediff(HediffDef.Named(newType), p);
            IncidentWorker_SetLeadership.doElect(p, hediff, true);
        }
    }

    private static string leaderType(Pawn current)
    {
        var h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
        if (h1 != null)
        {
            return "leader1";
        }

        h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
        if (h1 != null)
        {
            return "leader2";
        }

        h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
        if (h1 != null)
        {
            return "leader3";
        }

        h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
        if (h1 != null)
        {
            return "leader4";
        }

        //Scientist without psycic sensitivity
        h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader5"));
        return h1 != null
            ? "leader5"
            : "Error?";
    }

    private string leaderLabel(Pawn current)
    {
        var h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
        if (h1 != null)
        {
            return h1.Label;
        }

        h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
        if (h1 != null)
        {
            return h1.Label;
        }

        h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
        if (h1 != null)
        {
            return h1.Label;
        }

        h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
        if (h1 != null)
        {
            return h1.Label;
        }

        //Scientist without psycic sensitivity
        h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader5"));
        return h1 != null ? h1.Label : "Error?";
    }

    private float DrawLeaderRow(Pawn leader, float rowY, Rect fillRect)
    {
        var rect = new Rect(40f, rowY, 300f, 80f);
        var need = (Need_LeaderLevel)leader.needs.AllNeeds.Find(x =>
            x.def == DefDatabase<NeedDef>.GetNamed("LeaderLevel"));
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("");
        var text = stringBuilder.ToString();
        var width = fillRect.width - rect.xMax;
        var num = Text.CalcHeight(text, width);
        var num2 = Mathf.Max(80f, num);
        var position = new Rect(8f, rowY + 12f, 30f, 30f);
        var rect2 = new Rect(0f, rowY, fillRect.width, num2);
        if (Mouse.IsOver(rect2))
        {
            var stringBuilder2 = new StringBuilder();
            stringBuilder2.AppendLine("AverageOpinionAbout".Translate() + need.opinion);


            switch (need.opinion)
            {
                case < -60 when !Utility.isDictatorship:
                    stringBuilder2.AppendLine("UnpopularLeader".Translate());
                    break;
                case < -20:
                    stringBuilder2.AppendLine("UnlikedLeader".Translate());
                    break;
            }

            TooltipHandler.TipRegion(rect2, stringBuilder2.ToString());
            GUI.DrawTexture(rect2, TexUI.HighlightTex);
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    CameraJumper.TryJumpAndSelect(leader);
                }
            }
        }

        Text.Font = GameFont.Medium;

        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.ThingIcon(position, leader);

        var label = $"{leader.Name.ToStringFull}\n   {leaderLabel(leader)}\n";
        if (need.opinion < -20)
        {
            GUI.color = Color.yellow;
        }

        if (need.opinion < -60)
        {
            GUI.color = Color.red;
        }

        Widgets.Label(rect, label);
        GUI.color = Color.white;
        return num2;
    }
}