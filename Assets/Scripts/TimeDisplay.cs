using UnityEngine;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class TimeDisplay : MonoBehaviour
{
    private TMPro.TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TMPro.TextMeshProUGUI>();
    }

    private void Update()
    {
        if (Main.Instance.IsGameActive)
        {
            _text.text = "Seconds: " + Time.timeSinceLevelLoad.ToString("0.0");
        }
    }
}
