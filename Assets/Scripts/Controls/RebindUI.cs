using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class RebindUI : MonoBehaviour
{
    [SerializeField] private InputActionReference inputAction;
    [SerializeField]  private TextMeshProUGUI bindingText;
    [SerializeField] private Button rebindButton;
    [SerializeField] private int bindingIndex = 0;
    
    private InputActionRebindingExtensions.RebindingOperation rebindOperation;

    private void OnEnable()
    {
        UpdateBindingDisplay();
        rebindButton.onClick.AddListener(StartRebind);
    }

    private void OnDisable()
    {
        rebindButton.onClick.RemoveListener(StartRebind);
        rebindOperation?.Dispose();
    }

    private void UpdateBindingDisplay()
    {
        bindingText.text = inputAction.action.GetBindingDisplayString(bindingIndex);
    }

    private void StartRebind()
    {
        inputAction.action.Disable();
        bindingText.text = "...";

        rebindOperation = inputAction.action.PerformInteractiveRebinding(bindingIndex)
            .WithExpectedControlType("Button")
            .WithControlsExcluding("Mouse/position")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                UpdateBindingDisplay();
                operation.Dispose();
                inputAction.action.Enable();
                SaveRebinds();
                
                // Reload onto the gameplay instance
                string json = PlayerPrefs.GetString("rebinds", string.Empty);
                GameManager.Instance.GetPlayer().playerControl.ReloadBindings(json);
            })
            .OnCancel(operation =>
            {
                UpdateBindingDisplay();
                operation.Dispose();
                inputAction.action.Enable();
            })
            .Start();
    }

    private void SaveRebinds()
    {
        var asset = inputAction.action.actionMap.asset;
        PlayerPrefs.SetString("rebinds", asset.SaveBindingOverridesAsJson());
    }
}