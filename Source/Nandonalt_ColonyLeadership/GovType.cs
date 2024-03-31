namespace Nandonalt_ColonyLeadership;

public class GovType
{
    public readonly string desc;
    public readonly string name;
    public readonly string nameFemale;
    public readonly string nameMale;


    public GovType(string name, string desc, string nameMale, string nameFemale = "")
    {
        this.name = name;
        this.desc = desc;
        this.nameMale = nameMale;
        if (nameFemale == "")
        {
            this.nameFemale = nameMale;
        }
    }
}