using UnityEngine;
using UnityEngine.SceneManagement;

public class TreasureChestRunBootstrap : MonoBehaviour
{
    private static readonly string[] RunSceneNames = { "MainSea", "Stage2", "Stage3" };
    private static bool wasInRunScene;
    private static RunCrewManager currentRunCrewManager;

    [Header("Bird-Boy Prefabs")]
    [SerializeField] private GameObject birdBoyParrotPrefab;
    [SerializeField] private GameObject birdBoyProjectilePrefab;

    [Header("Evil-Bird-Boy Prefabs")]
    [SerializeField] private GameObject evilBirdBoyParrotPrefab;
    [SerializeField] private GameObject evilBirdBoyProjectilePrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureRunSceneServices(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureRunSceneServices(scene);
    }

    public static RunCrewManager EnsureActiveRunSceneServices()
    {
        return EnsureRunSceneServices(SceneManager.GetActiveScene());
    }

    private static RunCrewManager EnsureRunSceneServices(Scene scene)
    {
        bool isRunScene = IsRunScene(scene.name);
        if (!isRunScene)
        {
            wasInRunScene = false;
            return currentRunCrewManager;
        }

        bool startsNewRun = !wasInRunScene;
        wasInRunScene = true;

        TreasureChestRunBootstrap bootstrap = FindBootstrap();
        Transform playerTransform = FindPlayerTransform();
        Debug.Log($"[TreasureChestRunBootstrap] Scene '{scene.name}' loaded; bootstrap ran. Starts new run: {startsNewRun}. Bootstrap prefab source: {(bootstrap != null ? bootstrap.name : "none")}. Player transform: {(playerTransform != null ? playerTransform.name : "none")}");

        RunCrewManager runCrewManager = FindRunCrewManager();
        if (runCrewManager == null)
        {
            GameObject crewManagerObject = new("Run Crew Manager");
            runCrewManager = crewManagerObject.AddComponent<RunCrewManager>();
            Debug.Log($"[TreasureChestRunBootstrap] RunCrewManager created for scene '{scene.name}'.", runCrewManager);
        }
        else
        {
            Debug.Log($"[TreasureChestRunBootstrap] RunCrewManager found: '{runCrewManager.name}' in scene '{scene.name}'.", runCrewManager);
        }

        currentRunCrewManager = runCrewManager;
        Object.DontDestroyOnLoad(runCrewManager.gameObject);

        if (startsNewRun)
        {
            runCrewManager.ResetRunCrew();
        }

        EnsureSupportController<PaulCrewController>(runCrewManager.gameObject, "PaulCrewController");
        EnsureSupportController<CleanUpCrewController>(runCrewManager.gameObject, "CleanUpCrewController");
        EnsureBirdCrewController(runCrewManager, BirdCrewController.BirdCrewType.BirdBoy, bootstrap, playerTransform);
        EnsureBirdCrewController(runCrewManager, BirdCrewController.BirdCrewType.EvilBirdBoy, bootstrap, playerTransform);

        if (Object.FindFirstObjectByType<TreasureChestChoiceUI>() == null)
        {
            GameObject uiObject = new("Treasure Chest Choice UI");
            uiObject.AddComponent<TreasureChestChoiceUI>();
        }

        if (Object.FindFirstObjectByType<TreasureChestSpawner>() == null)
        {
            GameObject spawnerObject = new("Treasure Chest Spawner");
            spawnerObject.AddComponent<TreasureChestSpawner>();
        }

        return runCrewManager;
    }

    private static TreasureChestRunBootstrap FindBootstrap()
    {
        return Object.FindFirstObjectByType<TreasureChestRunBootstrap>();
    }

    private static RunCrewManager FindRunCrewManager()
    {
        if (currentRunCrewManager != null)
        {
            return currentRunCrewManager;
        }

        return Object.FindFirstObjectByType<RunCrewManager>();
    }

    private static T EnsureSupportController<T>(GameObject owner, string controllerName) where T : Component
    {
        T controller = owner.GetComponent<T>();
        if (controller != null)
        {
            Debug.Log($"[TreasureChestRunBootstrap] {controllerName} found on '{owner.name}'.", owner);
            return controller;
        }

        controller = owner.AddComponent<T>();
        Debug.Log($"[TreasureChestRunBootstrap] {controllerName} created on '{owner.name}'.", owner);
        return controller;
    }

    private static BirdCrewController EnsureBirdCrewController(RunCrewManager runCrewManager, BirdCrewController.BirdCrewType crewType, TreasureChestRunBootstrap bootstrap, Transform playerTransform)
    {
        if (runCrewManager == null)
        {
            return null;
        }

        BirdCrewController controller = FindConfiguredSceneController(crewType)
            ?? FindPlayerShipController(crewType)
            ?? FindControllerOnOwner(runCrewManager.gameObject, crewType);

        if (controller == null)
        {
            controller = AddBirdCrewController(runCrewManager.gameObject, crewType);
            Debug.Log($"[TreasureChestRunBootstrap] {GetCrewDisplayName(crewType)} BirdCrewController created on '{runCrewManager.name}'.", controller);
        }
        else
        {
            Debug.Log($"[TreasureChestRunBootstrap] {GetCrewDisplayName(crewType)} BirdCrewController found on '{controller.name}'.", controller);
        }

        ConfigureBirdCrewController(controller, runCrewManager, playerTransform, crewType, bootstrap);
        return controller;
    }

