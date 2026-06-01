using UnityEngine;
using UnityEngine.SceneManagement;

public static class TreasureChestRunBootstrap
{
    private static readonly string[] RunSceneNames = { "MainSea", "Stage2", "Stage3" };

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
        if (!IsRunScene(scene.name))
        {
            return;
        }

        RunCrewManager runCrewManager = Object.FindFirstObjectByType<RunCrewManager>();
        if (runCrewManager == null)
        {
            GameObject crewManagerObject = new("Run Crew Manager");
            crewManagerObject.AddComponent<RunCrewManager>();
        }
        else
        {
            runCrewManager.ResetRunCrew();
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
