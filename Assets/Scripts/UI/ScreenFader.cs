using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [Header("Transition Elements")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private Image logoImage;
    [SerializeField] private Image logoBackground;

    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetAlpha(0f); // Start fully transparent
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(1f));
    }

    public void FadeIn()
    {
        StartCoroutine(Fade(0f));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }

        if (logoImage != null)
        {
            Color c = logoImage.color;
            c.a = alpha;
            logoImage.color = c;
        }

        if (logoBackground != null)
        {
            Color c = logoBackground.color;
            c.a = alpha;
            logoBackground.color = c;
        }
    }
}

