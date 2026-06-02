using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to any main-menu Button root object to apply a subtle scale tween-like hover effect.
/// </summary>
public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float lerpSpeed = 12f;

    private Vector3 _baseScale;
    private Vector3 _targetScale;

    private void Awake()
    {
        _baseScale = transform.localScale;
        _targetScale = _baseScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * lerpSpeed);
    }

    /// <summary>
    /// Handles pointer enter events for hover feedback.
    /// </summary>
    /// <param name="eventData">Pointer event data supplied by Unity UI.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        _targetScale = _baseScale * hoverScale;
    }

    /// <summary>
    /// Handles pointer exit events for hover feedback.
    /// </summary>
    /// <param name="eventData">Pointer event data supplied by Unity UI.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        _targetScale = _baseScale;
    }
}
