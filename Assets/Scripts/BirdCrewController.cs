using System.Collections.Generic;
using UnityEngine;

public class BirdCrewController : MonoBehaviour
{
    public enum BirdCrewType
    {
        BirdBoy,
        EvilBirdBoy
    }

    [Header("Crew")]
    [SerializeField] private BirdCrewType crewType = BirdCrewType.BirdBoy;
    [SerializeField] private RunCrewManager runCrewManager;
    [SerializeField] private Transform playerTransform;

    [Header("Parrot Visuals")]
    [SerializeField] private GameObject parrotPrefab;
    [SerializeField] private int parrotCount = 2;

    [Header("Parrot Movement")]
    [SerializeField] private float flyRadius = 1.6f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float positionChangeIntervalMin = 0.35f;
    [SerializeField] private float positionChangeIntervalMax = 1f;
    [SerializeField] private float jitterAmount = 0.25f;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private int baseDamage = 1;
    [SerializeField] private float baseCooldown = 1.5f;
    [SerializeField] private float firingCooldown = 1.5f;
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float homingTurnSpeed = 360f;
    [Range(0f, 1f)] [SerializeField] private float slowChance = 0.25f;
    [SerializeField] private float slowDuration = 1.5f;

    [Header("Upgrades")]
    [SerializeField] private int damageIncreasePerLevel = 1;
    [SerializeField] private float cooldownReductionPerLevel = 0.2f;
    [SerializeField] private float minimumCooldown = 0.35f;
    [SerializeField] private int maxDamageUpgradeLevel = 3;
    [SerializeField] private int maxCooldownUpgradeLevel = 3;

    [Header("Targeting")]
    [SerializeField] private LayerMask enemyDetectionMask = Physics2D.DefaultRaycastLayers;

    private readonly List<Transform> parrots = new();
    private readonly List<ParrotMovementState> parrotMovementStates = new();
    private float nextFireTime;
    private bool loggedDuplicateSuppression;
    private RunCrewManager subscribedRunCrewManager;
    private static Sprite birdBoyProjectileSprite;
    private static Sprite evilBirdBoyProjectileSprite;
    private static Sprite parrotPlaceholderSprite;

    public BirdCrewType CrewType => crewType;
    public bool HasAssignedPrefabs => parrotPrefab != null || projectilePrefab != null;

    private string CrewId => crewType == BirdCrewType.BirdBoy ? RunCrewManager.BirdBoyCrewId : RunCrewManager.EvilBirdBoyCrewId;
    private string DamageUpgradeId => crewType == BirdCrewType.BirdBoy ? RunCrewManager.BirdBoyDamageUpgradeId : RunCrewManager.EvilBirdBoyDamageUpgradeId;
    private string CooldownUpgradeId => crewType == BirdCrewType.BirdBoy ? RunCrewManager.BirdBoyCooldownUpgradeId : RunCrewManager.EvilBirdBoyCooldownUpgradeId;

    public void SetCrewType(BirdCrewType type)
    {
        crewType = type;
        RefreshActiveState();
    }

    public void ConfigureRuntimeReferences(RunCrewManager newRunCrewManager, Transform newPlayerTransform)
    {
        if (newRunCrewManager != null && runCrewManager != newRunCrewManager)
        {
            runCrewManager = newRunCrewManager;
        }

        if (newPlayerTransform != null && playerTransform != newPlayerTransform)
        {
            playerTransform = newPlayerTransform;
        }

        EnsureReferences();
        RefreshActiveState();
    }

    public bool ConfigurePrefabsIfMissing(GameObject newParrotPrefab, GameObject newProjectilePrefab)
    {
        bool changed = false;
        if (parrotPrefab == null && newParrotPrefab != null)
        {
            parrotPrefab = newParrotPrefab;
            changed = true;
        }

        if (projectilePrefab == null && newProjectilePrefab != null)
        {
            projectilePrefab = newProjectilePrefab;
            changed = true;
        }

        if (changed)
        {
            RefreshActiveState();
        }

        return changed;
    }

    public bool HasAssignedPrefsFrom(GameObject assignedParrotPrefab, GameObject assignedProjectilePrefab)
    {
        return (assignedParrotPrefab != null && parrotPrefab == assignedParrotPrefab)
            || (assignedProjectilePrefab != null && projectilePrefab == assignedProjectilePrefab);
    }

