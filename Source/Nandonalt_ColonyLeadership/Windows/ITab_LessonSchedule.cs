using System.Collections.Generic;
using Nandonalt_ColonyLeadership.Config;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Nandonalt_ColonyLeadership;

public class ITab_LessonSchedule : ITab
{
    private static readonly MessageTypeDef nullSound = new MessageTypeDef();

    public static Vector2 CardSize = new Vector2(350f, 350f);
    private static float hourWidth;

    public ITab_LessonSchedule()
    {
        size = CardSize;
        labelKey = "Schedule";
        tutorTag = "Schedule";
    }

    private Building_TeachingSpot Spot => (Building_TeachingSpot)SelThing;

    protected override void FillTab()
    {
        var rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(5f);
        DrawCard(rect, Spot);
    }

    public static void DrawCard(Rect rect, Building_TeachingSpot spot)
    {
        //GUI.BeginGroup(rect);
        //Rect position = new Rect(0f, 0f, rect.width, 65f);
        var unused = rect;
        hourWidth = 20.833334f;
        var num = 15f;
        var num2 = 250f;
        Text.Font = GameFont.Medium;
        var title = new Rect(rect.x + 5, rect.y + 5, rect.width, rect.height);
        Widgets.Label(title, "LessonSchedule".Translate());

        Text.Font = GameFont.Small;

        var text = new Rect(rect.x + 5, rect.y + 40, 300f, 240f);
        Widgets.Label(text, "ScheduleText".Translate());


        var seasonLabel = new Rect(rect.x + 110f, rect.y + 5 + 230f, 150f, 200f);
        Widgets.Label(seasonLabel, "SeasonDays".Translate());

        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.LowerLeft;


        for (var i = 1; i <= 15; i++)
        {
            var rect2 = new Rect(num + 4f, num2 + 0f, hourWidth, 30f);
            Widgets.Label(rect2, i.ToString());
            var rect3 = new Rect(num, num2 + 30f, hourWidth, 30f);
            DoTimeAssignment(rect3, i - 1, spot);
            num += hourWidth;
        }

        Text.Anchor = TextAnchor.UpperLeft;
        var dist = 25f;


        var button = new Rect(rect.x + dist, rect.y + 175, 140f, 30f);
        var name = spot.teachers[0] == null ? "NoneL" : spot.teachers[0].Name.ToStringShort;
        if (Widgets.ButtonText(button, "RedTeacher".Translate() + name, true, false))
        {
            listPawns(0, spot);
        }

        var button2 = new Rect(rect.x + dist + 150f, rect.y + 175, 140f, 30f);
        var name2 = spot.teachers[1] == null ? "NoneL" : spot.teachers[1].Name.ToStringShort;
        if (Widgets.ButtonText(button2, "BlueTeacher".Translate() + name2, true, false))
        {
            listPawns(1, spot);
        }

        var button3 = new Rect(rect.x + dist, rect.y + 135f, 140f, 30f);
        var hour = $"{spot.lessonHour}:00h";
        if (Widgets.ButtonText(button3, "LessonStart".Translate() + hour, true, false))
        {
            listHours(spot);
        }

        var button4 = new Rect(rect.x + dist + 150f, rect.y + 135f, 140f, 30f);
        if (Widgets.ButtonText(button4, "IgnoreList".Translate(), true, false))
        {
            Find.WindowStack.Add(new Dialog_ChooseIgnored(spot));
        }


        // GUI.EndGroup();
    }


    private static void DoTimeAssignment(Rect rect, int day, Building_TeachingSpot spot)
    {
        rect = rect.ContractedBy(1f);
        var texture = TimeAssignmentDefOf.Anything.ColorTexture;
        switch (spot.seasonSchedule[day])
        {
            case 1:
                texture = ModTextures.RedColor;
                break;
            case 2:
                texture = ModTextures.BlueColor;
                break;
            case 3:
                texture = ModTextures.YellowColor;
                break;
        }

        GUI.DrawTexture(rect, texture);
        if (!Mouse.IsOver(rect))
        {
            return;
        }

        Widgets.DrawBox(rect, 2);
        //if (Input.GetMouseButton(0))
        if (!Widgets.ButtonInvisible(rect))
        {
            return;
        }

        spot.seasonSchedule[day] = (spot.seasonSchedule[day] % 4) + 1;
        SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera();
        //p.timetable.SetAssignment(hour, this.selectedAssignment);
    }


    #region list

    public static void listHours(Building_TeachingSpot spot)
    {
        var list = new List<FloatMenuOption>();
        var availableHours = new List<int>(new[]
            { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 });


        foreach (var i in availableHours)
        {
            list.Add(new FloatMenuOption($"{i}:00h", delegate { spot.lessonHour = i; }));
        }


        Find.WindowStack.Add(new FloatMenu(list));
    }

    public static void listPawns(int index, Building_TeachingSpot spot)
    {
        var sciLeaderDeffName = ConfigManager.getScientistLeaderDeffName();
        var list = new List<FloatMenuOption>();
        var tpawns = new List<Pawn>();

        foreach (var current in IncidentWorker_SetLeadership.getAllColonists())
        {
            var h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            var h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            var h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            var h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(sciLeaderDeffName));

            if (h1 == null && h2 == null && h3 == null && h4 == null)
            {
                continue;
            }

            if (!spot.teachers.Contains(current))
            {
                tpawns.Add(current);
            }
        }


        list.Add(new FloatMenuOption("-None-", delegate { spot.teachers[index] = null; }));

        foreach (var p in tpawns)
        {
            list.Add(new FloatMenuOption(p.Name.ToStringShort, delegate
            {
                var hasSkill = TeachingUtility.leaderHasAnySkill(p, out var report, out _);
                if (hasSkill)
                {
                    spot.teachers[index] = p;
                    if (report != "" && Prefs.DevMode)
                    {
                        Messages.Message(report, TargetInfo.Invalid, nullSound);
                    }
                }
                else
                {
                    spot.teachers[index] = null;
                    Messages.Message(report, TargetInfo.Invalid, nullSound);
                }
            }));
        }


        Find.WindowStack.Add(new FloatMenu(list));
    }

    #endregion
}