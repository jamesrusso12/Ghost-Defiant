using System.Collections;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{
    [Header("Fade Settings")]
    public bool fadeOnStart = true;
    public float fadeDuration = 2f;
    public Color fadeColor = Color.black;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public string colorPropertyName = "_Color";

    private Renderer rend;
    private Material fadeMaterial;
    private Coroutine fadeCoroutine;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("FadeScreen requires a Renderer component.");
            return;
        }

        fadeMaterial = rend.material;
        rend.enabled = false;

        if (fadeOnStart)
            FadeIn();
    }

    public void FadeIn()
    {
        StartFade(1f, 0f);
    }

    public void FadeOut()
    {
        StartFade(0f, 1f);
    }

    private void StartFade(float alphaIn, float alphaOut)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(alphaIn, alphaOut));
    }

    private IEnumerator FadeRoutine(float alphaIn, float alphaOut)
    {
        rend.enabled = true;

        float timer = 0f;
        while (timer <= fadeDuration)
        {
            if (fadeMaterial != null)
            {
                Color newColor = fadeColor;
                newColor.a = Mathf.Lerp(alphaIn, alphaOut, fadeCurve.Evaluate(timer / fadeDuration));
                fadeMaterial.SetColor(colorPropertyName, newColor);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (fadeMaterial != null)
        {
            Color finalColor = fadeColor;
            finalColor.a = alphaOut;
            fadeMaterial.SetColor(colorPropertyName, finalColor);
        }

        if (alphaOut == 0)
            rend.enabled = false;

        fadeCoroutine = null;
    }
}