    public void LogPrefabAssignmentSource(string source)
    {
        Debug.Log($"[BirdCrewController] {GetControllerDebugName()} using {source}. Parrot prefab: {GetPrefabName(parrotPrefab)}. Projectile prefab: {GetPrefabName(projectilePrefab)}.", this);
    }

    public int MaxDamageUpgradeLevel => Mathf.Max(1, maxDamageUpgradeLevel);
    public int MaxCooldownUpgradeLevel => Mathf.Max(1, maxCooldownUpgradeLevel);

    private void Awake()
    {
        EnsureReferences();
        firingCooldown = Mathf.Max(0.01f, firingCooldown <= 0f ? baseCooldown : firingCooldown);
    }

    private void OnEnable()
    {
        EnsureReferences();
        RefreshActiveState();
    }

    private void OnDisable()
    {
        UnsubscribeFromRunCrewManager();
        ClearParrots();
    }

    private void Update()
    {
        EnsureReferences();
        if (!CanSpawnParrots())
        {
            ClearParrots();
            return;
        }

        EnsureParrots();
        UpdateParrotMovement();

        if (Time.time < nextFireTime)
        {
            return;
        }

        ShipHealth target = PickRandomEnemyInRange();
        if (target == null)
        {
            return;
        }

        FireAt(target);
        nextFireTime = Time.time + GetCurrentCooldown();
    }

    private void HandleCrewStateChanged()
    {
        RefreshActiveState();
        if (IsCrewActive())
        {
            nextFireTime = Mathf.Min(nextFireTime, Time.time + GetCurrentCooldown());
        }
    }

    private void RefreshActiveState()
    {
        if (CanSpawnParrots())
        {
            EnsureParrots();
        }
        else
        {
            ClearParrots();
        }
    }

    private bool IsCrewActive()
    {
        return runCrewManager != null && runCrewManager.IsCrewActive(CrewId);
    }

    private bool CanSpawnParrots()
    {
        if (!IsCrewActive())
        {
            loggedDuplicateSuppression = false;
            return false;
        }

        bool isPrimaryController = IsPrimaryControllerForCrew();
        if (!isPrimaryController && !loggedDuplicateSuppression)
        {
            Debug.Log($"[BirdCrewController] {GetControllerDebugName()} is active but will not spawn parrots because another {crewType} controller is the visual owner.", this);
            loggedDuplicateSuppression = true;
        }
        else if (isPrimaryController)
        {
            loggedDuplicateSuppression = false;
        }

        return isPrimaryController;
    }

    private void FireAt(ShipHealth target)
    {
        Transform firePoint = GetFirePoint();
        if (firePoint == null || target == null)
        {
            return;
        }

        GameObject projectileObject = projectilePrefab != null
            ? Instantiate(projectilePrefab, firePoint.position, Quaternion.identity)
            : CreateRuntimeProjectileObject(firePoint.position);
        BirdHomingProjectile projectile = projectileObject.GetComponent<BirdHomingProjectile>();
        if (projectile == null)
        {
            projectile = projectileObject.AddComponent<BirdHomingProjectile>();
        }

        Vector2 direction = (target.transform.position - firePoint.position).sqrMagnitude > 0.001f
            ? ((Vector2)(target.transform.position - firePoint.position)).normalized
            : Vector2.up;
        projectile.SetFiredByPlayer(true);
        projectile.Initialize(target.transform, direction, projectileSpeed, homingTurnSpeed, GetCurrentDamage(), slowChance, slowDuration, GetOwnerObject());
        Debug.Log($"[BirdCrewController] {GetControllerDebugName()} missile fired at {target.name} using {(projectilePrefab != null ? "assigned projectile prefab" : "placeholder projectile")}.", this);
    }

    private ShipHealth PickRandomEnemyInRange()
    {
        Transform origin = GetFirePoint();
        if (origin == null || detectionRadius <= 0f)
        {
            return null;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin.position, detectionRadius, enemyDetectionMask);
        List<ShipHealth> candidates = new();
        foreach (Collider2D hit in hits)
        {
            ShipHealth health = hit.GetComponentInParent<ShipHealth>();
            if (IsValidEnemy(health) && !candidates.Contains(health))
            {
                candidates.Add(health);
            }
        }

        return candidates.Count == 0 ? null : candidates[Random.Range(0, candidates.Count)];
    }

