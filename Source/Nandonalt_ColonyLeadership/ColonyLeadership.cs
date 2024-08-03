using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Nandonalt_ColonyLeadership.Detour;
using RimWorld;
using Verse;

namespace Nandonalt_ColonyLeadership;

[StaticConstructorOnStartup]
public class ColonyLeadership
{
    public static readonly string helpNotes = "GovTypeFaq".Translate();


    public static string lastReadVersion = "none";
    public static readonly string newVersion = "v1.5";
    public static readonly List<GovType> govtypes = [];
    public static GovType tempGov = null;
    public static GameInfo gameInfoTemp = null;
    public static readonly bool useLogging = false;


    static ColonyLeadership()
    {
        try
        {
            Detours.TryDetourFromTo(
                typeof(ColonistBarColonistDrawer).GetMethod("DrawIcons",
                    BindingFlags.NonPublic | BindingFlags.Instance), typeof(Icon).GetMethod("DrawIconsModded"));
        }
        catch (Exception)
        {
            //File.WriteAllText("logt.txt", e.Message.ToString());
        }

        govtypes.Add(new GovType("Democracy", "DemocracyDesc", "Leader"));
        govtypes.Add(new GovType("Dictatorship", "DictatorshipDesc", "Dictator"));
        //govtypes.Add(new GovType("Monarchy", "MonarchyDesc", "Ruler"));
        if (!File.Exists(Path.Combine(GenFilePaths.SaveDataFolderPath, "ColonyLeadershipGlobal.xml")))
        {
            return;
        }

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

    //Uncomment for trace source debugging
    //private static TraceSource _source = new TraceSource("DebugLog");


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