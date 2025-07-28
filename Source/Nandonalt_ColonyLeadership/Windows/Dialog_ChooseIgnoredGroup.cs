using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Nandonalt_ColonyLeadership;

/**
 * A group of sixteen colonists is collected, and their respective check-mark option is returned to the ignored list.
 */
internal class Dialog_ChooseIgnoredGroup : Window
{
    public readonly List<PawnIgnoreData> group;
    private readonly Building_TeachingSpot spot;

    public Dialog_ChooseIgnoredGroup(List<PawnIgnoreData> group, ref Building_TeachingSpot spot)
    {
        forcePause = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = true;
        this.group = group;
        this.spot = spot;
    }

    public override Vector2 InitialSize
    {
        get
        {
            var maxHeight = 15f * 25f;
            return new Vector2(280f, 130f + maxHeight);
        }
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        var pressedReturn = false;

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            pressedReturn = true;
            Event.current.Use();
        }

        var ignoreList = new Listing_Standard();
        ignoreList.Begin(inRect);
        ignoreList.Label("IgnoreLectures".Translate());
        ignoreList.Gap(5f);

        foreach (var piData in group)
        {
            var p = piData.reference;
            if (!p.Dead)
            {
                ignoreList.CheckboxLabeled(p.LabelShort, ref piData.value);
            }
        }

        if (ignoreList.ButtonText("OK".Translate()) || pressedReturn)
        {
            foreach (var piData in group)
            {
                var p = piData.reference;
                if (spot.ignored.Contains(p))
                {
                    if (!piData.value)
                    {
                        spot.ignored.Remove(p);
                    }
                }
                else
                {
                    if (piData.value)
                    {
                        spot.ignored.Add(p);
                    }
                }
            }

            Find.WindowStack.TryRemove(this);
        }

        ignoreList.Gap(10f);
        ignoreList.End();
    }
}