    private bool IsValidEnemy(ShipHealth health)
    {
        if (health == null || health.IsDead)
        {
            return false;
        }

        if (playerTransform != null && (health.transform == playerTransform || health.transform.IsChildOf(playerTransform) || playerTransform.IsChildOf(health.transform)))
        {
            return false;
        }

        return health.GetComponentInParent<SimpleEnemyShipAI>() != null
            || health.GetComponentInParent<EnemyShipAttack>() != null
            || !health.CompareTag("PlayerShip");
    }

    private void EnsureParrots()
    {
        int desiredCount = Mathf.Max(0, parrotCount);
        RemoveMissingParrotReferences();
        if (parrots.Count == desiredCount)
        {
            return;
        }

        ClearParrots();

        if (desiredCount == 0)
        {
            Debug.Log($"[BirdCrewController] {GetControllerDebugName()} spawned 0 parrots (parrotCount is 0).", this);
            return;
        }

        Transform parent = playerTransform != null ? playerTransform : transform;
        bool usingPrefab = parrotPrefab != null;
        for (int i = 0; i < desiredCount; i++)
        {
            GameObject parrotObject = usingPrefab
                ? Instantiate(parrotPrefab, parent.position, Quaternion.identity, parent)
                : CreateRuntimeParrotObject(parent.position, parent);

            parrotObject.name = $"{GetCrewDisplayName()} Parrot {i + 1}";
            ParrotVisualMarker marker = parrotObject.GetComponent<ParrotVisualMarker>();
            if (marker == null)
            {
                marker = parrotObject.AddComponent<ParrotVisualMarker>();
            }

            marker.Owner = this;
            marker.CrewType = crewType;

            parrots.Add(parrotObject.transform);
            ParrotMovementState movementState = new();
            parrotMovementStates.Add(movementState);
            PickNewParrotTarget(movementState);
        }

        Debug.Log($"[BirdCrewController] {GetControllerDebugName()} spawned {parrots.Count}/{desiredCount} {GetCrewDisplayName()} parrot visuals using {(usingPrefab ? "prefab" : "placeholder")}.", this);
    }

    private void UpdateParrotMovement()
    {
        if (playerTransform == null || parrots.Count == 0)
        {
            return;
        }

        EnsureParrotMovementStateCount();
        for (int i = 0; i < parrots.Count; i++)
        {
            Transform parrot = parrots[i];
            if (parrot == null)
            {
                continue;
            }

            ParrotMovementState movementState = parrotMovementStates[i];
            Vector3 playerPosition = playerTransform.position;
            Vector3 targetPosition = GetParrotTargetPosition(movementState);
            float distanceToPlayer = Vector3.Distance(parrot.position, playerPosition);
            if (!movementState.HasTarget || Time.time >= movementState.NextTargetTime || IsNearCurrentTarget(parrot, targetPosition) || IsTooFarFromPlayer(distanceToPlayer))
            {
                PickNewParrotTarget(movementState);
                targetPosition = GetParrotTargetPosition(movementState);
            }

            Vector2 jitterOffset = Random.insideUnitCircle * Mathf.Max(0f, jitterAmount);
            Vector3 desiredPosition = targetPosition + new Vector3(jitterOffset.x, jitterOffset.y, 0f);
            float speedMultiplier = IsTooFarFromPlayer(distanceToPlayer) ? 1.75f : 1f;
            Vector3 previousPosition = parrot.position;
            parrot.position = Vector3.MoveTowards(parrot.position, desiredPosition, Mathf.Max(0f, moveSpeed) * speedMultiplier * Time.deltaTime);

            Vector3 travelDirection = parrot.position - previousPosition;
            if (travelDirection.sqrMagnitude > 0.001f)
            {
                parrot.up = travelDirection.normalized;
            }
        }
    }

    private void EnsureParrotMovementStateCount()
    {
        while (parrotMovementStates.Count < parrots.Count)
        {
            ParrotMovementState movementState = new();
            parrotMovementStates.Add(movementState);
            PickNewParrotTarget(movementState);
        }

        while (parrotMovementStates.Count > parrots.Count)
        {
            parrotMovementStates.RemoveAt(parrotMovementStates.Count - 1);
        }
    }

    private bool IsNearCurrentTarget(Transform parrot, Vector3 targetPosition)
    {
        return Vector3.SqrMagnitude(parrot.position - targetPosition) <= 0.12f * 0.12f;
    }

    private bool IsTooFarFromPlayer(float distanceToPlayer)
    {
        return distanceToPlayer > Mathf.Max(0.1f, flyRadius) * 1.5f;
    }

