using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipShopCurrencyHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text doubloonText;
    [SerializeField] private string format = "Doubloons: {0}";
    [SerializeField] private float refreshInterval = 0.25f;

    private float nextRefreshTime;

    private void Awake()
    {
        if (doubloonText == null)
        {
            CreateFallbackHUD();
        }
    }

    private void OnEnable()
    {
        Refresh();
        nextRefreshTime = Time.unscaledTime + Mathf.Max(0.05f, refreshInterval);
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefreshTime)
        {
            return;
        }

        Refresh();
        nextRefreshTime = Time.unscaledTime + Mathf.Max(0.05f, refreshInterval);
    }

    public void Refresh()
    {
        if (doubloonText == null)
        {
            return;
        }

        if (!PlayerProgression.HasActiveSaveSlot)
        {
            doubloonText.text = "No Save Active";
            return;
        }

        doubloonText.text = string.Format(format, PlayerProgression.Instance.GetDoubloons());
    }

    private void CreateFallbackHUD()
    {
        Canvas canvas = CreateCanvas();
        GameObject textObject = new("ShipShopDoubloonHUDText");
        textObject.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.sizeDelta = new Vector2(420f, 56f);
        rectTransform.anchoredPosition = new Vector2(-24f, -24f);

        doubloonText = textObject.AddComponent<TextMeshProUGUI>();
        doubloonText.alignment = TextAlignmentOptions.Right;
        doubloonText.fontSize = 30f;
        doubloonText.color = new Color(1f, 0.86f, 0.32f, 1f);
        doubloonText.raycastTarget = false;
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new("ShipShop Currency HUD Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }
}
