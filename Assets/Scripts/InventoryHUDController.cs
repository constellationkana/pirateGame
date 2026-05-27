using TMPro;
using UnityEngine;

public class InventoryHUDController : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text doubloonText;

    private void Update()
    {
        if (playerInventory == null)
        {
            return;
        }

        if (woodText != null)
        {
            woodText.text = $"Wood: {playerInventory.Wood}";
        }

        if (doubloonText != null)
        {
            doubloonText.text = $"Doubloons: {playerInventory.Doubloons}";
        }
    }
}
