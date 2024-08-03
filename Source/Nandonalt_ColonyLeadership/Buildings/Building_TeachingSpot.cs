using System.Collections.Generic;
using Nandonalt_ColonyLeadership.Config;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace Nandonalt_ColonyLeadership;

public class Building_TeachingSpot : Building
{
    private readonly MessageTypeDef nullSound = new MessageTypeDef();
    public bool destroyedFlag;
    public List<Pawn> ignored = [];
    public int lastLessonTick = -999999;
    public int lastTryTick = -999999;
    public int lessonHour = 15;
    public List<int> seasonSchedule = [..new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }];
    public Pawn teacher;
    public List<Pawn> teachers = [..new Pawn[] { null, null }];
    public Pawn tempTeacher;

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        // block further ticker work
        destroyedFlag = true;

        base.Destroy(mode);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref seasonSchedule, "seasonSchedule", LookMode.Value, false);
        Scribe_Collections.Look(ref teachers, "teachers", LookMode.Reference, false);
        Scribe_Collections.Look(ref ignored, "ignored", LookMode.Reference, false);

        Scribe_Values.Look(ref lessonHour, "lessonHour", 15);
        Scribe_Values.Look(ref lastLessonTick, "lastLessonTick", -999999);
        Scribe_Values.Look(ref lastTryTick, "lastTryTick", -999999);

        Scribe_References.Look(ref teacher, "teacher");
        Scribe_References.Look(ref tempTeacher, "tempTeacher");

        Scribe_Values.Look(ref currentState, "currentState");
        Scribe_Values.Look(ref currentLessonState, "currentLessonState");
    }

    public override void TickRare()
    {
        if (destroyedFlag) // Do nothing further, when destroyed (just a safety)
        {
            return;
        }


        if (!Spawned)
        {
            return;
        }

        // Don't forget the base work
        base.TickRare();


        if (currentState != State.lesson)
        {
            if (seasonSchedule[GenLocalDate.DayOfSeason(Map)] != 0 &&
                seasonSchedule[GenLocalDate.DayOfSeason(Map)] != 4)
            {
                if (GenLocalDate.HourOfDay(Map) == lessonHour)
                {
                    TryTimedLesson();
                }
            }
        }
        else
        {
            for (var i = 0; i < 2; i++)
            {
                if (teachers[i] == null)
                {
                    continue;
                }

                var noFreeTeachers = !isLeader(teachers[i]) || !teachers[i].IsColonistPlayerControlled ||
                                     teachers[i].Dead;

                if (noFreeTeachers)
                {
                    teachers[i] = null;
                }
            }
        }

        LessonRareTick();
    }


    public override void Tick()
    {
        if (destroyedFlag) // Do nothing further, when destroyed (just a safety)
        {
            return;
        }

        if (!Spawned)
        {
            return;
        }

        base.Tick();
        LessonTick();
    }


    public void LessonTick()
    {
        if (currentState != State.lesson)
        {
            return;
        }

        switch (currentLessonState)
        {
            case LessonState.started:
            case LessonState.gathering:
                if (!TeachingUtility.IsActorAvailable(teacher))
                {
                    return;
                }

                if (teacher.CurJob.def.defName == "TeachLesson")
                {
                    return;
                }

                TeachingUtility.AbortLesson(this, "TeacherUnavailable".Translate());
                return;

            case LessonState.finishing:
                if (!TeachingUtility.IsActorAvailable(teacher))
                {
                    TeachingUtility.AbortLesson(this, "TeacherUnavailable".Translate());
                }

                return;
            case LessonState.finished:
            case LessonState.off:
                currentState = State.notinuse;
                return;
        }
    }

    public bool isLeader(Pawn current)
    {
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        var h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
        var h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
        var h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
        var h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));
        return h1 != null || h2 != null || h3 != null || h4 != null;
    }

    public void LessonRareTick()
    {
        if (currentState != State.lesson)
        {
            return;
        }

        switch (currentLessonState)
        {
            case LessonState.started:
            case LessonState.gathering:
            case LessonState.teaching:
                if (!TeachingUtility.IsActorAvailable(teacher))
                {
                    TeachingUtility.AbortLesson(this, "TeacherUnavailable".Translate());
                    return;
                }

                if (teacher.CurJob.def.defName != "TeachLesson")
                {
                    TeachingUtility.AbortLesson(this, "TeacherUnavailable".Translate());
                    return;
                }

                TeachingUtility.GetLessonGroup(this, Map);

                return;
            case LessonState.finishing:
                if (!TeachingUtility.IsActorAvailable(teacher))
                {
                    TeachingUtility.AbortLesson(this, "TeacherUnavailable".Translate());
                    return;
                }

                /*if (this.teacher.CurJob.def != CultDefOfs.ReflectOnWorship)
                        return;*/
                TeachingUtility.GetLessonGroup(this, Map);

                return;
            case LessonState.finished:
            case LessonState.off:
                currentState = State.notinuse;
                return;
        }
    }


    private void TryTimedLesson()
    {
        var teacherInt = seasonSchedule[GenLocalDate.DayOfSeason(Map)];
        tempTeacher = teacherInt >= 3 ? TeachingUtility.DetermineTeacher(Map) : teachers[teacherInt - 1];

        TryLesson();
    }

    private void TryLesson(bool forced = false)
    {
        if (!CanGatherToLessonNow(out var skills))
        {
            return;
        }

        switch (currentLessonState)
        {
            case LessonState.finished:
            case LessonState.off:

                StartLesson(forced, skills);
                return;

            case LessonState.started:
            case LessonState.gathering:
            case LessonState.finishing:

//                        Messages.Message("A leader has started a lesson on the teaching spot.", TargetInfo.Invalid, nullSound);

                Messages.Message("LeaderHasStartedLesson".Translate(), TargetInfo.Invalid,
                    MessageTypeDefOf.RejectInput);

                return;
        }
    }

    public void StartLesson(bool forced = false, string skills = "")
    {
        teacher = tempTeacher;


        if (Destroyed || !Spawned)
        {
            TeachingUtility.AbortLesson(null, "SpotIsUnavailable".Translate());
            return;
        }

        if (!TeachingUtility.IsActorAvailable(teacher))
        {
            TeachingUtility.AbortLesson(this, "TeacherUnavailableNamed".Translate(teacher.LabelShort));
            teacher = null;
            return;
        }


        //FactionBase = (FactionBase)this.Map.info.parent;

        //Messages.Message("LessonGathering".Translate(new object[] { factionBase.label, teacher.LabelShort }) + skills, TargetInfo.Invalid, nullSound);

        var factionBase = (Settlement)Map.info.parent;

        Messages.Message("LessonGathering".Translate(factionBase.Label, teacher.LabelShort) + skills,
            TargetInfo.Invalid, MessageTypeDefOf.NeutralEvent);

        ChangeState(State.lesson, LessonState.started);
        //this.currentState = State.started;
        var job = new Job(DefDatabase<JobDef>.GetNamed("TeachLesson"), this);
        teacher.jobs.jobQueue.EnqueueLast(job);
        teacher.jobs.EndCurrentJob(JobCondition.InterruptForced);
        TeachingUtility.GetLessonGroup(this, Map, forced);
        lastLessonTick = Find.TickManager.TicksGame;
        //  HediffLeader hediff = TeachingUtility.leaderH(teacher);
        //  if (hediff != null) hediff.lastLessonTick = Find.TickManager.TicksGame;
        var comp = Utility.getCLComp();
        if (comp != null)
        {
            comp.lastLessonTick = Find.TickManager.TicksGame;
        }
    }


    private bool CanGatherToLessonNow(out string skills)
    {
        skills = "";
        if (Find.TickManager.TicksGame < lastTryTick + (GenDate.TicksPerHour / 3))
        {
            return false;
        }

        lastTryTick = Find.TickManager.TicksGame;
        if (Find.TickManager.TicksGame < lastLessonTick + GenDate.TicksPerDay - 1000)
        {
            return RejectMessage("MustWaitForLesson".Translate());
        }

        var comp = Utility.getCLComp();
        if (comp != null && Find.TickManager.TicksGame < comp.lastLessonTick + GenDate.TicksPerDay - 1000)
        {
            return RejectMessage("MustWaitForLesson".Translate());
        }

        if (tempTeacher == null)
        {
            return RejectMessage("NoTeacherSelected".Translate());
        }

        var hasSkill = TeachingUtility.leaderHasAnySkill(tempTeacher, out var report, out skills);
        switch (hasSkill)
        {
            case true when report != "":
            {
                //                Messages.Message(report, TargetInfo.Invalid, nullSound);

                if (Prefs.DevMode)
                {
                    Messages.Message(report, TargetInfo.Invalid, MessageTypeDefOf.NeutralEvent);
                }

                break;
            }
            case false:
                return RejectMessage(report);
        }

        if (tempTeacher.Drafted)
        {
            return RejectMessage("TeacherDrafted".Translate());
        }

        if (tempTeacher.Dead || tempTeacher.Downed)
        {
            return RejectMessage("TeacherDownedDead".Translate());
        }

        return tempTeacher.CanReserve(this) || RejectMessage("TeacherTableReserved".Translate());
    }


    private bool RejectMessage(string s)
    {
        Messages.Message(s, TargetInfo.Invalid, nullSound);

        Messages.Message(s, TargetInfo.Invalid, MessageTypeDefOf.RejectInput);

        return false;
    }

    #region states

    public enum State
    {
        notinuse = 0,
        lesson
    }

    public State currentState = State.notinuse;

    public enum LessonState
    {
        off = 0,
        started,
        gathering,
        teaching,
        finishing,
        finished
    }

    public LessonState currentLessonState = LessonState.off;

    public void ChangeState(State type)
    {
        if (type == State.notinuse)
        {
            currentState = type;
            currentLessonState = LessonState.off;
        }
        else
        {
            Log.Error("Changed default state of Sacrificial Altar this should never happen.");
        }
    }

    public void ChangeState(State type, LessonState worshipState)
    {
        currentState = type;
        currentLessonState = worshipState;
    }

    #endregion
}