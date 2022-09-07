using System.Collections.Generic;
using System.Linq;
using Nandonalt_ColonyLeadership.Config;
using UnityEngine;
using Verse;

namespace Nandonalt_ColonyLeadership;

public class Dialog_ChooseLeader : Window
{
    public Pawn chosenPawn;
    protected string curName;
    public int MaxSize;
    public string MaxSizebuf;
    public int MinSize;
    public string MinSizebuf;
    public bool permanent;

    public Dialog_ChooseLeader()
    {
        forcePause = true;
        doCloseX = true;
        absorbInputAroundWindow = false;
        closeOnClickedOutside = true;
        chosenPawn = null;
    }


    public override Vector2 InitialSize => new Vector2(280f, 170f);

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        var pressedReturn = false;

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            pressedReturn = true;
            Event.current.Use();
        }

        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(inRect);
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        listing_Standard.Label("ChooseLeader".Translate());
        var label = chosenPawn == null ? "NoneL" : chosenPawn.Name.ToStringShort;
        if (listing_Standard.ButtonText(label))
        {
            var list = new List<FloatMenuOption>();
            var tpawns2 = new List<Pawn>();

            list.Add(new FloatMenuOption("-" + "NoneL".Translate() + "-", delegate { chosenPawn = null; }));

            foreach (var current in IncidentWorker_SetLeadership.getAllColonists())
            {
                var h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
                var h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
                var h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
                var h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));
                var h5 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leaderExpired"));
                //Hediff h6 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("ruler1"));
                if (h1 == null && h2 == null && h3 == null && h4 == null && h5 == null &&
                    !current.WorkTagIsDisabled(WorkTags.Social))
                {
                    tpawns2.Add(current);
                }
            }

            foreach (var p in tpawns2)
            {
                list.Add(new FloatMenuOption(p.Name.ToStringShort, delegate { chosenPawn = p; }));
            }

            if (list.Count == 1)
            {
                TooltipHandler.TipRegion(inRect, "ChooseLeader_NoAbleColonists".Translate());
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }


        if (listing_Standard.ButtonText("OK".Translate()))
        {
            if (chosenPawn != null)
            {
                var most = chosenPawn;
                if (Utility.getGov().name == "Democracy".Translate() ||
                    Utility.getGov().name == "Dictatorship".Translate())
                {
                    var targetLeader = "";
                    var maxValue = new[]
                    {
                        IncidentWorker_SetLeadership.getBotanistScore(most),
                        IncidentWorker_SetLeadership.getWarriorScore(most),
                        IncidentWorker_SetLeadership.getCarpenterScore(most),
                        IncidentWorker_SetLeadership.getScientistScore(most)
                    }.Max();
                    if (maxValue == IncidentWorker_SetLeadership.getBotanistScore(most))
                    {
                        targetLeader = "leader1";
                    }

                    if (maxValue == IncidentWorker_SetLeadership.getWarriorScore(most))
                    {
                        targetLeader = "leader2";
                    }

                    if (maxValue == IncidentWorker_SetLeadership.getCarpenterScore(most))
                    {
                        targetLeader = "leader3";
                    }

                    if (maxValue == IncidentWorker_SetLeadership.getScientistScore(most))
                    {
                        targetLeader = sciLeaderDeffName;
                    }

                    var hediff = HediffMaker.MakeHediff(HediffDef.Named(targetLeader), most);
                    IncidentWorker_SetLeadership.doElect(most, hediff, true);
                }
            }

            Find.WindowStack.TryRemove(this);
        }

        listing_Standard.End();
    }
}