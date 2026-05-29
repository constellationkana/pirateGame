using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipShopController : MonoBehaviour
{
    [Serializable]
    private class GenericUnlockOffer
    {
        public string id;
        public int cost = 100;
    }
    [Header("Top Bar")]
    [SerializeField] private TMP_Text doubloonsText;

    [Header("Shop Feedback")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private float messageDuration = 2f;

    [Header("Scene Flow")]
    [SerializeField] private string mainSeaSceneName = "MainSea";
    [SerializeField] private bool logSceneTransitions = true;

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

    [Header("Stat Upgrade Amounts")]
    [SerializeField] private int healthIncreasePerLevel = 2;
    [SerializeField] private float speedIncreasePerLevel = 0.25f;
    [SerializeField] private float magnetRadiusIncreasePerLevel = 0.5f;
    [SerializeField] private int cannonDamageIncreasePerLevel = 1;

    [Header("Future Unlock Offers")]
    [SerializeField]
    private GenericUnlockOffer[] futureUnlockOffers =
    {
        new() { id = PlayerProgression.UnlockHealthRegenId, cost = 150 },
        new() { id = PlayerProgression.UnlockCannonballSizeId, cost = 150 },
        new() { id = PlayerProgression.UnlockCannonballSpeedId, cost = 150 },
        new() { id = PlayerProgression.UnlockCannonballPierceId, cost = 200 },
        new() { id = PlayerProgression.UnlockCannonballExplosionId, cost = 250 },
        new() { id = PlayerProgression.UnlockBarnaclesId, cost = 200 },
        new() { id = PlayerProgression.UnlockCursedDoubloonsId, cost = 200 }
    };

    private PlayerProgression progression;
    private int currentCosmeticIndex;
    private Coroutine clearMessageRoutine;

    private void Start()
    {
        progression = PlayerProgression.Instance;

        if (progression == null)
        {
            Debug.LogWarning("ShipShopController: PlayerProgression singleton could not be created.", this);
            return;
        }

        RefreshUI();
    }

    public void BuyHealthUpgrade()
    {
        if (!HasProgression()) return;
        TryBuyUpgrade(GetHealthUpgradeCost(), (p, cost) => p.BuyPermanentHealthUpgrade(cost), "Health upgraded!");
    }

    public void BuySpeedUpgrade()
    {
        if (!HasProgression()) return;
        TryBuyUpgrade(GetSpeedUpgradeCost(), (p, cost) => p.BuyPermanentSpeedUpgrade(cost), "Speed upgraded!");
    }

    public void BuyMagnetUpgrade()
    {
        if (!HasProgression()) return;
        TryBuyUpgrade(GetMagnetUpgradeCost(), (p, cost) => p.BuyPermanentMagnetUpgrade(cost), "Magnet upgraded and in-run magnet upgrades unlocked!");
    }

    public void BuyCannonDamageUpgrade()
    {
        if (!HasProgression()) return;
        TryBuyUpgrade(GetCannonUpgradeCost(), (p, cost) => p.BuyPermanentCannonDamageUpgrade(cost), "Cannon damage upgraded!");
    }

    public void UnlockDash()
    {
        if (!HasProgression())
        {
            return;
        }

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
        if (!HasProgression())
        {
            return;
        }

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

    public void BuyDashUnlock() => UnlockDash();
    public void BuyForceFieldUnlock() => UnlockForceField();

    public void BuyGenericUnlock(string unlockId)
    {
        if (!HasProgression())
        {
            return;
        }

        if (progression.IsUnlocked(unlockId))
        {
            SetMessage("Already unlocked.");
            RefreshUI();
            return;
        }

        int cost = GetFutureUnlockCost(unlockId);
        if (!progression.SpendDoubloons(cost))
        {
            SetMessage("Not enough doubloons.");
            RefreshUI();
            return;
        }

        progression.Unlock(unlockId);
        SetMessage($"Unlocked: {unlockId}");
        RefreshUI();
    }

    public void BuyOrSelectCosmetic(int index)
    {
        if (!HasProgression())
        {
            return;
        }

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
        if (logSceneTransitions)
        {
            Debug.Log($"Starting run, loading {mainSeaSceneName}", this);
        }

        SetMessage("Setting sail...");

        if (string.IsNullOrWhiteSpace(mainSeaSceneName))
        {
            Debug.LogWarning("ShipShopController: mainSeaSceneName is empty.", this);
            return;
        }

        SceneManager.LoadScene(mainSeaSceneName);
    }

    private int GetHealthUpgradeCost() => healthBaseCost * (progression.GetPermanentHealthLevel() + 1);
    private int GetSpeedUpgradeCost() => speedBaseCost * (progression.GetPermanentSpeedLevel() + 1);
    private int GetMagnetUpgradeCost() => magnetBaseCost * (progression.GetPermanentMagnetLevel() + 1);
    private int GetCannonUpgradeCost() => cannonBaseCost * (progression.GetPermanentCannonDamageLevel() + 1);

    private void TryBuyUpgrade(int cost, Func<PlayerProgression, int, bool> buyAction, string successMessage)
    {
        if (!HasProgression())
        {
            return;
        }

        if (!buyAction(progression, cost))
        {
            SetMessage("Not enough doubloons.");
            RefreshUI();
            return;
        }

        SetMessage(successMessage);
        RefreshUI();
    }

    private int GetFutureUnlockCost(string unlockId)
    {
        if (futureUnlockOffers == null)
        {
            return 0;
        }

        for (int i = 0; i < futureUnlockOffers.Length; i++)
        {
            GenericUnlockOffer offer = futureUnlockOffers[i];
            if (offer != null && string.Equals(offer.id, unlockId, StringComparison.OrdinalIgnoreCase))
            {
                return Mathf.Max(0, offer.cost);
            }
        }

        return 0;
    }

    private bool HasProgression()
    {
        if (progression != null)
        {
            return true;
        }

        progression = PlayerProgression.Instance;
        if (progression == null)
        {
            Debug.LogWarning("ShipShopController: PlayerProgression is unavailable.", this);
            return false;
        }

        return true;
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
        if (!HasProgression())
        {
            return;
        }

        if (doubloonsText != null)
        {
            doubloonsText.text = $"Doubloons: {progression.GetDoubloons()}";
        }

        if (healthUpgradeText != null)
        {
            healthUpgradeText.text = $"Max Health Lv {progression.GetPermanentHealthLevel()} (+{healthIncreasePerLevel}) | Cost: {GetHealthUpgradeCost()}";
        }

        if (speedUpgradeText != null)
        {
            speedUpgradeText.text = $"Speed Lv {progression.GetPermanentSpeedLevel()} (+{speedIncreasePerLevel:0.##}) | Cost: {GetSpeedUpgradeCost()}";
        }

        if (magnetUpgradeText != null)
        {
            string magnetUnlockState = progression.IsUnlocked(PlayerProgression.UnlockMagnetRadius) ? "Unlocked" : "Unlocks in-run upgrade";
            magnetUpgradeText.text = $"Magnet Lv {progression.GetPermanentMagnetLevel()} (+{magnetRadiusIncreasePerLevel:0.##}) | Cost: {GetMagnetUpgradeCost()} | {magnetUnlockState}";
        }

        if (cannonUpgradeText != null)
        {
            cannonUpgradeText.text = $"Cannon Damage Lv {progression.GetPermanentCannonDamageLevel()} (+{cannonDamageIncreasePerLevel}) | Cost: {GetCannonUpgradeCost()}";
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
