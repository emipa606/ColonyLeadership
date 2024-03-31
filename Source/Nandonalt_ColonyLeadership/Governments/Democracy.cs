namespace Nandonalt_ColonyLeadership.Governments;

internal class Democracy(string name, string desc, string nameMale, string nameFemale = "")
    : GovType(name, desc, nameMale,
        nameFemale);