    private static BirdCrewController FindConfiguredSceneController(BirdCrewController.BirdCrewType crewType)
    {
        BirdCrewController bestController = null;
        BirdCrewController[] controllers = Object.FindObjectsByType<BirdCrewController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (BirdCrewController controller in controllers)
        {
            if (controller == null || controller.CrewType != crewType || !controller.HasAssignedPrefabs)
            {
                continue;
            }

            if (bestController == null || IsOnPlayerShip(controller))
            {
                bestController = controller;
            }
        }

        return bestController;
    }

    private static BirdCrewController FindPlayerShipController(BirdCrewController.BirdCrewType crewType)
    {
        GameObject playerShip = FindPlayerShip();
        if (playerShip == null)
        {
            return null;
        }

        BirdCrewController[] controllers = playerShip.GetComponents<BirdCrewController>();
        foreach (BirdCrewController controller in controllers)
        {
            if (controller != null && controller.CrewType == crewType)
            {
                return controller;
            }
        }

        return null;
    }

    private static BirdCrewController FindControllerOnOwner(GameObject owner, BirdCrewController.BirdCrewType crewType)
    {
        BirdCrewController[] controllers = owner.GetComponents<BirdCrewController>();
        foreach (BirdCrewController controller in controllers)
        {
            if (controller != null && controller.CrewType == crewType)
            {
                return controller;
            }
        }

        return null;
    }

    private static BirdCrewController AddBirdCrewController(GameObject owner, BirdCrewController.BirdCrewType crewType)
    {
        BirdCrewController controller = owner.AddComponent<BirdCrewController>();
        controller.SetCrewType(crewType);
        return controller;
    }

    private static void ConfigureBirdCrewController(BirdCrewController controller, RunCrewManager runCrewManager, Transform playerTransform, BirdCrewController.BirdCrewType crewType, TreasureChestRunBootstrap bootstrap)
    {
        if (controller == null)
        {
            return;
        }

        controller.ConfigureRuntimeReferences(runCrewManager, playerTransform);
        ApplyBootstrapPrefabs(controller, crewType, bootstrap);
    }

    private static void ApplyBootstrapPrefabs(BirdCrewController controller, BirdCrewController.BirdCrewType crewType, TreasureChestRunBootstrap bootstrap)
    {
        if (controller == null)
        {
            return;
        }

        bool hadAssignedPrefabs = controller.HasAssignedPrefabs;
        GameObject parrotPrefab = bootstrap == null ? null : bootstrap.GetParrotPrefab(crewType);
        GameObject projectilePrefab = bootstrap == null ? null : bootstrap.GetProjectilePrefab(crewType);
        bool usedBootstrapPrefab = controller.ConfigurePrefabsIfMissing(parrotPrefab, projectilePrefab);

        if (hadAssignedPrefabs && usedBootstrapPrefab)
        {
            controller.LogPrefabAssignmentSource("scene-assigned prefab with bootstrap filling missing prefab");
        }
        else if (hadAssignedPrefabs)
        {
            controller.LogPrefabAssignmentSource("scene-assigned prefab");
        }
        else
        {
            controller.LogPrefabAssignmentSource(usedBootstrapPrefab ? "bootstrap-assigned prefab" : "no assigned prefab; using placeholder fallback");
        }
    }

    private GameObject GetParrotPrefab(BirdCrewController.BirdCrewType crewType)
    {
        return crewType == BirdCrewController.BirdCrewType.BirdBoy ? birdBoyParrotPrefab : evilBirdBoyParrotPrefab;
    }

    private GameObject GetProjectilePrefab(BirdCrewController.BirdCrewType crewType)
    {
        return crewType == BirdCrewController.BirdCrewType.BirdBoy ? birdBoyProjectilePrefab : evilBirdBoyProjectilePrefab;
    }

    private static bool IsOnPlayerShip(BirdCrewController controller)
    {
        GameObject playerShip = FindPlayerShip();
        return playerShip != null && controller != null && controller.gameObject == playerShip;
    }

    private static GameObject FindPlayerShip()
    {
        GameObject taggedShip = GameObject.FindWithTag("PlayerShip");
        if (taggedShip != null)
        {
            return taggedShip;
        }

        return GameObject.Find("PlayerShip");
    }

    private static Transform FindPlayerTransform()
    {
        GameObject playerShip = FindPlayerShip();
        if (playerShip != null)
        {
            return playerShip.transform;
        }

        ShipController2D shipController = Object.FindFirstObjectByType<ShipController2D>();
        if (shipController != null)
        {
            return shipController.transform;
        }

        return null;
    }

    private static string GetCrewDisplayName(BirdCrewController.BirdCrewType crewType)
    {
        return crewType == BirdCrewController.BirdCrewType.BirdBoy ? "Bird-Boy" : "Evil-Bird-Boy";
    }

    private static bool IsRunScene(string sceneName)
    {
        foreach (string runSceneName in RunSceneNames)
        {
            if (sceneName == runSceneName)
            {
                return true;
            }
        }

        return false;
    }
}
