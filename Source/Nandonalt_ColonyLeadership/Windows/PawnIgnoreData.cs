using Verse;

namespace Nandonalt_ColonyLeadership;

public class PawnIgnoreData(Pawn re, bool val = false)
{
    public readonly Pawn reference = re;
    public bool value = val;
}