using UnityEngine;

[System.Serializable]
public class LevelConfig
{
    public enum ShapeType { Grid, Pyramid, Cross, Diamond }

    public int Level;
    public ShapeType Shape;
    public int Layers;
    public int SizeParam;


    public static LevelConfig ForLevel(int level)
    {
        level = Mathf.Max(1, level);

        var cfg = new LevelConfig { Level = level };


        float t = Mathf.Clamp01((level - 1) / 49f);
        int raw = Mathf.RoundToInt(Mathf.Lerp(4f, 12f, t));
        cfg.SizeParam = Mathf.Max(4, (raw / 2) * 2);


        if (level <= 5) cfg.Shape = ShapeType.Grid;
        else if (level <= 15) cfg.Shape = ShapeType.Pyramid;
        else if (level <= 25) cfg.Shape = ShapeType.Cross;
        else cfg.Shape = ShapeType.Diamond;


        if (level <= 4) cfg.Layers = 1;
        else if (level <= 9) cfg.Layers = 2;
        else if (level <= 17) cfg.Layers = 3;
        else if (level <= 26) cfg.Layers = 4;
        else cfg.Layers = 5;

        return cfg;
    }


    public int ApproxTileCount
    {
        get
        {
            switch (Shape)
            {
                case ShapeType.Grid:
                    return Mathf.Min(144, SizeParam * 2 * SizeParam);
                case ShapeType.Pyramid:
                    {
                        int total = 0;
                        for (int z = 0; z < Layers; z++)
                        {
                            int side = SizeParam - z * 2;
                            if (side < 2) break;
                            total += side * side;
                        }
                        return Mathf.Min(144, total);
                    }
                default:
                    return Mathf.Min(144, SizeParam * SizeParam * Layers);
            }
        }
    }

    public override string ToString() =>
        $"Seviye {Level} | {Shape} | {Layers} Katman | Boyut {SizeParam} (~{ApproxTileCount} karo)";
}
