using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nandonalt_ColonyLeadership.Config;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace Nandonalt_ColonyLeadership;

internal class TeachingUtility
{
    public static readonly float learningFactor = 0.28f;
    public static int remainingDuration = 1100; // 15-second timer
    public static readonly int ritualDuration = 1100; // 15 seconds max
    public static int reflectDuration = 600; // 10 seconds max
    public static readonly int minSkill = 8;


    public static string getLeaderType(Pawn current)
    {
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();

        var h1 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
        var h2 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
        var h3 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
        var h4 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));
        var s = "leader1";

        if (h2 != null)
        {
            s = "leader2";
        }

        if (h3 != null)
        {
            s = "leader3";
        }

        if (h4 != null)
        {
            s = sciLeaderDeffName;
        }

        return s;
    }

    public static Pawn DetermineTeacher(Map map)
    {
        //Pawn result = null;
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        var pawns = IncidentWorker_SetLeadership.getAllColonists();
        var tpawns = new List<Pawn>();
        foreach (var current in pawns)
        {
            var h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            var h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            var h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            var h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));

            if (h1 == null && h2 == null && h3 == null && h4 == null)
            {
                continue;
            }

            if (current.Map == map)
            {
                tpawns.Add(current);
            }
        }


        return tpawns.RandomElement();
    }

    public static HediffLeader leaderH(Pawn current)
    {
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        var h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
        if (h1 != null)
        {
            return (HediffLeader)h1;
        }

        h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
        if (h1 != null)
        {
            return (HediffLeader)h1;
        }

        h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
        if (h1 != null)
        {
            return (HediffLeader)h1;
        }

        h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));
        return (HediffLeader)h1;
    }


    public static void AbortLesson(Building_TeachingSpot spot)
    {
        spot?.ChangeState(Building_TeachingSpot.State.notinuse);
    }

    public static void AbortLesson(Building_TeachingSpot spot, string reason)
    {
        spot?.ChangeState(Building_TeachingSpot.State.notinuse);

        Messages.Message($"{reason} Aborting lesson.", MessageTypeDefOf.NegativeEvent);
    }

    public static bool IsActorAvailable(Pawn actor, bool downedAllowed = false)
    {
        var s = new StringBuilder();
        s.AppendLine("ActorAvailable Checks Initiated");
        if (actor == null)
        {
            return false;
        }

        s.AppendLine("ActorAvailable: Passed null Check");
        if (actor.Dead)
        {
            return false;
        }

        s.AppendLine("ActorAvailable: Passed not-dead");
        if (actor.health == null)
        {
            return false;
        }

        s.AppendLine("ActorAvailable: Passed health check");
        if (actor.health.capacities == null)
        {
            return false;
        }

        s.AppendLine("ActorAvailable: Passed capacities check");
        if (actor.Downed && !downedAllowed)
        {
            return false;
        }

        s.AppendLine($"ActorAvailable: Passed downed check & downedAllowed = {downedAllowed}");
        if (actor.Drafted)
        {
            return false;
        }

        s.AppendLine("ActorAvailable: Passed drafted check");
        if (actor.InAggroMentalState)
        {
            return false;
        }

        s.AppendLine("ActorAvailable: Passed drafted check");
        if (actor.InMentalState)
        {
            return false;
        }

        s.AppendLine("ActorAvailable: Passed InMentalState check");
        s.AppendLine("ActorAvailable Checks Passed");
        return true;
    }


    public static bool ShouldAttendLesson(Pawn p, Building_TeachingSpot spot)
    {
        if (IsActorAvailable(spot.teacher))
        {
            return p != spot.teacher && p.IsColonistPlayerControlled;
        }

        AbortLesson(spot);
        return false;

        //Everyone get over here!
    }

    public static void GetLessonGroup(Building_TeachingSpot spot, Map map, bool forced = false)
    {
        var room = spot.GetRoom();

        if (room.Role == RoomRoleDefOf.PrisonBarracks || room.Role == RoomRoleDefOf.PrisonCell)
        {
            return;
        }

        List<Pawn> listeners;
        if (forced)
        {
            listeners = map.mapPawns.AllPawnsSpawned.ToList().FindAll(x =>
                x.RaceProps.intelligence == Intelligence.Humanlike && !x.Downed && !x.Dead &&
                x.CurJob.def.defName != "AttendLesson" &&
                x.CurJob.def != JobDefOf.ExtinguishSelf &&
                x.CurJob.def != JobDefOf.Rescue &&
                x.CurJob.def != JobDefOf.TendPatient &&
                x.CurJob.def != JobDefOf.BeatFire &&
                !spot.ignored.Contains(x) &&
                x.CurJob.def != JobDefOf.FleeAndCower &&
                (x.GetCaravan() == null || x.GetCaravan().IsPlayerControlled) &&
                !x.InAggroMentalState && !x.InMentalState);
        }
        else
        {
            listeners = map.mapPawns.AllPawnsSpawned.ToList().FindAll(x =>
                x.RaceProps.intelligence == Intelligence.Humanlike && !x.Downed && !x.Dead &&
                x.CurJob.def.defName != "AttendLesson" &&
                x.CurJob.def != JobDefOf.ExtinguishSelf &&
                x.CurJob.def != JobDefOf.Rescue &&
                x.CurJob.def != JobDefOf.TendPatient &&
                x.CurJob.def != JobDefOf.BeatFire &&
                x.CurJob.def != JobDefOf.Lovin &&
                x.CurJob.def != JobDefOf.LayDown &&
                !spot.ignored.Contains(x) &&
                (x.GetCaravan() == null || x.GetCaravan().IsPlayerControlled) &&
                x.CurJob.def != JobDefOf.FleeAndCower &&
                !x.InAggroMentalState && !x.InMentalState);
        }

        for (var i = 0; i < listeners.Count; i++)
        {
            if ((new bool[listeners.Count])[i] || !ShouldAttendLesson(listeners[i], spot))
            {
                continue;
            }

            GiveAttendLessonJob(spot, listeners[i]);
            (new bool[listeners.Count])[i] = true;
        }
    }

    public static bool IsTeacher(Pawn p)
    {
        var list = p.Map.listerThings.AllThings.FindAll(s => s.GetType() == typeof(Building_TeachingSpot));
        foreach (var thing in list)
        {
            var b = (Building_TeachingSpot)thing;
            if (b.teacher == p)
            {
                return true;
            }
        }

        return false;
    }

    public static void GiveAttendLessonJob(Building_TeachingSpot spot, Pawn attendee)
    {
        if (IsTeacher(attendee))
        {
            return;
        }

        if (attendee.Drafted)
        {
            return;
        }

        if (attendee.IsPrisoner)
        {
            return;
        }

        if (attendee.jobs.curJob.def.defName == "AttendLesson")
        {
            return;
        }

        if (!WatchBuildingUtility.TryFindBestWatchCell(spot, attendee, true, out var result, out var chair))
        {
            if (!WatchBuildingUtility.TryFindBestWatchCell(spot, attendee, false, out result, out chair))
            {
                return;
            }
        }

        var dir = spot.Rotation.Opposite.AsInt;

        if (chair != null)
        {
            var newPos = chair.Position + GenAdj.CardinalDirections[dir];

            var J = new Job(DefDatabase<JobDef>.GetNamed("AttendLesson"), spot, newPos, chair)
            {
                playerForced = true,
                ignoreJoyTimeAssignment = true,
                expiryInterval = 9999,
                ignoreDesignations = true,
                ignoreForbidden = true
            };
            attendee.jobs.jobQueue.EnqueueLast(J);
            attendee.jobs.EndCurrentJob(JobCondition.Incompletable);
        }
        else
        {
            var newPos = result + GenAdj.CardinalDirections[dir];

            var J = new Job(DefDatabase<JobDef>.GetNamed("AttendLesson"), spot, newPos, result)
            {
                playerForced = true,
                ignoreJoyTimeAssignment = true,
                expiryInterval = 9999,
                ignoreDesignations = true,
                ignoreForbidden = true
            };
            attendee.jobs.jobQueue.EnqueueLast(J);
            attendee.jobs.EndCurrentJob(JobCondition.Incompletable);
        }
    }

    public static void TeachingComplete(Pawn teacher, Building_TeachingSpot spot)
    {
        spot.ChangeState(Building_TeachingSpot.State.lesson, Building_TeachingSpot.LessonState.finishing);

        spot.ChangeState(Building_TeachingSpot.State.lesson, Building_TeachingSpot.LessonState.finished);
        //altar.currentState = Building_SacrificialAltar.State.finished;

        _ = (Settlement)spot.Map.info.parent;

        Messages.Message("The lesson has finished.", TargetInfo.Invalid, MessageTypeDefOf.PositiveEvent);
    }

    public static bool leaderHasAnySkill(Pawn teacher, out string report, out string skills)
    {
        var leaderType = getLeaderType(teacher);
        var teachable = new List<SkillDef>();
        var missing = "";
        skills = "";
        var reachedLimit = false;
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        switch (leaderType)
        {
            case "leader1":
            {
                if (teacher.skills.GetSkill(SkillDefOf.Plants).Level >= minSkill)
                {
                    teachable.Add(SkillDefOf.Plants);
                }

                if (teacher.skills.GetSkill(SkillDefOf.Medicine).Level >= minSkill)
                {
                    teachable.Add(SkillDefOf.Medicine);
                }

                if (teacher.skills.GetSkill(SkillDefOf.Animals).Level >= minSkill)
                {
                    teachable.Add(SkillDefOf.Animals);
                }

                if (teachable.Count < 3)
                {
                    reachedLimit = true;
                }

                missing = "SkillSet1";
                break;
            }
            case "leader2":
            {
                if (teacher.skills.GetSkill(SkillDefOf.Shooting).Level >= minSkill)
                {
                    teachable.Add(SkillDefOf.Shooting);
                }

                if (teacher.skills.GetSkill(SkillDefOf.Melee).Level >= minSkill)
                {
                    teachable.Add(SkillDefOf.Melee);
                }

                if (teachable.Count < 2)
                {
                    reachedLimit = true;
                }

                missing = "SkillSet2";
                break;
            }
            case "leader3":
            {
                if (teacher.skills.GetSkill(SkillDefOf.Construction).Level >= minSkill)
                {
                    teachable.Add(SkillDefOf.Construction);
                }

                if (teacher.skills.GetSkill(SkillDefOf.Crafting).Level >= minSkill)
                {
                    teachable.Add(SkillDefOf.Crafting);
                }

                if (teacher.skills.GetSkill(SkillDefOf.Artistic).Level >= minSkill)
                {
                    teachable.Add(SkillDefOf.Artistic);
                }

                if (teachable.Count < 3)
                {
                    reachedLimit = true;
                }

                missing = "SkillSet3";
                break;
            }
            default:
            {
                if (leaderType == sciLeaderDeffName)
                {
                    if (teacher.skills.GetSkill(SkillDefOf.Intellectual).Level >= minSkill)
                    {
                        teachable.Add(SkillDefOf.Intellectual);
                    }

                    if (teachable.Count < 1)
                    {
                        reachedLimit = true;
                    }

                    missing = "SkillSet4";
                }

                break;
            }
        }

        if (teachable.NullOrEmpty())
        {
            report = $"MustHaveSkill{minSkill} {missing}";
            return false;
        }

        if (reachedLimit)
        {
            report = $"OnlyTeachIfSkill{minSkill} {missing}";
            return true;
        }

        report = "";
        skills = missing.ReplaceFirst(" or ", " and ");
        return true;
    }
}