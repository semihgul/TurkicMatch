using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class MahjongTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI & Visuals")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private TextMeshPro symbolText;
    [SerializeField] private SpriteRenderer symbolRenderer;
    [SerializeField] private bool tintSpriteWithCategoryColor = false;

    [Header("Settings & Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color blockedColor = new Color(0.65f, 0.65f, 0.65f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.85f, 0.95f, 1f, 1f);
    [SerializeField] private Color hoverColor = new Color(0.95f, 0.95f, 0.95f, 1f);

    public Vector3Int GridPosition { get; private set; }
    public int TileTypeId { get; private set; }
    public bool IsFree { get; private set; }
    public bool IsSelected { get; private set; }
    public bool IsInSlot { get; private set; }

    private MahjongBoard board;
    private bool isHovered = false;
    private Vector3 originalLocalPos;
    private Color originalSymbolColor;
    private Sprite originalSprite;

    public SpriteRenderer SymbolRenderer
    {
        get
        {
            EnsureSymbolRenderer();
            return symbolRenderer;
        }
    }

    private void Awake()
    {
        EnsureSymbolRenderer();
    }

    private void EnsureSymbolRenderer()
    {
        if (symbolRenderer == null)
        {
            Transform existing = transform.Find("SymbolRenderer");
            if (existing != null)
            {
                symbolRenderer = existing.GetComponent<SpriteRenderer>();
            }
            else
            {
                GameObject go = new GameObject("SymbolRenderer");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0.02f, -0.01f);
                go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                symbolRenderer = go.AddComponent<SpriteRenderer>();
            }
        }
    }

    public void Initialize(Vector3Int position, int typeId, Sprite symbolSprite, string symbol, Color symbolColor, MahjongBoard boardRef)
    {
        GridPosition = position;
        TileTypeId = typeId;
        board = boardRef;
        IsFree = false;
        IsSelected = false;
        originalSymbolColor = symbolColor;

        EnsureSymbolRenderer();

        if (symbolRenderer != null && symbolSprite != null)
        {
            symbolRenderer.gameObject.SetActive(true);
            symbolRenderer.sprite = symbolSprite;
            originalSprite = symbolSprite;
            symbolRenderer.color = tintSpriteWithCategoryColor ? symbolColor : Color.white;

            if (symbolText != null)
            {
                symbolText.gameObject.SetActive(false);
            }
        }
        else
        {
            if (symbolRenderer != null)
            {
                symbolRenderer.gameObject.SetActive(false);
            }

            if (symbolText != null)
            {
                symbolText.gameObject.SetActive(true);
                symbolText.text = symbol;
                symbolText.color = symbolColor;
            }
        }


        gameObject.name = $"Tile_{typeId}_{position.x}_{position.y}_{position.z}";


        originalLocalPos = transform.localPosition;

        UpdateVisuals();
    }

    public void SetFree(bool free)
    {
        if (IsFree != free)
        {
            IsFree = free;
            UpdateVisuals();
        }
    }

    public void SetSelected(bool selected)
    {
        if (IsSelected != selected)
        {
            IsSelected = selected;

            float targetYOffset = IsSelected ? 0.2f : 0f;
            transform.localPosition = originalLocalPos + new Vector3(0, targetYOffset, -0.1f * GridPosition.z - (IsSelected ? 0.2f : 0f));
            UpdateVisuals();
        }
    }

    public void UpdateVisuals()
    {
        if (backgroundRenderer == null) return;

        Color targetSymbolColor = tintSpriteWithCategoryColor ? originalSymbolColor : Color.white;

        if (symbolText != null) symbolText.color = originalSymbolColor;
        if (symbolRenderer != null) symbolRenderer.color = targetSymbolColor;

        if (IsSelected)
        {
            backgroundRenderer.color = selectedColor;
        }
        else if (!IsFree)
        {
            backgroundRenderer.color = blockedColor;

            if (symbolText != null) symbolText.color = new Color(originalSymbolColor.r, originalSymbolColor.g, originalSymbolColor.b, 0.35f);
            if (symbolRenderer != null) symbolRenderer.color = new Color(targetSymbolColor.r, targetSymbolColor.g, targetSymbolColor.b, 0.35f);
        }
        else if (isHovered)
        {
            backgroundRenderer.color = hoverColor;
        }
        else
        {
            backgroundRenderer.color = normalColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsFree || IsSelected) return;
        isHovered = true;

        transform.localScale = Vector3.one * 1.05f;
        UpdateVisuals();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsFree) return;
        isHovered = false;
        transform.localScale = Vector3.one;
        UpdateVisuals();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsInSlot || !IsFree) return;
        FindFirstObjectByType<GameManager>()?.OnTileSelected(this);
    }




    public void EnterSlot(Vector3 slotWorldPos, System.Action onComplete)
    {
        IsInSlot = true;


        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;


        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.sortingOrder += 2000;
        var tmp = GetComponentInChildren<TMPro.TextMeshPro>();
        if (tmp != null) tmp.sortingOrder += 2000;


        if (backgroundRenderer != null) backgroundRenderer.color = normalColor;
        if (symbolText != null) symbolText.color = originalSymbolColor;
        if (symbolRenderer != null) symbolRenderer.color = tintSpriteWithCategoryColor ? originalSymbolColor : Color.white;

        StartCoroutine(FlyToPositionCoroutine(slotWorldPos, onComplete));
    }


    public void SlideTo(Vector3 targetWorldPos, System.Action onComplete = null)
    {
        StartCoroutine(FlyToPositionCoroutine(targetWorldPos, onComplete));
    }

    private IEnumerator FlyToPositionCoroutine(Vector3 target, System.Action onComplete)
    {
        float duration = 0.22f;
        float elapsed = 0f;
        Vector3 start = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Pow(1f - elapsed / duration, 2f);
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
        onComplete?.Invoke();
    }


    public void AnimateMatchAndDestroy()
    {
        StartCoroutine(AnimateMatchAndDestroyCoroutine());
    }

    private System.Collections.IEnumerator AnimateMatchAndDestroyCoroutine()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;


        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t * t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
