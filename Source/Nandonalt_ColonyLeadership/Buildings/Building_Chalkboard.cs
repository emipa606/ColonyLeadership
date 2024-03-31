using Verse;

namespace Nandonalt_ColonyLeadership;

public class Building_Chalkboard : Building
{
    public int frame = 0;

    public int state;

    public override Graphic Graphic
    {
        get
        {
            if (state == 0 || frame < 0)
            {
                return ModTextures.CH_Empty.GetColoredVersion(ShaderDatabase.CutoutComplex, DrawColor, DrawColorTwo);
            }

            switch (state)
            {
                case 1 when frame == 0:
                    return ModTextures.CH_Botanist1.GetColoredVersion(ShaderDatabase.CutoutComplex, DrawColor,
                        DrawColorTwo);
                case 1 when frame == 1:
                    return ModTextures.CH_Botanist2.GetColoredVersion(ShaderDatabase.CutoutComplex, DrawColor,
                        DrawColorTwo);
                case 1 when frame >= 2:
                    return ModTextures.CH_Botanist.GetColoredVersion(ShaderDatabase.CutoutComplex, DrawColor,
                        DrawColorTwo);
                case 2 when frame == 0:
                    return ModTextures.CH_Warrior1.GetColoredVersion(ShaderDatabase.CutoutComplex, DrawColor,
                        DrawColorTwo);
                case 2 when frame == 1:
                    return ModTextures.CH_Warrior2.GetColoredVersion(ShaderDatabase.CutoutComplex, DrawColor,
                        DrawColorTwo);
                case 2 when frame >= 2:
                    return ModTextures.CH_Warrior.GetColoredVersion(ShaderDatabase.CutoutComplex, DrawColor,
                        DrawColorTwo);
                case 3 when frame == 0:
                    return ModTextures.CH_Carpenter1.GetColoredVersion(ShaderDatabase.CutoutComplex, DrawColor,
                        DrawColorTwo);
                case 3 when frame == 1:
                    return ModTextures.CH_Carpenter2.GetColoredVersion(ShaderDatabase.CutoutComplex, DrawColor,
                        DrawColorTwo);
                case 3 when frame >= 2:
                    return ModTextures.CH_Carpenter.GetColoredVersion(ShaderDatabase.CutoutComplex, DrawColor,
                        DrawColorTwo);
                case 4 when frame == 0:
                    return ModTextures.CH_Scientist1.GetColoredVersion(ShaderDatabase.CutoutComplex, DrawColor,
                        DrawColorTwo);
                case 4 when frame == 1:
                    return ModTextures.CH_Scientist2.GetColoredVersion(ShaderDatabase.CutoutComplex, DrawColor,
                        DrawColorTwo);
                case 4 when frame >= 2:
                    return ModTextures.CH_Scientist.GetColoredVersion(ShaderDatabase.CutoutComplex, DrawColor,
                        DrawColorTwo);
                default:
                    return def.graphicData.GraphicColoredFor(this);
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref state, "state");
        Scribe_Values.Look(ref state, "frame");
    }
}