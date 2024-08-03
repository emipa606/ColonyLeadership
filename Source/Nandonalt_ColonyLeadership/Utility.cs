using Verse;

namespace Nandonalt_ColonyLeadership;

internal class Utility
{
    public static bool isDictatorship
    {
        get
        {
            var comp = getCLComp();
            return comp != null && comp.chosenLeadership.name == "Dictatorship".Translate();
        }
    }


    public static bool isDemocracy
    {
        get
        {
            var comp = getCLComp();
            return comp != null && comp.chosenLeadership.name == "Democracy".Translate();
        }
    }


    public static bool isMonarchy
    {
        get
        {
            var comp = getCLComp();
            return comp != null && comp.chosenLeadership.name == "Monarchy".Translate();
        }
    }

    public static GameComponent_ColonyLeadership getCLComp()
    {
        var comp = Current.Game.GetComponent<GameComponent_ColonyLeadership>();
        return comp;
    }

    public static GovType getGov()
    {
        var comp = getCLComp();
        return comp?.chosenLeadership;
    }
}