using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Nandonalt_ColonyLeadership;

public class PlaceWorker_BallotBox : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        var allBuildingsColonist = map.listerThings.AllThings;
        foreach (var building in allBuildingsColonist)
        {
            if (building.def.defName is "BallotBox" or "BallotBox_Blueprint")
            {
                return new AcceptanceReport("OnlyOnePerColony".Translate(building.def.LabelCap));
            }
        }

        return true;
    }

    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var ourMap = Find.CurrentMap;
        GenDraw.DrawFieldEdges(WatchBuildingUtility.CalculateWatchCells(def, center, rot, ourMap).ToList());
    }
}