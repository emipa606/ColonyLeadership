using Verse;

namespace Nandonalt_ColonyLeadership;

public class GameComponent_ColonyLeadership : GameComponent
{
    public int chosenGov;
    public GovType chosenLeadership;
    public int electionFrequency = 3;

    public int lastLessonTick = -999999;

    public GameComponent_ColonyLeadership(Game game)
    {
    }

    public GameComponent_ColonyLeadership()
    {
    }

    public GameComponent_ColonyLeadership(Map map)
    {
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref chosenGov, "chosenGov");
        chosenLeadership = ColonyLeadership.govtypes[chosenGov];
        Scribe_Values.Look(ref lastLessonTick, "lastLessonTick", -999999);
        base.ExposeData();
    }

    public override void StartedNewGame()
    {
        checkGovs();
    }

    public override void LoadedGame()
    {
        checkGovs();
    }

    public void checkGovs()
    {
        if (chosenLeadership == null || chosenLeadership.name == "" && !Find.WindowStack.WindowsForcePause)
        {
            if (ColonyLeadership.tempGov != null && Find.GameInfo == ColonyLeadership.gameInfoTemp)
            {
                chosenLeadership = ColonyLeadership.tempGov;
                chosenGov = ColonyLeadership.govtypes.IndexOf(ColonyLeadership.tempGov);
            }
            else
            {
                Find.WindowStack.Add(new Dialog_ChooseRules());
            }
        }

        ColonyLeadership.tempGov = chosenLeadership;
        ColonyLeadership.gameInfoTemp = Find.GameInfo;
    }
}