using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class RebindControls : MonoBehaviour
{
    [Header("References")]
    [SerializeField] InputActionAsset actionAsset = null;
    [SerializeField] TMPro.TextMeshProUGUI actionText = null;
    [SerializeField] PlayerInput controls = null;
    [SerializeField] List<InputActionReference> actionReferences = new List<InputActionReference>();
    [SerializeField] List<TMPro.TextMeshProUGUI> textReferences = new List<TMPro.TextMeshProUGUI>();
    [SerializeField] UnityEvent OnDoneRebind = null;
    [SerializeField] UnityEvent OnWaitRebind = null;
    [SerializeField] UnityEvent OnStartRebind = null;

    InputActionRebindingExtensions.RebindingOperation rebindingOperation = null;

    Dictionary<string, InputActionReference> actions = new Dictionary<string, InputActionReference>();
    Dictionary<string, TMPro.TextMeshProUGUI> text = new Dictionary<string, TMPro.TextMeshProUGUI>();

    [HideInInspector] public string currentRebindActionName = "";

    private void Start()
    {
        //Get persistant key binds
        var rebinds = PlayerPrefs.GetString("rebinds");
        bool successfulyRead = !string.IsNullOrEmpty(rebinds);
        if (successfulyRead)
            actionAsset.LoadBindingOverridesFromJson(rebinds);

        //Add actions to list, remove Player/ from the beginning
        actionReferences.ForEach(x => actions.Add(x.name.Remove(0, 7), x));
        textReferences.ForEach(x => text.Add(x.transform.parent.name, x));

        if (successfulyRead)
            foreach (var item in text)
            {
                currentRebindActionName = item.Key;
                item.Value.text = GetControlString();
            }


        OnDoneRebind.AddListener(delegate ()
        {
            controls.SwitchCurrentActionMap("Player");

            rebindingOperation?.Dispose();
            rebindingOperation = null;
            var rebinds = actionAsset.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("rebinds", rebinds);
        });
        OnWaitRebind.AddListener(delegate ()
        {
            text[currentRebindActionName].text = GetControlString();
            OnDoneRebind.Invoke();
        });
        OnStartRebind.AddListener(() => actionText.text = currentRebindActionName);
    }
    List<InputActionRebindingExtensions.RebindingOperation> rebinds = new List<InputActionRebindingExtensions.RebindingOperation>();
    public void Rebind(string actionName)
    {
        if (actions[actionName].action.bindings[0].isComposite)
        {
            currentRebindActionName = actionName;
            controls.SwitchCurrentActionMap("Empty");
            //hard coded for now...
            var wasd = actions[actionName].action.ChangeCompositeBinding("WASD");
            List<string> l = new List<string>();
            l.Add("Up");
            l.Add("Down");
            l.Add("Left");
            l.Add("Right");
            rebinds.Clear();
            l.ForEach(x =>
            {
                var part = wasd.NextPartBinding(x);

                var rp = actions[actionName].action.PerformInteractiveRebinding()
                    .WithTargetBinding(part.bindingIndex)
                    .OnMatchWaitForAnother(0.1f)
                    .OnComplete(operation =>
                    {
                        Debug.Log(x);
                        rebinds[l.IndexOf(x)].Dispose();
                        if (l.IndexOf(x) == l.Count - 1)
                            controls.SwitchCurrentActionMap("Player");
                    })
                    .Start();
                rebinds.Add(rp);
            });

            return;
        }
        if (rebindingOperation != null)
            return;
        currentRebindActionName = actionName;
        OnStartRebind.Invoke();
        controls.SwitchCurrentActionMap("Empty");



        rebindingOperation = actions[actionName].action.PerformInteractiveRebinding()
        .OnMatchWaitForAnother(0.1f)
        .OnComplete(operation => OnWaitRebind.Invoke())
        .Start();
    }

    public string GetControlString()
    {
        return "[" + InputControlPath.ToHumanReadableString(
                actions[currentRebindActionName].action.bindings[0].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice
            ) + "]";
    }
}
