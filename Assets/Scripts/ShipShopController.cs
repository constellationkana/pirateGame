using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipShopController : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private TMP_Text doubloonsText;

    [Header("Shop Feedback")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private float messageDuration = 2f;

    [Header("Stat Upgrade Text")]
    [SerializeField] private TMP_Text healthUpgradeText;
    [SerializeField] private TMP_Text speedUpgradeText;
    [SerializeField] private TMP_Text magnetUpgradeText;
    [SerializeField] private TMP_Text cannonUpgradeText;

    [Header("Unlock Text")]
    [SerializeField] private TMP_Text dashUnlockText;
    [SerializeField] private TMP_Text forceFieldUnlockText;

    [Header("Cosmetics")]
    [SerializeField] private string[] cosmeticIds;
    [SerializeField] private Sprite[] cosmeticSprites;
    [SerializeField] private int[] cosmeticCosts;
    [SerializeField] private TMP_Text cosmeticStatusText;

    [Header("Costs")]
    [SerializeField] private int healthBaseCost = 50;
    [SerializeField] private int speedBaseCost = 75;
    [SerializeField] private int magnetBaseCost = 60;
    [SerializeField] private int cannonBaseCost = 100;
    [SerializeField] private int dashUnlockCost = 250;
    [SerializeField] private int forceFieldUnlockCost = 400;

    private PlayerProgression progression;
    private int currentCosmeticIndex;
    private Coroutine clearMessageRoutine;

    private void Start()
    {
        progression = PlayerProgression.Instance;
        RefreshUI();
    }

    public void BuyHealthUpgrade() => TryBuyUpgrade(GetHealthUpgradeCost(), progression.BuyPermanentHealthUpgrade, "Health upgraded!");
    public void BuySpeedUpgrade() => TryBuyUpgrade(GetSpeedUpgradeCost(), progression.BuyPermanentSpeedUpgrade, "Speed upgraded!");
    public void BuyMagnetUpgrade() => TryBuyUpgrade(GetMagnetUpgradeCost(), progression.BuyPermanentMagnetUpgrade, "Magnet upgraded!");
    public void BuyCannonDamageUpgrade() => TryBuyUpgrade(GetCannonUpgradeCost(), progression.BuyPermanentCannonDamageUpgrade, "Cannon damage upgraded!");

    public void UnlockDash()
    {
        if (progression.IsDashUnlocked())
        {
            SetMessage("Already unlocked.");
            RefreshUI();
            return;
        }

        if (!progression.SpendDoubloons(dashUnlockCost))
        {
            SetMessage("Not enough doubloons.");
            RefreshUI();
            return;
        }

        progression.UnlockDash();
        SetMessage("Dash unlocked!");
        RefreshUI();
    }

    public void UnlockForceField()
    {
        if (progression.IsForceFieldUnlocked())
        {
            SetMessage("Already unlocked.");
            RefreshUI();
            return;
        }

        if (!progression.SpendDoubloons(forceFieldUnlockCost))
        {
            SetMessage("Not enough doubloons.");
            RefreshUI();
            return;
        }

        progression.UnlockForceField();
        SetMessage("Force field unlocked!");
        RefreshUI();
    }

    // Keep legacy button methods functional.
    public void BuyDashUnlock() => UnlockDash();
    public void BuyForceFieldUnlock() => UnlockForceField();

    public void BuyOrSelectCosmetic(int index)
    {
        if (index < 0 || index >= cosmeticIds.Length)
        {
            return;
        }

        string cosmeticId = cosmeticIds[index];
        int cost = (index < cosmeticCosts.Length) ? Mathf.Max(0, cosmeticCosts[index]) : 0;

        if (!progression.IsCosmeticOwned(cosmeticId) && !progression.BuyCosmetic(cosmeticId, cost))
        {
            SetMessage("Not enough doubloons.");
            RefreshUI();
            return;
        }

        progression.SetSelectedShipCosmeticId(cosmeticId);
        SetMessage($"Selected: {cosmeticId}");
        RefreshUI();
    }

    public void CycleCosmetic()
    {
        if (cosmeticIds == null || cosmeticIds.Length == 0)
        {
            SetMessage("No cosmetics configured.");
            return;
        }

        BuyOrSelectCosmetic(currentCosmeticIndex);
        currentCosmeticIndex = (currentCosmeticIndex + 1) % cosmeticIds.Length;
    }

    public void OpenCosmeticShop() => CycleCosmetic();

    public void SetGlobalPrompt(string prompt)
    {
        if (promptText != null)
        {
            promptText.text = prompt;
        }
    }

    public void StartRun()
    {
        SetMessage("Setting sail...");
        SceneManager.LoadScene("MainSea");
    }

    private int GetHealthUpgradeCost() => healthBaseCost * (progression.GetPermanentHealthLevel() + 1);
    private int GetSpeedUpgradeCost() => speedBaseCost * (progression.GetPermanentSpeedLevel() + 1);
    private int GetMagnetUpgradeCost() => magnetBaseCost * (progression.GetPermanentMagnetLevel() + 1);
    private int GetCannonUpgradeCost() => cannonBaseCost * (progression.GetPermanentCannonDamageLevel() + 1);

    private void TryBuyUpgrade(int cost, System.Func<int, bool> buyAction, string successMessage)
    {
        if (!buyAction(cost))
        {
            SetMessage("Not enough doubloons.");
            RefreshUI();
            return;
        }

        SetMessage(successMessage);
        RefreshUI();
    }

    private void SetMessage(string message)
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = message;

        if (clearMessageRoutine != null)
        {
            StopCoroutine(clearMessageRoutine);
        }

        if (messageDuration > 0f)
        {
            clearMessageRoutine = StartCoroutine(ClearMessageAfterDelay(messageDuration));
        }
    }

    private IEnumerator ClearMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (messageText != null)
        {
            messageText.text = string.Empty;
        }

        clearMessageRoutine = null;
    }

    private void RefreshUI()
    {
        if (doubloonsText != null)
        {
            doubloonsText.text = $"Doubloons: {progression.GetDoubloons()}";
        }

        if (healthUpgradeText != null)
        {
            healthUpgradeText.text = $"Max Health Lv {progression.GetPermanentHealthLevel()} | Cost: {GetHealthUpgradeCost()}";
        }

        if (speedUpgradeText != null)
        {
            speedUpgradeText.text = $"Speed Lv {progression.GetPermanentSpeedLevel()} | Cost: {GetSpeedUpgradeCost()}";
        }

        if (magnetUpgradeText != null)
        {
            magnetUpgradeText.text = $"Magnet Lv {progression.GetPermanentMagnetLevel()} | Cost: {GetMagnetUpgradeCost()}";
        }

        if (cannonUpgradeText != null)
        {
            cannonUpgradeText.text = $"Cannon Damage Lv {progression.GetPermanentCannonDamageLevel()} | Cost: {GetCannonUpgradeCost()}";
        }

        if (dashUnlockText != null)
        {
            dashUnlockText.text = progression.IsDashUnlocked() ? "Dash: Unlocked" : $"Unlock Dash ({dashUnlockCost})";
        }

        if (forceFieldUnlockText != null)
        {
            forceFieldUnlockText.text = progression.IsForceFieldUnlocked() ? "Force Field: Unlocked" : $"Unlock Force Field ({forceFieldUnlockCost})";
        }

        if (cosmeticStatusText != null)
        {
            cosmeticStatusText.text = $"Selected Cosmetic: {progression.GetSelectedShipCosmeticId()}\nCosmetics Configured: {Mathf.Min(cosmeticIds.Length, cosmeticSprites.Length)}";
        }
    }
}
