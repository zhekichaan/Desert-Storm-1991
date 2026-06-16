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
        string json = PlayerPrefs.GetString("rebinds", string.Empty);
        if (!string.IsNullOrEmpty(json))
            inputAction.action.actionMap.asset.LoadBindingOverridesFromJson(json);

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
            .WithControlsExcluding("Mouse/position")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                ClearConflictingBindings();
                
                UpdateBindingDisplay();
                operation.Dispose();
                inputAction.action.Enable();
                SaveRebinds();
                
                string json = PlayerPrefs.GetString("rebinds", string.Empty);
                GameManager.Instance?.GetPlayer().playerControl.ReloadBindings(json);
            })
            .OnCancel(operation =>
            {
                UpdateBindingDisplay();
                operation.Dispose();
                inputAction.action.Enable();
            })
            .Start();
    }

    private void ClearConflictingBindings()
    {
        string newPath = inputAction.action.bindings[bindingIndex].effectivePath;

        foreach (var action in inputAction.action.actionMap.actions)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {                
                if (action == inputAction.action && i == bindingIndex) continue;
                
                if (action.bindings[i].isComposite) continue;
                
                if (action.bindings[i].effectivePath == newPath)
                {
                    action.ApplyBindingOverride(i, string.Empty);
                    RefreshAllRebindUIs();
                    return;
                }
            }
        }
    }

    private void RefreshAllRebindUIs()
    {
        foreach (var rebindUI in FindObjectsOfType<RebindUI>())
        {
            rebindUI.RefreshDisplay();
        }
    }

    public void RefreshDisplay()
    {
        UpdateBindingDisplay();
    }
    
    private void SaveRebinds()
    {
        var asset = inputAction.action.actionMap.asset;
        PlayerPrefs.SetString("rebinds", asset.SaveBindingOverridesAsJson());
    }
}