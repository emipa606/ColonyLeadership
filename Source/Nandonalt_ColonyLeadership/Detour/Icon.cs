using RimWorld;
using UnityEngine;
using Verse;
//using UnityEngine;

namespace Nandonalt_ColonyLeadership;

[StaticConstructorOnStartup]
public class Icon : ColonistBarColonistDrawer
{
    private static readonly Texture2D DeadColonistTex = ContentFinder<Texture2D>.Get("UI/Misc/DeadColonist");

    private static readonly Texture2D Icon_MentalStateNonAggro =
        ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MentalStateNonAggro");

    private static readonly Texture2D Icon_MentalStateAggro =
        ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MentalStateAggro");

    private static readonly Texture2D Icon_MedicalRest =
        ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MedicalRest");

    private static readonly Texture2D Icon_Sleeping = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Sleeping");

    private static readonly Texture2D Icon_Fleeing = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Fleeing");

    private static readonly Texture2D Icon_Attacking = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Attacking");

    private static readonly Texture2D Icon_Idle = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Idle");

    private static readonly Texture2D Icon_Burning = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Burning");

    private static readonly Texture2D Icon_Star = ContentFinder<Texture2D>.Get("ColonyLeadership/star");

    private static readonly Texture2D Icon_Inspired = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Inspired");


    private ColonistBar ColonistBar => Find.ColonistBar;

    public void DrawIconsModded(Rect rect, Pawn colonist)
    {
        if (colonist.Dead)
        {
            return;
        }

        var num = 20f * ColonistBar.Scale;
        var vector = new Vector2(rect.x + 1f, rect.yMax - num - 1f);
        var hasTarget = false;
        if (colonist.CurJob != null)
        {
            var def = colonist.CurJob.def;
            if (def == JobDefOf.AttackMelee || def == JobDefOf.AttackStatic)
            {
                hasTarget = true;
            }
            else if (def == JobDefOf.Wait_Combat)
            {
                if (colonist.stances.curStance is Stance_Busy { focusTarg.IsValid: true })
                {
                    hasTarget = true;
                }
            }
        }

        Hediff Leader = TeachingUtility.leaderH(colonist);
        if (colonist.InAggroMentalState)
        {
            DrawIcon(Icon_MentalStateAggro, ref vector, colonist.MentalStateDef.LabelCap);
        }
        else if (colonist.InMentalState)
        {
            DrawIcon(Icon_MentalStateNonAggro, ref vector, colonist.MentalStateDef.LabelCap);
        }
        else if (colonist.InBed() && colonist.CurrentBed().Medical)
        {
            DrawIcon(Icon_MedicalRest, ref vector, "ActivityIconMedicalRest");
        }
        else if (colonist.CurJob != null && colonist.jobs.curDriver.asleep)
        {
            DrawIcon(Icon_Sleeping, ref vector, "ActivityIconSleeping");
        }
        else if (colonist.CurJob != null && colonist.CurJob.def == JobDefOf.FleeAndCower)
        {
            DrawIcon(Icon_Fleeing, ref vector, "ActivityIconFleeing");
        }
        else if (hasTarget)
        {
            DrawIcon(Icon_Attacking, ref vector, "ActivityIconAttacking");
        }
        else if (colonist.mindState.IsIdle && GenDate.DaysPassed >= 1)
        {
            DrawIcon(Icon_Idle, ref vector, "ActivityIconIdle");
        }
        else if (Leader != null)
        {
            DrawIcon(Icon_Star, ref vector, Leader.LabelBase);
        }

        if (colonist.IsBurning())
        {
            DrawIcon(Icon_Burning, ref vector, "ActivityIconBurning");
        }

        else if (colonist.Inspired)
        {
            DrawIcon(Icon_Inspired, ref vector, colonist.InspirationDef.LabelCap);
        }
    }

    private void DrawIcon(Texture2D icon, ref Vector2 pos, string tooltip)
    {
        var num = 20f * ColonistBar.Scale;
        var rect = new Rect(pos.x, pos.y, num, num);
        GUI.DrawTexture(rect, icon);
        TooltipHandler.TipRegion(rect, tooltip);
        pos.x += num;
    }
}