using System.Collections.Generic;
using Nandonalt_ColonyLeadership.Config;
using UnityEngine;
using Verse;

namespace Nandonalt_ColonyLeadership;

public class Dialog_ChooseRules : Window
{
    public readonly List<string> electFrequencyOptions = ["3", "2", "1"];
    public GovType chosenLeadership;
    public string electionFrequency;

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

    public override Vector2 InitialSize => new(360f, 360f);


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

            Find.WindowStack.Add(new FloatMenu(list, "GovernmentTypes".Translate()));
        }

        listing_Standard.Gap(24f);
        listing_Standard.Label("ElectionsPerSeason".Translate());

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
                    }, MenuOptionPriority.Default,
                    delegate { TooltipHandler.TipRegion(inRect, "PerSeason".Translate()); }));
            }

            Find.WindowStack.Add(new FloatMenu(electFreqList, "ElectionFrequency".Translate()));
        }


        listing_Standard.Gap(24f);
        listing_Standard.Label("ScientistPsychicSensitivity".Translate());
        var isScientistSensitiveToPsychic = ConfigManager.isPsychicSensitivityForScientist();
        var sensitivityLabel = isScientistSensitiveToPsychic ? "ON".Translate() : "OFF".Translate();

        const string classicSciLeader = "leader4";
        const string noSensitivitySciLeader = "leader5";


        if (listing_Standard.ButtonText(sensitivityLabel))
        {
            var sensitivityOptions = new List<FloatMenuOption>
            {
                new("ON".Translate(), delegate
                    {
                        ConfigManager.setPsychicSensitivityForScientist(true);
                        LeaderWindow.changeLeaderType(noSensitivitySciLeader, classicSciLeader);
                    }, MenuOptionPriority.Default,
                    delegate { TooltipHandler.TipRegion(inRect, "BuffSettingText".Translate()); }),
                new("OFF".Translate(), delegate
                    {
                        ConfigManager.setPsychicSensitivityForScientist(false);
                        LeaderWindow.changeLeaderType(classicSciLeader, noSensitivitySciLeader);
                    }, MenuOptionPriority.Default,
                    delegate { TooltipHandler.TipRegion(inRect, "BuffSettingText".Translate()); })
            };

            Find.WindowStack.Add(new FloatMenu(sensitivityOptions, "ScientistLeaderPsychicSensitivity".Translate()));
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
            if (chosenLeadership.name == "Dictatorship".Translate() || chosenLeadership.name == "Monarchy".Translate())
            {
                foreach (var p in IncidentWorker_SetLeadership.getAllColonists())
                {
                    LeaderWindow.purgeLeadership(p);
                }

                Find.WindowStack.Add(new Dialog_ChooseLeader());
            }
            else
            {
                _ = getElectionDays(freq);
                ConfigManager.setElectionsDays(getElectionDays(freq));
            }

            updateLabel = false;
        }

        listing_Standard.End();
    }

    private List<int> getElectionDays(int freq)
    {
        var allowedDays = new List<int> { 0 };

        switch (freq)
        {
            case 2:
                allowedDays.Add(10);
                break;
            case 3:
                allowedDays.Add(6);
                allowedDays.Add(14);
                break;
        }

        return allowedDays;
    }
}