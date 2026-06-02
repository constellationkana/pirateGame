using UnityEngine;
using UnityEngine.SceneManagement;

public class TreasureChestRunBootstrap : MonoBehaviour
{
    private static readonly string[] RunSceneNames = { "MainSea", "Stage2", "Stage3" };
    private static bool wasInRunScene;

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

    private static void EnsureRunSceneServices(Scene scene)
    {
        bool isRunScene = IsRunScene(scene.name);
        if (!isRunScene)
        {
            wasInRunScene = false;
            return;
        }

        bool startsNewRun = !wasInRunScene;
        wasInRunScene = true;

        TreasureChestRunBootstrap bootstrap = FindBootstrap();
        RunCrewManager runCrewManager = Object.FindFirstObjectByType<RunCrewManager>();
        if (runCrewManager == null)
        {
            GameObject crewManagerObject = new("Run Crew Manager");
            runCrewManager = crewManagerObject.AddComponent<RunCrewManager>();
            crewManagerObject.AddComponent<PaulCrewController>();
            crewManagerObject.AddComponent<CleanUpCrewController>();
            EnsureBirdCrewController(crewManagerObject, BirdCrewController.BirdCrewType.BirdBoy, bootstrap);
            EnsureBirdCrewController(crewManagerObject, BirdCrewController.BirdCrewType.EvilBirdBoy, bootstrap);
            Object.DontDestroyOnLoad(crewManagerObject);
        }
        else
        {
            Object.DontDestroyOnLoad(runCrewManager.gameObject);

            if (startsNewRun)
            {
                runCrewManager.ResetRunCrew();
            }

            if (runCrewManager.GetComponent<PaulCrewController>() == null)
            {
                runCrewManager.gameObject.AddComponent<PaulCrewController>();
            }

            if (runCrewManager.GetComponent<CleanUpCrewController>() == null)
            {
                runCrewManager.gameObject.AddComponent<CleanUpCrewController>();
            }

            EnsureBirdCrewController(runCrewManager.gameObject, BirdCrewController.BirdCrewType.BirdBoy, bootstrap);
            EnsureBirdCrewController(runCrewManager.gameObject, BirdCrewController.BirdCrewType.EvilBirdBoy, bootstrap);
        }

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
    }

    private static TreasureChestRunBootstrap FindBootstrap()
    {
        return Object.FindFirstObjectByType<TreasureChestRunBootstrap>();
    }

    private static void EnsureBirdCrewController(GameObject owner, BirdCrewController.BirdCrewType crewType, TreasureChestRunBootstrap bootstrap)
    {
        if (owner == null)
        {
            return;
        }

        BirdCrewController configuredSceneController = FindConfiguredSceneController(crewType);
        if (configuredSceneController != null)
        {
            ApplyBootstrapPrefabs(configuredSceneController, crewType, bootstrap);
            return;
        }

        BirdCrewController playerShipController = FindPlayerShipController(crewType);
        if (playerShipController != null)
        {
            ApplyBootstrapPrefabs(playerShipController, crewType, bootstrap);
            return;
        }

        BirdCrewController ownerController = FindControllerOnOwner(owner, crewType);
        if (ownerController != null)
        {
            ApplyBootstrapPrefabs(ownerController, crewType, bootstrap);
            return;
        }

        BirdCrewController controller = AddBirdCrewController(owner, crewType);
        ApplyBootstrapPrefabs(controller, crewType, bootstrap);
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
