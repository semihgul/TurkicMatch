using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SlotManager : MonoBehaviour
{
    [Header("Slot Ayarları")]
    [SerializeField] private int slotCount = 4;
    [SerializeField] private float slotSpacing = 1.2f;
    [SerializeField] private float offsetBelowCameraTop = 0.75f;

    [Header("Görsel")]
    [SerializeField] private Sprite slotBackgroundSprite;
    [SerializeField] private Color slotEmptyColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private Vector2 slotSpriteScale = new Vector2(0.85f, 1.15f);
    [SerializeField] private bool showDebugVisuals = false;
    [SerializeField] private GameObject externalSlotVisuals;

    [Header("Animasyon")]
    [SerializeField] private float moveDuration = 0.22f;
    [SerializeField] private float matchPauseDuration = 0.30f;

    public System.Action OnSlotsFullNoMatch;
    public System.Action<int> OnMatchCleared;


    private MahjongTile[] slots;
    private Vector3[] slotPositions;
    private SpriteRenderer[] slotBgRenderers;
    private bool isProcessing = false;

    public int FilledCount => CountFilled();
    public bool IsFull => CountFilled() >= slotCount;
    public bool IsProcessing => isProcessing;



    private void Awake()
    {
        slots = new MahjongTile[slotCount];
        slotPositions = new Vector3[slotCount];
        BuildSlotVisuals();
    }

    private void BuildSlotVisuals()
    {
        slotBgRenderers = new SpriteRenderer[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            var go = new GameObject($"SlotBG_{i}");
            go.transform.SetParent(transform);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = slotBackgroundSprite;
            sr.color = slotEmptyColor;
            sr.sortingOrder = 1499;
            go.transform.localScale = new Vector3(slotSpriteScale.x, slotSpriteScale.y, 1f);
            sr.enabled = showDebugVisuals;

            slotBgRenderers[i] = sr;
        }
    }


    public void RefreshPositions()
    {
        if (externalSlotVisuals != null)
        {
            externalSlotVisuals.SetActive(true);
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        float y = cam.transform.position.y + cam.orthographicSize - offsetBelowCameraTop;
        float totalWidth = (slotCount - 1) * slotSpacing;
        float startX = cam.transform.position.x - totalWidth * 0.5f;

        for (int i = 0; i < slotCount; i++)
        {
            slotPositions[i] = new Vector3(startX + i * slotSpacing, y, -3f);

            if (slotBgRenderers[i] != null)
                slotBgRenderers[i].transform.position = slotPositions[i];


            if (slots[i] != null)
                slots[i].transform.position = slotPositions[i];
        }
    }


    public bool TryAddTile(MahjongTile tile)
    {
        if (IsFull || isProcessing) return false;


        var target = new List<MahjongTile>(slotCount);
        for (int i = 0; i < slotCount; i++)
            if (slots[i] != null) target.Add(slots[i]);
        target.Add(tile);
        target.Sort((a, b) => a.TileTypeId.CompareTo(b.TileTypeId));

        for (int i = 0; i < slotCount; i++)
            slots[i] = i < target.Count ? target[i] : null;

        isProcessing = true;
        StartCoroutine(AddTileRoutine(tile));
        return true;
    }


    private IEnumerator AddTileRoutine(MahjongTile newTile)
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i] == null) continue;

            if (slots[i] == newTile)
                slots[i].EnterSlot(slotPositions[i], null);
            else
                slots[i].SlideTo(slotPositions[i], null);
        }

        yield return new WaitForSeconds(moveDuration + 0.02f);

        yield return StartCoroutine(MatchRoutine());
    }



    public void ClearAll()
    {
        StopAllCoroutines();
        isProcessing = false;

        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i] != null)
            {
                Destroy(slots[i].gameObject);
                slots[i] = null;
            }
        }

        if (externalSlotVisuals != null)
        {
            externalSlotVisuals.SetActive(false);
        }
    }



    private IEnumerator MatchRoutine()
    {
        bool anyMatch = true;
        while (anyMatch)
        {
            anyMatch = false;

            for (int i = 0; i < slotCount - 1; i++)
            {
                if (slots[i] != null &&
                    slots[i + 1] != null &&
                    slots[i].TileTypeId == slots[i + 1].TileTypeId)
                {

                    MahjongTile t1 = slots[i];
                    MahjongTile t2 = slots[i + 1];

                    slots[i] = null;
                    slots[i + 1] = null;

                    t1.AnimateMatchAndDestroy();
                    t2.AnimateMatchAndDestroy();

                    OnMatchCleared?.Invoke(200);

                    anyMatch = true;
                    yield return new WaitForSeconds(matchPauseDuration);

                    yield return StartCoroutine(CompactAndSlideRoutine());
                    break;
                }
            }
        }

        isProcessing = false;


        if (IsFull)
            OnSlotsFullNoMatch?.Invoke();
    }


    private IEnumerator CompactAndSlideRoutine()
    {
        var filled = new List<MahjongTile>(slotCount);
        for (int i = 0; i < slotCount; i++)
            if (slots[i] != null) filled.Add(slots[i]);

        for (int i = 0; i < slotCount; i++)
            slots[i] = i < filled.Count ? filled[i] : null;

        if (filled.Count == 0) yield break;

        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i] != null)
                slots[i].SlideTo(slotPositions[i], null);
        }

        yield return new WaitForSeconds(moveDuration + 0.02f);
    }



    private int CountFilled()
    {
        int count = 0;
        for (int i = 0; i < slotCount; i++)
            if (slots[i] != null) count++;
        return count;
    }
}
