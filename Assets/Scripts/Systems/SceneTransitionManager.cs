using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public FadeScreen fadeScreen;
    public static SceneTransitionManager singleton;

    private void Awake()
    {
        if (singleton != null && singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        singleton = this;
        DontDestroyOnLoad(gameObject); //Persist across scene reloads
    }

    public void GoToScene(int sceneIndex)
    {
        StartCoroutine(GoToSceneRoutine(sceneIndex));
    }

    private IEnumerator GoToSceneRoutine(int sceneIndex)
    {
        if (fadeScreen != null)
            fadeScreen.FadeOut();
        yield return new WaitForSeconds(fadeScreen != null ? fadeScreen.fadeDuration : 0f);
        SceneManager.LoadScene(sceneIndex);
    }

    public void GoToSceneAsync(int sceneIndex)
    {
        StartCoroutine(GoToSceneAsyncRoutine(sceneIndex));
    }

    private IEnumerator GoToSceneAsyncRoutine(int sceneIndex)
    {
        if (fadeScreen != null)
            fadeScreen.FadeOut();

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        float timer = 0f;
        while (timer <= (fadeScreen != null ? fadeScreen.fadeDuration : 0f) && !operation.isDone)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        operation.allowSceneActivation = true;
    }
}
