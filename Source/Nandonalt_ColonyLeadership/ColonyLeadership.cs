using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Nandonalt_ColonyLeadership.Detour;
using RimWorld;
using Verse;

namespace Nandonalt_ColonyLeadership;

[StaticConstructorOnStartup]
public class ColonyLeadership
{
    public static string updateNotes = "";
    public static string helpNotes = "";


    public static string lastReadVersion = "none";
    public static string newVersion = "v1.5";
    public static List<GovType> govtypes = new List<GovType>();
    public static GovType tempGov = null;
    public static GameInfo gameInfoTemp = null;
    public static bool useLogging = false;


    static ColonyLeadership()
    {
        try
        {
            Detours.TryDetourFromTo(
                typeof(ColonistBarColonistDrawer).GetMethod("DrawIcons",
                    BindingFlags.NonPublic | BindingFlags.Instance), typeof(Icon).GetMethod("DrawIconsModded"));
        }
        catch (Exception e)
        {
            //File.WriteAllText("logt.txt", e.Message.ToString());
        }

        govtypes.Add(new GovType("Democracy", "DemocracyDesc", "Leader"));
        govtypes.Add(new GovType("Dictatorship", "DictatorshipDesc", "Dictator"));
        //govtypes.Add(new GovType("Monarchy", "MonarchyDesc", "Ruler"));
        if (File.Exists(Path.Combine(GenFilePaths.SaveDataFolderPath, "ColonyLeadershipGlobal.xml")))
        {
            try
            {
                Scribe.loader.InitLoading(Path.Combine(GenFilePaths.SaveDataFolderPath, "ColonyLeadershipGlobal.xml"));
                Scribe_Values.Look(ref lastReadVersion, "lastReadVersion", "none");
                Scribe.loader.FinalizeLoading();


                //PostLoadIniter.DoAllPostLoadInits();
            }
            catch (Exception ex)
            {
                //File.WriteAllText("logt.txt", ex.Message.ToString());
                Log.Error($"Exception loading colony leadership userdata: {ex}");
                Scribe.ForceStop();
            }
        }

        if (lastReadVersion != newVersion)
        {
            DefDatabase<MainButtonDef>.GetNamed("LeaderTab").label = "(!) " + "LeadershipTab";
            doUpdateNotes();
        }

        doHelpNotes();
    }

    //Uncomment for trace source debugging
    //private static TraceSource _source = new TraceSource("DebugLog");


    public static void doUpdateNotes()
    {
        var str = new StringBuilder();
        str.AppendLine("COLONY LEADERSHIP MOD  - VERSION 1.6.2 - UPDATE!");
        str.AppendLine("");
        str.AppendLine("Colony Leadership updated for 1.2 release of rimworld.");
        str.AppendLine("BUG FIXES:");
        str.AppendLine(" - Ignore Pawn List for Lessons now has a scroll bar and is populated correctly.");
        str.AppendLine("1.6 Notes: ");
        str.AppendLine(" - Configurable Election Frequency Added");
        str.AppendLine(" - Once a month: Elections on day 1.");
        str.AppendLine(" - Twice a month: Days 1 and 11");
        str.AppendLine(" - Three times a motnh: Days 1 and 7 and 15");
        str.AppendLine(" - Configurable Scientist Psychic Sensitivity ");
        str.AppendLine(" - Turning psycic sensitivity off gives a small bonus to surgery success and negotiation");
        str.AppendLine("");
        str.AppendLine("Note ** The ballot box still says on days 1, 7 & 15 in it's built form description.");
        str.AppendLine("-> This Update is brought to you by McKay, please report any bugs! ");
        str.AppendLine("-> Please support the original author! Nandonalt - https://www.patreon.com/nandonalt");

        updateNotes = str.ToString();
    }

    public static void doHelpNotes()
    {
        var str = new StringBuilder();

        str.AppendLine("FAQ:");
        str.AppendLine(" - How do I switch government types?");
        str.AppendLine("    -> Enter Development mode, and proceed to the Leadership Menu at the bottom ");
        str.AppendLine("       of the screen. From there, you will see that there are several new buttons");
        str.AppendLine("       prepended with DEV. Simply select the button that says Reset Leadership Type.");
        str.AppendLine("");
        str.AppendLine(" - My Colonists are gathering for elections but they don't actually vote, what's going on?");
        str.AppendLine("    -> Check your job schedule, if a colonist is scheduled to switch from one task to another");
        str.AppendLine(
            "       during elections, they'll leave the election in progress and won't vote. It helps to set");
        str.AppendLine("       their work schedule to Any during elections.");
        str.AppendLine("");
        str.AppendLine(" - How do I change election frequency?");
        str.AppendLine(
            "    -> When you go to set your leadership type, you will have an options menu to select either once, twice, or three times a season to hold elections.");
        str.AppendLine("");
        str.AppendLine(
            "If you have any problems, comment on the steam workshop page and I'll do my best to address them!");
        str.AppendLine(" - McKay");

        helpNotes = str.ToString();
    }

    public static void Save()
    {
        try
        {
            var path = Path.Combine(GenFilePaths.SaveDataFolderPath, "ColonyLeadershipGlobal.xml");
            var label = "ColonyLeadershipGlobal";

            void Action()
            {
                Scribe_Values.Look(ref lastReadVersion, "lastReadVersion", "none");
                //Scribe_Values.LookValue<int>(ref MaxSize, "MaxSize", 90, false);
                //Scribe_Values.LookValue<bool>(ref permanentCamps, "permanentCamps", false, false);
            }

            SafeSaver.Save(path, label, Action);
        }
        catch (Exception ex)
        {
            Log.Error($"Exception while saving colony leadership userdata: {ex}");
        }
    }
}