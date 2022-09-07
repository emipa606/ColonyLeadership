using Verse;

namespace Nandonalt_ColonyLeadership;

public class PawnIgnoreData
{
    public Pawn reference;
    public bool value;

    public PawnIgnoreData(Pawn re, bool val = false)
    {
        value = val;
        reference = re;
    }
}