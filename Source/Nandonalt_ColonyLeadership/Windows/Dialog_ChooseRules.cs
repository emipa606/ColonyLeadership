using System.Collections.Generic;
using Nandonalt_ColonyLeadership.Config;
using UnityEngine;
using Verse;

namespace Nandonalt_ColonyLeadership;

public class Dialog_ChooseRules : Window
{
    public GovType chosenLeadership;
    protected string curName;
    public List<string> electFrequencyOptions = new List<string>(new[] { "3", "2", "1" });
    public string electionFrequency;
    public int MaxSize;
    public string MaxSizebuf;
    public int MinSize;
    public string MinSizebuf;
    public bool permanent;

    private bool updateLabel;

    public Dialog_ChooseRules()
    {
        forcePause = true;
        doCloseX = false;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        chosenLeadership = ColonyLeadership.govtypes[0];
        electionFrequency = "3";
    }

    public override Vector2 InitialSize => new Vector2(360f, 360f);


    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;

        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(inRect);

        listing_Standard.Label("ChooseGov".Translate());

        var label = chosenLeadership.name;
        if (listing_Standard.ButtonText(label))
        {
            var list = new List<FloatMenuOption>();

            foreach (var gov in ColonyLeadership.govtypes)
            {
                list.Add(new FloatMenuOption(gov.name, delegate { chosenLeadership = gov; }, MenuOptionPriority.Default,
                    delegate { TooltipHandler.TipRegion(inRect, gov.desc); }));
            }

            Find.WindowStack.Add(new FloatMenu(list, "Government Types"));
        }

        listing_Standard.Gap(24f);
        listing_Standard.Label("Elections per Season");

        if (!updateLabel)
        {
            electionFrequency = ConfigManager.getElectionDays().Count.ToString();
        }

        var freqLabel = electionFrequency;


        if (listing_Standard.ButtonText(freqLabel))
        {
            var electFreqList = new List<FloatMenuOption>();
            foreach (var f in electFrequencyOptions)
            {
                electFreqList.Add(new FloatMenuOption(f, delegate
                {
                    electionFrequency = f;
                    freqLabel = f;
                    updateLabel = true;
                }, MenuOptionPriority.Default, delegate { TooltipHandler.TipRegion(inRect, "PerSeason"); }));
            }

            Find.WindowStack.Add(new FloatMenu(electFreqList, "election frequency"));
        }


        listing_Standard.Gap(24f);
        listing_Standard.Label("Scientist Psychic Sensitivity");
        var isScientistSensitiveToPsychic = ConfigManager.isPsychicSensitivityForScientist();
        var sensitivityLabel = isScientistSensitiveToPsychic ? "ON" : "OFF";

        const string classicSciLeader = "leader4";
        const string noSensitivitySciLeader = "leader5";


        if (listing_Standard.ButtonText(sensitivityLabel))
        {
            var sensitivityOptions = new List<FloatMenuOption>
            {
                new FloatMenuOption("ON", delegate
                    {
                        ConfigManager.setPsychicSensitivityForScientist(true);
                        LeaderWindow.changeLeaderType(noSensitivitySciLeader, classicSciLeader);
                    }, MenuOptionPriority.Default,
                    delegate
                    {
                        TooltipHandler.TipRegion(inRect,
                            "Adds Buffs to medical surgery and negotiation if turned off.");
                    }),
                new FloatMenuOption("OFF", delegate
                    {
                        ConfigManager.setPsychicSensitivityForScientist(false);
                        LeaderWindow.changeLeaderType(classicSciLeader, noSensitivitySciLeader);
                    }, MenuOptionPriority.Default,
                    delegate
                    {
                        TooltipHandler.TipRegion(inRect,
                            "Adds Buffs to medical surgery and negotiation if turned off.");
                    })
            };

            Find.WindowStack.Add(new FloatMenu(sensitivityOptions, "Science Leader Psycic Sensitivty"));
        }


        listing_Standard.Gap(24f);
        if (listing_Standard.ButtonText("OK".Translate()))
        {
            var freq = int.Parse(electionFrequency);
            var comp = Utility.getCLComp();
            if (comp != null)
            {
                comp.chosenLeadership = chosenLeadership;
            }

            if (comp != null)
            {
                comp.chosenGov = ColonyLeadership.govtypes.IndexOf(chosenLeadership);
            }

            Find.WindowStack.TryRemove(this);
            if (chosenLeadership.name == "Dictatorship".Translate() || chosenLeadership.name == "Monarchy")
            {
                foreach (var p in IncidentWorker_SetLeadership.getAllColonists())
                {
                    LeaderWindow.purgeLeadership(p);
                }

                Find.WindowStack.Add(new Dialog_ChooseLeader());
            }
            else
            {
                var newDays = getElectionDays(freq);
                ConfigManager.setElectionsDays(getElectionDays(freq));
            }

            updateLabel = false;
        }

        listing_Standard.End();
    }

    private List<int> getElectionDays(int freq)
    {
        var allowedDays = new List<int> { 0 };

        if (freq == 2)
        {
            allowedDays.Add(10);
        }
        else if (freq == 3)
        {
            allowedDays.Add(6);
            allowedDays.Add(14);
        }

        return allowedDays;
    }
}