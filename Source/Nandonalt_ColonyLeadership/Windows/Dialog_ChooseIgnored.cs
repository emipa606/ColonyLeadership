using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Logger = Nandonalt_ColonyLeadership.Util.Logger;

namespace Nandonalt_ColonyLeadership;

public class Dialog_ChooseIgnored : Window
{
    public readonly List<Pawn> ignoreTemp;
    private readonly Building_TeachingSpot spot;
    public readonly List<PawnIgnoreData> tempPawnList = [];
    protected string curName;
    public List<bool> ignoreTempValues;
    public int MaxSize;
    public string MaxSizebuf;
    public int MinSize;
    public string MinSizebuf;
    public bool permanent;


    private Vector2 scrollPosition = Vector2.zero;
    private float scrollViewHeight;

    public Dialog_ChooseIgnored(Building_TeachingSpot spot)
    {
        forcePause = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = true;
        this.spot = spot;
        ignoreTemp = IncidentWorker_SetLeadership.getAllColonists();


        foreach (var p in ignoreTemp)
        {
            tempPawnList.Add(this.spot.ignored.Contains(p) ? new PawnIgnoreData(p, true) : new PawnIgnoreData(p));
        }
    }


    public override Vector2 InitialSize =>
        // return new Vector2(280, 130 + (tempPawnList.Count * 25)); //Old setting before the switch to a scroll view.
        new(280, 260);

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            Event.current.Use();
        }

        var position = new Rect(0f, 0f, inRect.width, inRect.height);
        GUI.BeginGroup(position);
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        Widgets.Label(new Rect(5f, 5f, 140f, 30f), "ChooseIgnorePawns".Translate());

        var outRect =
            new Rect(0f, 50f, position.width,
                position.height - 85f); // Leaves 50 spacing at  the top, leaves 35 spacing at the bottom.


        var rect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);
        Widgets.BeginScrollView(outRect, ref scrollPosition, rect);


        var num = 0f;
        foreach (var temp in tempPawnList)
        {
            var p = temp.reference;
            if (p.Dead)
            {
                continue;
            }

            GUI.color = new Color(1f, 1f, 1f, 0.2f);
            Widgets.DrawLineHorizontal(0f, num, rect.width);
            GUI.color = Color.white;
            num += DrawIgnorePawnRow(p, num, rect, ref temp.value);
        }

        if (Event.current.type == EventType.Layout)
        {
            scrollViewHeight = num;
        }

        Widgets.EndScrollView();
        var endRect = new Rect(0f, position.height - 30f, position.width, 25f);
        if (Widgets.ButtonText(endRect, "OK".Translate()))
        {
            Logger.log("ChooseIgnorePawnsOK".Translate());
            updatePawnIgnoreData();
            Find.WindowStack.TryRemove(this);
        }

        GUI.EndGroup();
    }


    private void updatePawnIgnoreData()
    {
        foreach (var temp in tempPawnList)
        {
            var p = temp.reference;
            if (spot.ignored.Contains(p))
            {
                if (temp.value)
                {
                    continue;
                }

                Logger.log("RemoveFromIgnore".Translate(p.Name.ToStringFull));
                spot.ignored.Remove(p);
            }
            else
            {
                if (!temp.value)
                {
                    continue;
                }

                Logger.log("AddingToIgnore".Translate(p.Name.ToStringFull));
                spot.ignored.Add(p);
            }
        }
    }

    private float DrawIgnorePawnRow(Pawn ignorePawn, float rowY, Rect fillRect, ref bool toIgnore)
    {
        var rect = new Rect(40f, rowY, 300f, 80f);
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("");
        var text = stringBuilder.ToString();
        var width = fillRect.width - rect.xMax;
        var num = Text.CalcHeight(text, width);
        var num2 = Mathf.Max(80f, num);
        _ = new Rect(8f, rowY + 12f, 30f, 30f);
        var rect2 = new Rect(0f, rowY, fillRect.width, num2);
        var isMouseOver = false;
        if (Widgets.CheckboxLabeledSelectable(rect2, ignorePawn.Name.ToString(), ref isMouseOver, ref toIgnore))
        {
            Logger.log("PawnNameDebug".Translate(ignorePawn.Name.ToStringFull, toIgnore.ToString()));

            toIgnore = !toIgnore;
        }

        Text.Font = GameFont.Medium;

        Text.Anchor = TextAnchor.UpperLeft;
        var label = string.Concat();

        Widgets.Label(rect, label);
        GUI.color = Color.white;
        return num2;
    }
}