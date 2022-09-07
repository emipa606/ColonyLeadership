namespace Nandonalt_ColonyLeadership.Governments;

internal class Democracy : GovType
{
    public Democracy(string name, string desc, string nameMale, string nameFemale = "") : base(name, desc, nameMale,
        nameFemale)
    {
    }
}