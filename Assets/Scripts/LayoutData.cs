using System.Collections.Generic;
using UnityEngine;

public static class LayoutData
{
    public class Layout
    {
        public string Name;
        public List<Vector3Int> Positions;

        public Layout(string name, List<Vector3Int> positions)
        {
            Name = name;
            Positions = positions;
        }
    }

    public static List<Layout> GetAvailableLayouts()
    {
        return new List<Layout>
        {
            GetSimpleArenaLayout(),
            GetPyramidLayout(),
            GetTurtleLayout()
        };
    }


    public static Layout GetSimpleArenaLayout()
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        int z = 0;

        for (int y = -3; y <= 3; y += 2)
        {
            for (int x = -7; x <= 7; x += 2)
            {
                positions.Add(new Vector3Int(x, y, z));
            }
        }
        return new Layout("Simple Arena", positions);
    }


    public static Layout GetPyramidLayout()
    {
        List<Vector3Int> positions = new List<Vector3Int>();


        for (int y = -5; y <= 5; y += 2)
        {
            for (int x = -7; x <= 7; x += 2)
            {
                positions.Add(new Vector3Int(x, y, 0));
            }
        }


        for (int y = -3; y <= 3; y += 2)
        {
            for (int x = -5; x <= 5; x += 2)
            {
                positions.Add(new Vector3Int(x, y, 1));
            }
        }


        for (int y = -1; y <= 1; y += 2)
        {
            for (int x = -3; x <= 3; x += 2)
            {
                positions.Add(new Vector3Int(x, y, 2));
            }
        }


        return new Layout("Pyramid", positions);
    }


    public static Layout GetTurtleLayout()
    {
        List<Vector3Int> positions = new List<Vector3Int>();




        List<Vector3Int> L0 = new List<Vector3Int>();
        for (int y = -7; y <= 7; y += 2)
        {
            int limit = 0;
            if (y == -7 || y == 7) limit = 5;
            else if (y == -5 || y == 5) limit = 7;
            else if (y == -3 || y == 3) limit = 9;
            else if (y == -1 || y == 1) limit = 11;

            for (int x = -limit; x <= limit; x += 2)
            {
                L0.Add(new Vector3Int(x, y, 0));
            }
        }

        L0.Add(new Vector3Int(-13, 0, 0));
        L0.Add(new Vector3Int(13, 0, 0));


        List<Vector3Int> L1 = new List<Vector3Int>();
        for (int y = -5; y <= 5; y += 2)
        {
            int limit = 0;
            if (y == -5 || y == 5) limit = 5;
            else if (y == -3 || y == 3) limit = 7;
            else if (y == -1 || y == 1) limit = 7;

            for (int x = -limit; x <= limit; x += 2)
            {
                L1.Add(new Vector3Int(x, y, 1));
            }
        }


        List<Vector3Int> L2 = new List<Vector3Int>();
        for (int y = -3; y <= 3; y += 2)
        {
            int limit = (y == -3 || y == 3) ? 3 : 5;
            for (int x = -limit; x <= limit; x += 2)
            {
                L2.Add(new Vector3Int(x, y, 2));
            }
        }


        List<Vector3Int> L3 = new List<Vector3Int>();
        for (int y = -1; y <= 1; y += 2)
        {
            for (int x = -3; x <= 3; x += 2)
            {
                L3.Add(new Vector3Int(x, y, 3));
            }
        }




        positions.AddRange(L0);
        positions.AddRange(L1);
        positions.AddRange(L2);
        positions.AddRange(L3);

        return new Layout("Turtle (144 Tiles)", positions);
    }
}
