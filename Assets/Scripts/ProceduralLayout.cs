using System.Collections.Generic;
using UnityEngine;


public static class ProceduralLayout
{
    private const int MaxTiles = 144;


    public static LayoutData.Layout Generate(LevelConfig cfg)
    {
        List<Vector3Int> positions;

        switch (cfg.Shape)
        {
            case LevelConfig.ShapeType.Pyramid:
                positions = BuildPyramid(cfg.SizeParam, cfg.Layers);
                break;
            case LevelConfig.ShapeType.Cross:
                positions = BuildCross(cfg.SizeParam, cfg.Layers);
                break;
            case LevelConfig.ShapeType.Diamond:
                positions = BuildDiamond(cfg.SizeParam, cfg.Layers);
                break;
            default:
                positions = BuildGrid(cfg.SizeParam);
                break;
        }


        int count = (Mathf.Min(positions.Count, MaxTiles) / 4) * 4;
        if (count < positions.Count)
            positions = positions.GetRange(0, count);

        return new LayoutData.Layout($"Seviye {cfg.Level}", positions);
    }


    private static List<Vector3Int> BuildGrid(int size)
    {
        int W = size * 2;
        int H = size;

        var positions = new List<Vector3Int>(W * H);
        for (int row = 0; row < H; row++)
            for (int col = 0; col < W; col++)
                positions.Add(MakePos(col, W, row, H, 0));

        return positions;
    }


    private static List<Vector3Int> BuildPyramid(int size, int layers)
    {
        var positions = new List<Vector3Int>();

        for (int z = 0; z < layers; z++)
        {
            int side = size - z * 2;
            if (side < 2) break;

            for (int row = 0; row < side; row++)
                for (int col = 0; col < side; col++)
                    positions.Add(MakePos(col, side, row, side, z));
        }

        return positions;
    }


    private static List<Vector3Int> BuildCross(int size, int layers)
    {
        int baseArm = size;
        int baseWidth = Mathf.Max(2, (size / 4) * 2);

        var positions = new List<Vector3Int>();
        var seen = new HashSet<Vector3Int>();

        for (int z = 0; z < layers; z++)
        {
            int armLen = Mathf.Max(baseWidth, baseArm - z * 2);
            int armWidth = Mathf.Max(2, baseWidth - z);


            for (int x = -armLen; x <= armLen; x += 2)
                for (int y = -armWidth; y <= armWidth; y += 2)
                    TryAdd(positions, seen, new Vector3Int(x, y, z));


            for (int x = -armWidth; x <= armWidth; x += 2)
                for (int y = -armLen; y <= armLen; y += 2)
                    TryAdd(positions, seen, new Vector3Int(x, y, z));
        }

        return positions;
    }


    private static List<Vector3Int> BuildDiamond(int size, int layers)
    {
        int baseRadius = size * 2;

        var positions = new List<Vector3Int>();

        for (int z = 0; z < layers; z++)
        {
            int r = baseRadius - z * 4;
            if (r < 4) break;

            for (int y = -r; y <= r; y += 2)
                for (int x = -r; x <= r; x += 2)
                    if (Mathf.Abs(x) + Mathf.Abs(y) <= r)
                        positions.Add(new Vector3Int(x, y, z));
        }

        return positions;
    }


    private static Vector3Int MakePos(int col, int W, int row, int H, int z)
    {
        return new Vector3Int(
            col * 2 - (W - 1),
            row * 2 - (H - 1),
            z
        );
    }

    private static void TryAdd(List<Vector3Int> list, HashSet<Vector3Int> seen, Vector3Int v)
    {
        if (seen.Add(v)) list.Add(v);
    }
}
