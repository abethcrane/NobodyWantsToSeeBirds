using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Encoding = System.Text.Encoding;

[RequireComponent(typeof(Toggle))]
public class ToggleButtonSwitcher : MonoBehaviour
{
    [SerializeField]
    private string _toggledOffText;

    [SerializeField]
    private string _toggledOnText;

    [SerializeField]
    private TMPro.TextMeshProUGUI _text;

    [SerializeField]
    private bool _areStringsUnicode = false;

    private Dictionary<string, string> _unicodeDictionary = new Dictionary<string, string>
    {
        ["speaker"] = "\uf028",
        ["mute"] = "\uF6A9",
        ["play"] = "\uf04b",
        ["pause"] = "\uf04c",
    };

    private Toggle _toggle;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnToggled);

        if (_areStringsUnicode)
        {
            if (_unicodeDictionary.ContainsKey(_toggledOffText))
            {
                _toggledOffText = _unicodeDictionary[_toggledOffText];
            }

            if (_unicodeDictionary.ContainsKey(_toggledOnText))
            {
                _toggledOnText = _unicodeDictionary[_toggledOnText];
            }
        }

        // Set initial text value
        OnToggled(_toggle.isOn);
    }

    private void OnToggled(bool newValue)
    {
        _text.text = newValue ? _toggledOnText : _toggledOffText;
    }
}
