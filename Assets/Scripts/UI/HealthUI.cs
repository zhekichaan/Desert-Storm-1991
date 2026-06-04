using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HealthUI : MonoBehaviour
{
    [SerializeField] private GameObject heartPrefab; // Drag your heart prefab here
    private Animator heartAnimator;
    private GameObject heartInstance;

    [SerializeField] private Image portraitRenderer;
    
    [SerializeField] private Sprite soldierPortrait;
    [SerializeField] private Sprite reconPortrait;

    private void Start()
    {
        // Spawn the heart prefab once
        heartInstance = Instantiate(heartPrefab, transform);
        heartAnimator = heartInstance.GetComponent<Animator>();

        string className = GameManager.Instance.playerDetails.playerPrefab.name;

        if (className == "Soldier")
            portraitRenderer.sprite = soldierPortrait;
        else if (className == "Recon")
            portraitRenderer.sprite = reconPortrait;
    }

    private void OnEnable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(HealthEvent healthEvent, HealthEventArgs args)
    {
        UpdateHeartAnimation(args.healthPercent);
    }

    private string currentState = "Full";

    private void UpdateHeartAnimation(float healthPercent)
    {
        string newState = "Full";

        if (healthPercent < 0.25f) newState = "Quarter";
        else if (healthPercent < 0.5f) newState = "Half";
        else if (healthPercent < 0.75f) newState = "ThreeQuarter";

        // Only trigger Animator if the state changes
        if (newState == currentState) return;

        heartAnimator.SetBool("Quarter", false);
        heartAnimator.SetBool("Half", false);
        heartAnimator.SetBool("ThreeQuarter", false);
        heartAnimator.SetBool("Full", false);

        heartAnimator.SetBool(newState, true);

        currentState = newState;
    }
}