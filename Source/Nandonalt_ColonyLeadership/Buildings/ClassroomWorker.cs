using Verse;

namespace Nandonalt_ColonyLeadership;

public class ClassroomWorker : RoomRoleWorker
{
    public override float GetScore(Room room)
    {
        var num = 0;
        var allContainedThings = room.ContainedAndAdjacentThings;
        foreach (var thing in allContainedThings)
        {
            if (thing is Building_TeachingSpot)
            {
                num += 2;
            }

            if (thing is Building_Chalkboard)
            {
                num++;
            }

            if (thing.def.defName == "GlobeCL")
            {
                num++;
            }
        }

        return 30f * num;
    }
}