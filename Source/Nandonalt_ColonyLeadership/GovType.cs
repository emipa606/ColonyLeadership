namespace Nandonalt_ColonyLeadership;

public class GovType
{
    public string desc;
    public string name;
    public string nameFemale;
    public string nameMale;


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