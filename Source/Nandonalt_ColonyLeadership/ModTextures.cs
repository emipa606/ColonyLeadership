using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Nandonalt_ColonyLeadership;

[StaticConstructorOnStartup]
public class ModTextures
{
    static ModTextures()
    {
        var botanistTextures = 3;
        var warriorTextures = 3;
        var scientistTextures = 3;
        var carpenterTextures = 3;

        for (var i = 1; i <= botanistTextures; i++)
        {
            icons_leader1.Add(ContentFinder<Texture2D>.Get($"ColonyLeadership/TeachingBubbles/botanist{i}"));
        }

        for (var i = 1; i <= warriorTextures; i++)
        {
            icons_leader2.Add(ContentFinder<Texture2D>.Get($"ColonyLeadership/TeachingBubbles/warrior{i}"));
        }

        for (var i = 1; i <= carpenterTextures; i++)
        {
            icons_leader3.Add(ContentFinder<Texture2D>.Get($"ColonyLeadership/TeachingBubbles/carpenter{i}"));
        }

        for (var i = 1; i <= scientistTextures; i++)
        {
            icons_leader4.Add(ContentFinder<Texture2D>.Get($"ColonyLeadership/TeachingBubbles/scientist{i}"));
        }
    }


    #region Textures

    public static readonly Texture2D GreenTex = SolidColorMaterials.NewSolidColorTexture(Color.green);
    public static readonly Texture2D BlueColor = SolidColorMaterials.NewSolidColorTexture(Color.blue);
    public static readonly Texture2D RedColor = SolidColorMaterials.NewSolidColorTexture(Color.red);
    public static readonly Texture2D YellowColor = SolidColorMaterials.NewSolidColorTexture(Color.yellow);


    public static readonly Texture2D carpenter1 =
        ContentFinder<Texture2D>.Get("ColonyLeadership/TeachingBubbles/carpenter1");

    public static readonly Texture2D carpenter2 =
        ContentFinder<Texture2D>.Get("ColonyLeadership/TeachingBubbles/carpenter2");

    public static readonly Texture2D carpenter3 =
        ContentFinder<Texture2D>.Get("ColonyLeadership/TeachingBubbles/carpenter3");

    public static readonly Texture2D botanist1 =
        ContentFinder<Texture2D>.Get("ColonyLeadership/TeachingBubbles/botanist1");

    public static readonly Texture2D botanist2 =
        ContentFinder<Texture2D>.Get("ColonyLeadership/TeachingBubbles/botanist2");

    public static readonly Texture2D botanist3 =
        ContentFinder<Texture2D>.Get("ColonyLeadership/TeachingBubbles/botanist3");

    public static readonly Texture2D warrior1 =
        ContentFinder<Texture2D>.Get("ColonyLeadership/TeachingBubbles/warrior1");

    public static readonly Texture2D warrior2 =
        ContentFinder<Texture2D>.Get("ColonyLeadership/TeachingBubbles/warrior2");

    public static readonly Texture2D warrior3 =
        ContentFinder<Texture2D>.Get("ColonyLeadership/TeachingBubbles/warrior3");

    public static readonly Texture2D scientist1 =
        ContentFinder<Texture2D>.Get("ColonyLeadership/TeachingBubbles/scientist1");

    public static readonly Texture2D scientist2 =
        ContentFinder<Texture2D>.Get("ColonyLeadership/TeachingBubbles/scientist2");

    public static readonly Texture2D scientist3 =
        ContentFinder<Texture2D>.Get("ColonyLeadership/TeachingBubbles/scientist3");

    public static readonly Texture2D waiting = ContentFinder<Texture2D>.Get("ColonyLeadership/TeachingBubbles/waiting");
    public static readonly List<Texture2D> icons_leader1 = [];
    public static readonly List<Texture2D> icons_leader2 = [];
    public static readonly List<Texture2D> icons_leader3 = [];
    public static readonly List<Texture2D> icons_leader4 = [];

    public static readonly Graphic CH_Empty = GraphicDatabase.Get(
        ThingDef.Named("ChalkboardCL").graphicData.graphicClass,
        ThingDef.Named("ChalkboardCL").graphicData.texPath, ThingDef.Named("ChalkboardCL").graphic.Shader,
        ThingDef.Named("ChalkboardCL").graphicData.drawSize, ThingDef.Named("ChalkboardCL").graphicData.color,
        ThingDef.Named("ChalkboardCL").graphicData.colorTwo);

    public static readonly Graphic CH_Botanist = GraphicDatabase.Get(
        ThingDef.Named("ChalkboardCL").graphicData.graphicClass,
        "ColonyLeadership/Chalkboard/ch_botanist", ThingDef.Named("ChalkboardCL").graphic.Shader,
        ThingDef.Named("ChalkboardCL").graphicData.drawSize, ThingDef.Named("ChalkboardCL").graphicData.color,
        ThingDef.Named("ChalkboardCL").graphicData.colorTwo);

    public static readonly Graphic CH_Botanist1 = GraphicDatabase.Get(
        ThingDef.Named("ChalkboardCL").graphicData.graphicClass,
        "ColonyLeadership/Chalkboard/ch_botanist1", ThingDef.Named("ChalkboardCL").graphic.Shader,
        ThingDef.Named("ChalkboardCL").graphicData.drawSize, ThingDef.Named("ChalkboardCL").graphicData.color,
        ThingDef.Named("ChalkboardCL").graphicData.colorTwo);

