using UnityEngine;
using UnityEngine.SceneManagement;

public static class TreasureChestRunBootstrap
{
    private static readonly string[] RunSceneNames = { "MainSea", "Stage2", "Stage3" };
    private static bool wasInRunScene;

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

        RunCrewManager runCrewManager = Object.FindFirstObjectByType<RunCrewManager>();
        if (runCrewManager == null)
        {
            GameObject crewManagerObject = new("Run Crew Manager");
            runCrewManager = crewManagerObject.AddComponent<RunCrewManager>();
            crewManagerObject.AddComponent<PaulCrewController>();
            crewManagerObject.AddComponent<CleanUpCrewController>();
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
