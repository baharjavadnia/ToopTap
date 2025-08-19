using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

[System.Serializable]
public class StoreItemData
{
    [Tooltip("Ball_... یا BG_...")]
    public string itemID;
    public string displayName = "Item";

    [Header("Prices")]
    public int pricePoints = 5000;
    public int priceToman  = 2000;

    [FormerlySerializedAs("poolakeySku")]
    [Tooltip("همان Product Id تعریف شده در بازار")]
    public string productId;

    [Header("Icon Source (یکی کافیست)")]
    public Sprite iconOverride;
    public GameObject prefabForIcon;

    public Sprite GetIconSprite()
    {
        if (iconOverride) return iconOverride;

        if (prefabForIcon)
        {
            var imgSelf  = prefabForIcon.GetComponent<Image>();
            if (imgSelf && imgSelf.sprite) return imgSelf.sprite;

            var imgChild = prefabForIcon.GetComponentInChildren<Image>(true);
            if (imgChild && imgChild.sprite) return imgChild.sprite;

            var srSelf   = prefabForIcon.GetComponent<SpriteRenderer>();
            if (srSelf && srSelf.sprite) return srSelf.sprite;

            var srChild  = prefabForIcon.GetComponentInChildren<SpriteRenderer>(true);
            if (srChild && srChild.sprite) return srChild.sprite;
        }
        return null;
    }
}