    private void PickNewParrotTarget(ParrotMovementState movementState)
    {
        movementState.TargetOffset = Random.insideUnitCircle * Mathf.Max(0.1f, flyRadius);
        movementState.NextTargetTime = Time.time + Random.Range(
            Mathf.Max(0.01f, positionChangeIntervalMin),
            Mathf.Max(Mathf.Max(0.01f, positionChangeIntervalMin), positionChangeIntervalMax));
        movementState.HasTarget = true;
    }

    private Vector3 GetParrotTargetPosition(ParrotMovementState movementState)
    {
        Transform targetTransform = playerTransform != null ? playerTransform : transform;
        return targetTransform.position + new Vector3(movementState.TargetOffset.x, movementState.TargetOffset.y, 0f);
    }

    private void ClearParrots()
    {
        int destroyedCount = 0;
        HashSet<GameObject> destroyedObjects = new();
        for (int i = 0; i < parrots.Count; i++)
        {
            if (parrots[i] != null && destroyedObjects.Add(parrots[i].gameObject))
            {
                destroyedCount++;
                Destroy(parrots[i].gameObject);
            }
        }

        parrots.Clear();
        parrotMovementStates.Clear();

        ParrotVisualMarker[] markers = FindObjectsByType<ParrotVisualMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (ParrotVisualMarker marker in markers)
        {
            if (marker != null && marker.Owner == this && destroyedObjects.Add(marker.gameObject))
            {
                destroyedCount++;
                Destroy(marker.gameObject);
            }
        }

        if (destroyedCount > 0)
        {
            Debug.Log($"[BirdCrewController] {GetControllerDebugName()} cleared {destroyedCount} existing parrot visuals before respawn/deactivation.", this);
        }
    }

    private void RemoveMissingParrotReferences()
    {
        for (int i = parrots.Count - 1; i >= 0; i--)
        {
            if (parrots[i] != null)
            {
                continue;
            }

            parrots.RemoveAt(i);
            if (i < parrotMovementStates.Count)
            {
                parrotMovementStates.RemoveAt(i);
            }
        }
    }

    private Transform GetFirePoint()
    {
        if (parrots.Count > 0)
        {
            int index = Mathf.Abs(Mathf.FloorToInt(Time.time / Mathf.Max(0.01f, GetCurrentCooldown()))) % parrots.Count;
            if (parrots[index] != null)
            {
                return parrots[index];
            }
        }

        return playerTransform != null ? playerTransform : transform;
    }

    private int GetCurrentDamage()
    {
        int level = runCrewManager == null ? 0 : runCrewManager.GetBirdCrewUpgradeLevel(DamageUpgradeId);
        return Mathf.Max(0, baseDamage + level * damageIncreasePerLevel);
    }

    private float GetCurrentCooldown()
    {
        int level = runCrewManager == null ? 0 : runCrewManager.GetBirdCrewUpgradeLevel(CooldownUpgradeId);
        float startingCooldown = firingCooldown > 0f ? firingCooldown : baseCooldown;
        return Mathf.Max(minimumCooldown, startingCooldown - level * cooldownReductionPerLevel);
    }

    private GameObject GetOwnerObject()
    {
        return playerTransform != null ? playerTransform.gameObject : gameObject;
    }

    private void EnsureReferences()
    {
        if (runCrewManager == null)
        {
            runCrewManager = FindFirstObjectByType<RunCrewManager>();
        }

        SyncRunCrewManagerSubscription();

        if (playerTransform == null)
        {
            ShipController2D shipController = FindFirstObjectByType<ShipController2D>();
            if (shipController != null)
            {
                playerTransform = shipController.transform;
            }
        }
    }

    private void SyncRunCrewManagerSubscription()
    {
        if (subscribedRunCrewManager != null && subscribedRunCrewManager != runCrewManager)
        {
            subscribedRunCrewManager.CrewStateChanged -= HandleCrewStateChanged;
            subscribedRunCrewManager = null;
        }

        if (runCrewManager == null)
        {
            return;
        }

        runCrewManager.SetBirdCrewUpgradeMaxLevels(CrewId, maxDamageUpgradeLevel, maxCooldownUpgradeLevel);
        if (subscribedRunCrewManager == null && isActiveAndEnabled)
        {
            runCrewManager.CrewStateChanged += HandleCrewStateChanged;
            subscribedRunCrewManager = runCrewManager;
            Debug.Log($"[BirdCrewController] {GetControllerDebugName()} registered with RunCrewManager '{runCrewManager.name}'.", this);
        }
    }

