using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipShopController : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private TMP_Text doubloonsText;

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

    private void Start()
    {
        progression = PlayerProgression.Instance;
        RefreshUI();
    }

    public void BuyHealthUpgrade()
    {
        progression.BuyPermanentHealthUpgrade(GetHealthUpgradeCost());
        RefreshUI();
    }

    public void BuySpeedUpgrade()
    {
        progression.BuyPermanentSpeedUpgrade(GetSpeedUpgradeCost());
        RefreshUI();
    }

    public void BuyMagnetUpgrade()
    {
        progression.BuyPermanentMagnetUpgrade(GetMagnetUpgradeCost());
        RefreshUI();
    }

    public void BuyCannonDamageUpgrade()
    {
        progression.BuyPermanentCannonDamageUpgrade(GetCannonUpgradeCost());
        RefreshUI();
    }

    public void BuyDashUnlock()
    {
        if (!progression.IsDashUnlocked() && progression.SpendDoubloons(dashUnlockCost))
        {
            progression.UnlockDash();
        }

        RefreshUI();
    }

    public void BuyForceFieldUnlock()
    {
        if (!progression.IsForceFieldUnlocked() && progression.SpendDoubloons(forceFieldUnlockCost))
        {
            progression.UnlockForceField();
        }

        RefreshUI();
    }

    public void BuyOrSelectCosmetic(int index)
    {
        if (index < 0 || index >= cosmeticIds.Length)
        {
            return;
        }

        string cosmeticId = cosmeticIds[index];
        int cost = (index < cosmeticCosts.Length) ? Mathf.Max(0, cosmeticCosts[index]) : 0;

        if (!progression.IsCosmeticOwned(cosmeticId))
        {
            if (!progression.BuyCosmetic(cosmeticId, cost))
            {
                RefreshUI();
                return;
            }
        }

        progression.SetSelectedShipCosmeticId(cosmeticId);
        RefreshUI();
    }

    public void StartRun()
    {
        SceneManager.LoadScene("MainSea");
    }

    private int GetHealthUpgradeCost() => healthBaseCost * (progression.GetPermanentHealthLevel() + 1);
    private int GetSpeedUpgradeCost() => speedBaseCost * (progression.GetPermanentSpeedLevel() + 1);
    private int GetMagnetUpgradeCost() => magnetBaseCost * (progression.GetPermanentMagnetLevel() + 1);
    private int GetCannonUpgradeCost() => cannonBaseCost * (progression.GetPermanentCannonDamageLevel() + 1);

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
