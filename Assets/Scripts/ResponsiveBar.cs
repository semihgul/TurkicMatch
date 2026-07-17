using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class ResponsiveBar : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite middleSprite;
    [SerializeField] private Sprite rightSprite;

    [Header("Child References")]
    [SerializeField] private Image leftImage;
    [SerializeField] private Image middleImage;
    [SerializeField] private Image rightImage;

    private void Start()
    {
        RebuildLayout();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {

        UnityEditor.EditorApplication.delayCall += SafeRebuild;
    }

    private void SafeRebuild()
    {
        UnityEditor.EditorApplication.delayCall -= SafeRebuild;
        if (this != null)
        {
            RebuildLayout();
        }
    }
#endif

    public void Setup(Sprite left, Sprite middle, Sprite right)
    {
        leftSprite = left;
        middleSprite = middle;
        rightSprite = right;
        RebuildLayout();
    }

    public void RebuildLayout()
    {
        if (leftSprite == null || middleSprite == null || rightSprite == null)
            return;


        leftImage = GetOrCreateChildImage("LeftCap", ref leftImage);
        middleImage = GetOrCreateChildImage("MiddleBar", ref middleImage);
        rightImage = GetOrCreateChildImage("RightCap", ref rightImage);


        leftImage.sprite = leftSprite;
        leftImage.type = Image.Type.Simple;
        RectTransform leftRect = leftImage.rectTransform;
        leftRect.anchorMin = new Vector2(0, 0.5f);
        leftRect.anchorMax = new Vector2(0, 0.5f);
        leftRect.pivot = new Vector2(0, 0.5f);
        leftRect.sizeDelta = new Vector2(leftSprite.rect.width, leftSprite.rect.height);


        rightImage.sprite = rightSprite;
        rightImage.type = Image.Type.Simple;
        RectTransform rightRect = rightImage.rectTransform;
        rightRect.anchorMin = new Vector2(1, 0.5f);
        rightRect.anchorMax = new Vector2(1, 0.5f);
        rightRect.pivot = new Vector2(1, 0.5f);
        rightRect.sizeDelta = new Vector2(rightSprite.rect.width, rightSprite.rect.height);


        middleImage.sprite = middleSprite;
        middleImage.type = Image.Type.Tiled;
        RectTransform middleRect = middleImage.rectTransform;
        middleRect.anchorMin = new Vector2(0, 0.5f);
        middleRect.anchorMax = new Vector2(1, 0.5f);
        middleRect.pivot = new Vector2(0.5f, 0.5f);
        

        middleRect.offsetMin = new Vector2(leftSprite.rect.width, middleRect.offsetMin.y);
        middleRect.offsetMax = new Vector2(-rightSprite.rect.width, middleRect.offsetMax.y);
        middleRect.sizeDelta = new Vector2(middleRect.sizeDelta.x, middleSprite.rect.height);


        float leftY = (leftSprite.rect.y + leftSprite.pivot.y) - (middleSprite.rect.y + middleSprite.pivot.y);
        float rightY = (rightSprite.rect.y + rightSprite.pivot.y) - (middleSprite.rect.y + middleSprite.pivot.y);

        leftRect.anchoredPosition = new Vector2(0, leftY);
        rightRect.anchoredPosition = new Vector2(0, rightY);
        middleRect.anchoredPosition = new Vector2(middleRect.anchoredPosition.x, 0);

#if UNITY_EDITOR

        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(gameObject);
            if (leftImage != null) UnityEditor.EditorUtility.SetDirty(leftImage.gameObject);
            if (middleImage != null) UnityEditor.EditorUtility.SetDirty(middleImage.gameObject);
            if (rightImage != null) UnityEditor.EditorUtility.SetDirty(rightImage.gameObject);
        }
#endif
    }

    private Image GetOrCreateChildImage(string childName, ref Image imgRef)
    {
        if (imgRef != null) return imgRef;

        Transform child = transform.Find(childName);
        if (child == null)
        {
            GameObject go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            child = go.transform;
        }

        Image img = child.GetComponent<Image>();
        if (img == null)
        {
            img = child.gameObject.AddComponent<Image>();
        }
        return img;
    }
}
