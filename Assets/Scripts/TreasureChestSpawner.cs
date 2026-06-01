using System.Collections.Generic;
using UnityEngine;

public class TreasureChestSpawner : MonoBehaviour
{
    [Header("Chest Spawn Settings")]
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float spawnInterval = 30f;
    [SerializeField] private float spawnRadiusMin = 8f;
    [SerializeField] private float spawnRadiusMax = 14f;
    [SerializeField] private int maxActiveChests = 2;
    [Range(0f, 1f)]
    [SerializeField] private float spawnChance = 0.35f;

    private readonly HashSet<TreasureChestPickup> activeChests = new();
    private float nextSpawnTime;
    private RunTimerDirector runTimerDirector;

    private void Awake()
    {
        if (playerTransform == null)
        {
            playerTransform = FindPlayerTransform();
        }

        runTimerDirector = FindFirstObjectByType<RunTimerDirector>();
    }

    private void OnEnable()
    {
        nextSpawnTime = Time.time + Mathf.Max(0.1f, spawnInterval);
    }

    private void Update()
    {
        CleanupMissingChests();

        if (!IsRunActive() || Time.timeScale <= 0f || Time.time < nextSpawnTime)
        {
            return;
        }

        nextSpawnTime = Time.time + Mathf.Max(0.1f, spawnInterval);

        if (activeChests.Count >= Mathf.Max(1, maxActiveChests) || Random.value > spawnChance)
        {
            return;
        }

        SpawnChest();
    }

    public void NotifyChestRemoved(TreasureChestPickup chest)
    {
        if (chest != null)
        {
            activeChests.Remove(chest);
        }
    }

    private bool IsRunActive()
    {
        if (runTimerDirector == null)
        {
            runTimerDirector = FindFirstObjectByType<RunTimerDirector>();
        }

        return runTimerDirector == null || runTimerDirector.RunStarted;
    }

    private void SpawnChest()
    {
        if (playerTransform == null)
        {
            playerTransform = FindPlayerTransform();
        }

        if (playerTransform == null)
        {
            return;
        }

        Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(Mathf.Max(0f, spawnRadiusMin), Mathf.Max(spawnRadiusMin, spawnRadiusMax));
        Vector3 spawnPosition = playerTransform.position + new Vector3(offset.x, offset.y, 0f);
        GameObject chestObject = chestPrefab != null ? Instantiate(chestPrefab, spawnPosition, Quaternion.identity) : CreateFallbackChest(spawnPosition);

        TreasureChestPickup pickup = chestObject.GetComponent<TreasureChestPickup>();
        if (pickup == null)
        {
            pickup = chestObject.AddComponent<TreasureChestPickup>();
        }

        pickup.Initialize(this);
        activeChests.Add(pickup);
    }

    private GameObject CreateFallbackChest(Vector3 position)
    {
        GameObject chestObject = new("Treasure Chest");
        chestObject.transform.position = position;

        SpriteRenderer spriteRenderer = chestObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(0.95f, 0.62f, 0.16f, 1f);
        spriteRenderer.sprite = CreateFallbackSprite();
        spriteRenderer.sortingOrder = 3;

        BoxCollider2D collider = chestObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1.2f, 1f);

        return chestObject;
    }

    private static Sprite CreateFallbackSprite()
    {
        Texture2D texture = new(16, 12);
        Color clear = Color.clear;
        Color gold = new(0.95f, 0.62f, 0.16f, 1f);
        Color dark = new(0.25f, 0.12f, 0.03f, 1f);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                bool border = x == 0 || x == texture.width - 1 || y == 0 || y == texture.height - 1;
                bool band = y == 6 || x == 7 || x == 8;
                texture.SetPixel(x, y, border || band ? dark : gold);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 16f);
    }

    private void CleanupMissingChests()
    {
        activeChests.RemoveWhere(chest => chest == null);
    }

    private static Transform FindPlayerTransform()
    {
        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            return taggedPlayer.transform;
        }

        ShipController2D ship = FindFirstObjectByType<ShipController2D>();
        if (ship != null)
        {
            return ship.transform;
        }

        PlayerWalk2D player = FindFirstObjectByType<PlayerWalk2D>();
        return player != null ? player.transform : null;
    }
}
