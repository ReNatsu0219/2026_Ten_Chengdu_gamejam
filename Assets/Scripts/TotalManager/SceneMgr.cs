using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMgr : MonoSingleton<SceneMgr>
{
    [SerializeField] private SceneFader _sceneFaderPrefab;

    public void TransitionToLevelScene()
    {
        LoadLevel("LevelScene");
    }
    private async void LoadLevel(string sceneName)
    {
        SceneFader fade = Instantiate(_sceneFaderPrefab);

        if (sceneName != null)
        {
            await fade.FadeOut();
            await SceneManager.LoadSceneAsync(sceneName);
            await fade.FadeIn();
        }
    }
}
