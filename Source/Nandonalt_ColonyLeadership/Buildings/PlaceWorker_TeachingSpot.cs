using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Nandonalt_ColonyLeadership;

public class PlaceWorker_TeachingSpot : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        var currentMap = Find.CurrentMap;
        var allBuildingsColonist = currentMap.listerThings.AllThings;

        foreach (var t in allBuildingsColonist)
        {
            if (t.def.defName is "TeachingSpot" or "TeachingSpot_Blueprint")
            {
                return new AcceptanceReport("OnlyOnePerColony");
            }
        }

        return true;
    }

    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var currentMap = Find.CurrentMap;
        GenDraw.DrawFieldEdges(WatchBuildingUtility.CalculateWatchCells(def, center, rot, currentMap).ToList());
    }
}