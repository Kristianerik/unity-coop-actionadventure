using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ControlSchemeText : MonoBehaviour
{
    [SerializeField] private InputActionReference action;
    [SerializeField] private string fullText = "Press {0} to hit";

    private TextMeshProUGUI _text;
    private PlayerInput _playerInput;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        _playerInput = GetComponentInParent<PlayerInput>();
        if (_playerInput != null)
            _playerInput.onControlsChanged += _ => UpdateText();
        UpdateText();
    }

private void UpdateText()
    {
        if (_text == null || action == null) return;

        string binding = action.action.GetBindingDisplayString();
        _text.text = string.Format(fullText, binding);
    }
}
