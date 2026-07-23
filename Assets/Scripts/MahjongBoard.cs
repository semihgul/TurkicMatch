using System.Collections.Generic;
using UnityEngine;

public class MahjongBoard : MonoBehaviour
{
    [Header("Prefabs & Parents")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform boardParent;

    [Header("Tile Spacing Configuration")]
    [SerializeField] private float tileWidthMultiplier = 0.42f;
    [SerializeField] private float tileHeightMultiplier = 0.58f;
    [SerializeField] private float tileDepthOffset = 0.12f;
    [SerializeField] private Vector3 visualShift3D = new Vector3(0.08f, 0.08f, 0f);
    public Vector3 VisualShift3D => visualShift3D;

    [Header("Runes Configuration")]
    [SerializeField] private Sprite[] runeSprites;

    private List<MahjongTile> activeTiles = new List<MahjongTile>();


    private static readonly string[] TileSymbols = new string[36]
    {
        "A", "B", "C", "D", "E", "F", "G", "H", "I",
        "J", "K", "L", "M", "N", "O", "P", "Q", "R",
        "S", "T", "U", "V", "W", "X", "Y", "Z", "1",
        "2", "3", "4", "5", "6", "7", "8", "9", "0"
    };



    public List<MahjongTile> GetActiveTiles() => activeTiles;

    public void ClearBoard()
    {
        foreach (var tile in activeTiles)
        {
            if (tile != null)
            {
                Destroy(tile.gameObject);
            }
        }
        activeTiles.Clear();
    }

    private void Awake()
    {
        EnsureRaycaster();
    }

    private void EnsureRaycaster()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.GetComponent<UnityEngine.EventSystems.Physics2DRaycaster>() == null)
        {
            mainCam.gameObject.AddComponent<UnityEngine.EventSystems.Physics2DRaycaster>();
        }
    }

    public void GenerateBoard(LayoutData.Layout layout, int level)
    {
        EnsureRaycaster();
        ClearBoard();

        List<Vector3Int> positions = layout.Positions;
        int totalTiles = positions.Count;

        if (totalTiles % 4 != 0)
        {
            Debug.LogWarning($"Layout tile count ({totalTiles}) is not a multiple of 4. Adjusting grid...");
            // Trim to nearest multiple of 4
            int trimCount = totalTiles % 4;
            positions = positions.GetRange(0, totalTiles - trimCount);
            totalTiles = positions.Count;
        }


        int numUniqueTypes = Mathf.Min(36, totalTiles / 4);
        List<int> selectedTypes = new List<int>();

        List<int> availableRunes = new List<int>();
        for (int i = 0; i < 18; i++) availableRunes.Add(i);

        List<int> availableMotifs = new List<int>();
        for (int i = 18; i < 36; i++) availableMotifs.Add(i);

        if (level <= 20)
        {
            List<int> pool = new List<int>();
            while (pool.Count < numUniqueTypes)
            {
                List<int> temp = new List<int>(availableRunes);
                ShuffleList(temp);
                pool.AddRange(temp);
            }
            selectedTypes = pool.GetRange(0, numUniqueTypes);
        }
        else if (level <= 40)
        {
            List<int> combined = new List<int>();
            combined.AddRange(availableRunes);
            combined.AddRange(availableMotifs);

            List<int> pool = new List<int>();
            while (pool.Count < numUniqueTypes)
            {
                List<int> temp = new List<int>(combined);
                ShuffleList(temp);
                pool.AddRange(temp);
            }
            selectedTypes = pool.GetRange(0, numUniqueTypes);
        }
        else
        {
            List<int> pool = new List<int>();
            List<int> tempMotifs = new List<int>(availableMotifs);
            ShuffleList(tempMotifs);
            pool.AddRange(tempMotifs);

            if (pool.Count < numUniqueTypes)
            {
                List<int> tempRunes = new List<int>(availableRunes);
                ShuffleList(tempRunes);
                pool.AddRange(tempRunes);
            }
            selectedTypes = pool.GetRange(0, numUniqueTypes);
        }

        List<int> typePool = new List<int>();
        for (int i = 0; i < totalTiles / 4; i++)
        {
            int typeId = selectedTypes[i];
            typePool.Add(typeId);
            typePool.Add(typeId);
            typePool.Add(typeId);
            typePool.Add(typeId);
        }

        // Shuffle the type pool
        ShuffleList(typePool);

        // Spawn tiles
        for (int i = 0; i < totalTiles; i++)
        {
            Vector3Int gridPos = positions[i];
            int typeId = typePool[i];


            Vector3 layerShift = visualShift3D * gridPos.z;
            Vector3 worldPos = new Vector3(
                gridPos.x * tileWidthMultiplier + layerShift.x,
                gridPos.y * tileHeightMultiplier + layerShift.y,
                -gridPos.z * tileDepthOffset
            );

            GameObject tileObj = Instantiate(tilePrefab, boardParent);
            tileObj.transform.localPosition = worldPos;


            int baseSortingOrder = (gridPos.z * 200) + (100 - gridPos.y) * 2;
            foreach (var sr in tileObj.GetComponentsInChildren<SpriteRenderer>())
                sr.sortingOrder = baseSortingOrder + 1;

            MahjongTile tileScript = tileObj.GetComponent<MahjongTile>();


            if (tileScript != null)
            {
                SpriteRenderer symRenderer = tileScript.SymbolRenderer;
                if (symRenderer != null)
                {
                    symRenderer.sortingOrder = baseSortingOrder + 2;
                }
            }


            TMPro.TextMeshPro textMesh = tileObj.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.sortingOrder = baseSortingOrder + 2;
            }


            string symbol = TileSymbols[typeId];
            Color symbolColor = Color.white;
            Sprite runeSprite = null;
            if (runeSprites != null && typeId < runeSprites.Length)
            {
                runeSprite = runeSprites[typeId];
            }

            if (tileScript != null)
            {
                tileScript.Initialize(gridPos, typeId, runeSprite, symbol, symbolColor, this);
                activeTiles.Add(tileScript);
            }
        }

        UpdateTileStates();

        CenterCamera(positions);
    }

    public void UpdateTileStates()
    {

        foreach (var tile in activeTiles)
        {
            if (tile == null) continue;

            bool blockedAbove = false;
            bool blockedLeft = false;
            bool blockedRight = false;

            Vector3Int p = tile.GridPosition;


            foreach (var other in activeTiles)
            {
                if (other == null || other == tile) continue;
                Vector3Int op = other.GridPosition;


                if (op.z == p.z + 1)
                {
                    if (Mathf.Abs(op.x - p.x) < 2 && Mathf.Abs(op.y - p.y) < 2)
                    {
                        blockedAbove = true;
                    }
                }


                if (op.z == p.z && Mathf.Abs(op.y - p.y) < 2)
                {
                    if (op.x >= p.x - 2 && op.x < p.x)
                    {
                        blockedLeft = true;
                    }
                    if (op.x <= p.x + 2 && op.x > p.x)
                    {
                        blockedRight = true;
                    }
                }
            }


            bool isFree = !blockedAbove && (!blockedLeft || !blockedRight);
            tile.SetFree(isFree);
        }
    }

    public bool HasAvailableMatches()
    {

        List<MahjongTile> freeTiles = GetFreeTiles();

        for (int i = 0; i < freeTiles.Count; i++)
        {
            for (int j = i + 1; j < freeTiles.Count; j++)
            {
                if (freeTiles[i].TileTypeId == freeTiles[j].TileTypeId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public List<MahjongTile> GetFreeTiles()
    {
        List<MahjongTile> freeList = new List<MahjongTile>();
        foreach (var tile in activeTiles)
        {
            if (tile != null && tile.IsFree)
            {
                freeList.Add(tile);
            }
        }
        return freeList;
    }

    public KeyValuePair<MahjongTile, MahjongTile>? FindHint()
    {
        List<MahjongTile> freeTiles = GetFreeTiles();
        for (int i = 0; i < freeTiles.Count; i++)
        {
            for (int j = i + 1; j < freeTiles.Count; j++)
            {
                if (freeTiles[i].TileTypeId == freeTiles[j].TileTypeId)
                {
                    return new KeyValuePair<MahjongTile, MahjongTile>(freeTiles[i], freeTiles[j]);
                }
            }
        }
        return null;
    }

    public void ShuffleBoard()
    {
        if (activeTiles.Count == 0) return;


        List<int> currentTypes = new List<int>();
        foreach (var tile in activeTiles)
        {
            if (tile != null)
            {
                currentTypes.Add(tile.TileTypeId);
            }
        }


        int shuffleAttempts = 0;
        bool hasMatches = false;

        while (!hasMatches && shuffleAttempts < 10)
        {
            ShuffleList(currentTypes);

            for (int i = 0; i < activeTiles.Count; i++)
            {
                int newTypeId = currentTypes[i];
                string symbol = TileSymbols[newTypeId];
                Color symbolColor = Color.white;
                Sprite runeSprite = null;
                if (runeSprites != null && newTypeId < runeSprites.Length)
                {
                    runeSprite = runeSprites[newTypeId];
                }


                activeTiles[i].Initialize(activeTiles[i].GridPosition, newTypeId, runeSprite, symbol, symbolColor, this);
            }

            UpdateTileStates();
            hasMatches = HasAvailableMatches();
            shuffleAttempts++;
        }
    }

    public void RemoveTile(MahjongTile tile)
    {
        activeTiles.Remove(tile);
        UpdateTileStates();
    }

    public void AddTile(MahjongTile tile)
    {
        activeTiles.Add(tile);
        UpdateTileStates();
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    private void CenterCamera(List<Vector3Int> positions)
    {
        if (positions.Count == 0) return;


        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var pos in positions)
        {
            float worldX = pos.x * tileWidthMultiplier;
            float worldY = pos.y * tileHeightMultiplier;

            if (worldX < minX) minX = worldX;
            if (worldX > maxX) maxX = worldX;
            if (worldY < minY) minY = worldY;
            if (worldY > maxY) maxY = worldY;
        }

        Vector3 boardCenter = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, -10f);
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = boardCenter;
            // camera size
            float height = maxY - minY + 3f;
            float width = (maxX - minX + 3f) / mainCam.aspect;
            mainCam.orthographic = true;
            mainCam.orthographicSize = Mathf.Max(height * 0.6f, width * 0.6f, 4f);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Load Runes from Sprites Folder")]
    private void LoadRunesFromSpritesFolder()
    {
        string path = "Assets/Sprites/runes.png";
        UnityEngine.Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
        if (assets == null || assets.Length <= 1)
        {
            Debug.LogError($"Could not find sliced sprites at {path}. Make sure runes.png is sliced in Unity!");
            return;
        }

        List<Sprite> sprites = new List<Sprite>();
        foreach (var asset in assets)
        {
            if (asset is Sprite sprite)
            {
                sprites.Add(sprite);
            }
        }


        sprites.Sort((a, b) => CompareNatural(a.name, b.name));

        runeSprites = sprites.ToArray();
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log($"Loaded {runeSprites.Length} rune sprites into the board config!");
    }


    private int CompareNatural(string str1, string str2)
    {
        return System.Text.RegularExpressions.Regex.Replace(str1, @"\d+", m => m.Value.PadLeft(10, '0'))
              .CompareTo(System.Text.RegularExpressions.Regex.Replace(str2, @"\d+", m => m.Value.PadLeft(10, '0')));
    }
#endif
}
