using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class MinimapPing : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Time in seconds for one full ping cycle (fully visible, fading out, then repeating)")]
    #endregion Tooltip
    [SerializeField] private float pingInterval = 2f;

    #region Tooltip
    [Tooltip("Alpha over one ping cycle - x is time 0-1 through the cycle, y is alpha 0-1")]
    #endregion Tooltip
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private SpriteRenderer spriteRenderer;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = timer / pingInterval;

        if (t >= 1f)
        {
            timer = 0f;
            t = 0f;
        }

        Color color = spriteRenderer.color;
        color.a = fadeCurve.Evaluate(t);
        spriteRenderer.color = color;
    }
}