    public static readonly Graphic CH_Botanist2 = GraphicDatabase.Get(
        ThingDef.Named("ChalkboardCL").graphicData.graphicClass,
        "ColonyLeadership/Chalkboard/ch_botanist2", ThingDef.Named("ChalkboardCL").graphic.Shader,
        ThingDef.Named("ChalkboardCL").graphicData.drawSize, ThingDef.Named("ChalkboardCL").graphicData.color,
        ThingDef.Named("ChalkboardCL").graphicData.colorTwo);

    public static readonly Graphic CH_Warrior = GraphicDatabase.Get(
        ThingDef.Named("ChalkboardCL").graphicData.graphicClass,
        "ColonyLeadership/Chalkboard/ch_warrior", ThingDef.Named("ChalkboardCL").graphic.Shader,
        ThingDef.Named("ChalkboardCL").graphicData.drawSize, ThingDef.Named("ChalkboardCL").graphicData.color,
        ThingDef.Named("ChalkboardCL").graphicData.colorTwo);

    public static readonly Graphic CH_Warrior1 = GraphicDatabase.Get(
        ThingDef.Named("ChalkboardCL").graphicData.graphicClass,
        "ColonyLeadership/Chalkboard/ch_warrior1", ThingDef.Named("ChalkboardCL").graphic.Shader,
        ThingDef.Named("ChalkboardCL").graphicData.drawSize, ThingDef.Named("ChalkboardCL").graphicData.color,
        ThingDef.Named("ChalkboardCL").graphicData.colorTwo);

    public static readonly Graphic CH_Warrior2 = GraphicDatabase.Get(
        ThingDef.Named("ChalkboardCL").graphicData.graphicClass,
        "ColonyLeadership/Chalkboard/ch_warrior2", ThingDef.Named("ChalkboardCL").graphic.Shader,
        ThingDef.Named("ChalkboardCL").graphicData.drawSize, ThingDef.Named("ChalkboardCL").graphicData.color,
        ThingDef.Named("ChalkboardCL").graphicData.colorTwo);

    public static readonly Graphic CH_Carpenter = GraphicDatabase.Get(
        ThingDef.Named("ChalkboardCL").graphicData.graphicClass,
        "ColonyLeadership/Chalkboard/ch_carpenter", ThingDef.Named("ChalkboardCL").graphic.Shader,
        ThingDef.Named("ChalkboardCL").graphicData.drawSize, ThingDef.Named("ChalkboardCL").graphicData.color,
        ThingDef.Named("ChalkboardCL").graphicData.colorTwo);

    public static readonly Graphic CH_Carpenter1 = GraphicDatabase.Get(
        ThingDef.Named("ChalkboardCL").graphicData.graphicClass,
        "ColonyLeadership/Chalkboard/ch_carpenter1", ThingDef.Named("ChalkboardCL").graphic.Shader,
        ThingDef.Named("ChalkboardCL").graphicData.drawSize, ThingDef.Named("ChalkboardCL").graphicData.color,
        ThingDef.Named("ChalkboardCL").graphicData.colorTwo);

    public static readonly Graphic CH_Carpenter2 = GraphicDatabase.Get(
        ThingDef.Named("ChalkboardCL").graphicData.graphicClass,
        "ColonyLeadership/Chalkboard/ch_carpenter2", ThingDef.Named("ChalkboardCL").graphic.Shader,
        ThingDef.Named("ChalkboardCL").graphicData.drawSize, ThingDef.Named("ChalkboardCL").graphicData.color,
        ThingDef.Named("ChalkboardCL").graphicData.colorTwo);

    public static readonly Graphic CH_Scientist = GraphicDatabase.Get(
        ThingDef.Named("ChalkboardCL").graphicData.graphicClass,
        "ColonyLeadership/Chalkboard/ch_scientist", ThingDef.Named("ChalkboardCL").graphic.Shader,
        ThingDef.Named("ChalkboardCL").graphicData.drawSize, ThingDef.Named("ChalkboardCL").graphicData.color,
        ThingDef.Named("ChalkboardCL").graphicData.colorTwo);

    public static readonly Graphic CH_Scientist1 = GraphicDatabase.Get(
        ThingDef.Named("ChalkboardCL").graphicData.graphicClass,
        "ColonyLeadership/Chalkboard/ch_scientist1", ThingDef.Named("ChalkboardCL").graphic.Shader,
        ThingDef.Named("ChalkboardCL").graphicData.drawSize, ThingDef.Named("ChalkboardCL").graphicData.color,
        ThingDef.Named("ChalkboardCL").graphicData.colorTwo);

    public static readonly Graphic CH_Scientist2 = GraphicDatabase.Get(
        ThingDef.Named("ChalkboardCL").graphicData.graphicClass,
        "ColonyLeadership/Chalkboard/ch_scientist2", ThingDef.Named("ChalkboardCL").graphic.Shader,
        ThingDef.Named("ChalkboardCL").graphicData.drawSize, ThingDef.Named("ChalkboardCL").graphicData.color,
        ThingDef.Named("ChalkboardCL").graphicData.colorTwo);

    #endregion textures
}