    private void UnsubscribeFromRunCrewManager()
    {
        if (subscribedRunCrewManager == null)
        {
            return;
        }

        subscribedRunCrewManager.CrewStateChanged -= HandleCrewStateChanged;
        subscribedRunCrewManager = null;
    }

    private bool IsPrimaryControllerForCrew()
    {
        BirdCrewController[] controllers = FindObjectsByType<BirdCrewController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        BirdCrewController primaryController = null;
        foreach (BirdCrewController controller in controllers)
        {
            if (controller == null || controller.CrewType != crewType || !controller.IsCrewActive())
            {
                continue;
            }

            if (primaryController == null || ShouldPreferController(controller, primaryController))
            {
                primaryController = controller;
            }
        }

        return primaryController == null || primaryController == this;
    }

    private static bool ShouldPreferController(BirdCrewController candidate, BirdCrewController current)
    {
        bool candidateHasPrefab = candidate.parrotPrefab != null;
        bool currentHasPrefab = current.parrotPrefab != null;
        if (candidateHasPrefab != currentHasPrefab)
        {
            return candidateHasPrefab;
        }

        return candidate.GetInstanceID() < current.GetInstanceID();
    }

    private string GetCrewDisplayName()
    {
        return crewType == BirdCrewType.BirdBoy ? "Bird-Boy" : "Evil-Bird-Boy";
    }

    private string GetControllerDebugName()
    {
        string managerName = runCrewManager != null ? runCrewManager.name : "no RunCrewManager";
        return $"{GetCrewDisplayName()} controller '{name}' (id {GetInstanceID()}, manager {managerName}, parrotCount {Mathf.Max(0, parrotCount)})";
    }

    private static string GetPrefabName(GameObject prefab)
    {
        return prefab != null ? prefab.name : "none";
    }

    private GameObject CreateRuntimeProjectileObject(Vector3 position)
    {
        GameObject projectile = new(crewType == BirdCrewType.BirdBoy ? "Egg Missile" : "Poop Missile");
        projectile.transform.position = position;
        SpriteRenderer spriteRenderer = projectile.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = crewType == BirdCrewType.BirdBoy ? GetBirdBoyProjectileSprite() : GetEvilBirdBoyProjectileSprite();
        spriteRenderer.sortingOrder = 20;
        CircleCollider2D collider = projectile.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.16f;
        projectile.AddComponent<Rigidbody2D>();
        projectile.AddComponent<BirdHomingProjectile>();
        return projectile;
    }

    private GameObject CreateRuntimeParrotObject(Vector3 position, Transform parent)
    {
        GameObject parrot = new("RuntimeParrot");
        parrot.transform.SetParent(parent, false);
        parrot.transform.position = position;
        SpriteRenderer spriteRenderer = parrot.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetParrotPlaceholderSprite();
        spriteRenderer.color = crewType == BirdCrewType.BirdBoy ? new Color(0.2f, 0.8f, 0.25f, 1f) : new Color(0.35f, 0.1f, 0.45f, 1f);
        spriteRenderer.sortingOrder = 15;
        return parrot;
    }

    private sealed class ParrotMovementState
    {
        public Vector2 TargetOffset;
        public float NextTargetTime;
        public bool HasTarget;
    }

    private sealed class ParrotVisualMarker : MonoBehaviour
    {
        public BirdCrewController Owner;
        public BirdCrewType CrewType;
    }

    private static Sprite GetBirdBoyProjectileSprite()
    {
        return birdBoyProjectileSprite != null ? birdBoyProjectileSprite : birdBoyProjectileSprite = CreateCircleSprite(new Color(1f, 0.95f, 0.75f, 1f));
    }

    private static Sprite GetEvilBirdBoyProjectileSprite()
    {
        return evilBirdBoyProjectileSprite != null ? evilBirdBoyProjectileSprite : evilBirdBoyProjectileSprite = CreateCircleSprite(new Color(0.33f, 0.18f, 0.06f, 1f));
    }

    private static Sprite GetParrotPlaceholderSprite()
    {
        return parrotPlaceholderSprite != null ? parrotPlaceholderSprite : parrotPlaceholderSprite = CreateCircleSprite(Color.white);
    }

    private static Sprite CreateCircleSprite(Color color)
    {
        const int textureSize = 32;
        Texture2D texture = new(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Vector2 center = new((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
        float radius = textureSize * 0.45f;
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - Mathf.InverseLerp(radius * 0.8f, radius, distance));
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
    }
}
