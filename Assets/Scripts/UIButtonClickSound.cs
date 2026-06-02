using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Per-button runtime hook added automatically by UIAudioManager.
/// It only adds/removes its own click listener and never clears existing Button listeners.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonClickSound : MonoBehaviour
{
    private Button button;
    private bool listenerAdded;

    private void Awake()
    {
        button = GetComponent<Button>();
        AddClickListener();
    }

    private void OnEnable()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        AddClickListener();
    }

    private void OnDestroy()
    {
        RemoveClickListener();
    }

    private void AddClickListener()
    {
        if (listenerAdded || button == null)
        {
            return;
        }

        button.onClick.AddListener(PlayClickSound);
        listenerAdded = true;
    }

    private void RemoveClickListener()
    {
        if (!listenerAdded || button == null)
        {
            return;
        }

        button.onClick.RemoveListener(PlayClickSound);
        listenerAdded = false;
    }

    private static void PlayClickSound()
    {
        UIAudioManager.Instance?.PlayButtonClick();
    }
}
