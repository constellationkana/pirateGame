using UnityEngine;

public class ShipCosmeticApplier : MonoBehaviour
{
    [SerializeField] private string[] cosmeticIds;
    [SerializeField] private Sprite[] cosmeticSprites;
    [SerializeField] private SpriteRenderer targetRenderer;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Start()
    {
        if (targetRenderer == null)
        {
            return;
        }

        string selectedId = PlayerProgression.Instance.GetSelectedShipCosmeticId();
        if (string.IsNullOrWhiteSpace(selectedId))
        {
            return;
        }

        for (int i = 0; i < cosmeticIds.Length && i < cosmeticSprites.Length; i++)
        {
            if (cosmeticIds[i] == selectedId && cosmeticSprites[i] != null)
            {
                targetRenderer.sprite = cosmeticSprites[i];
                return;
            }
        }
    }
}
