using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpUIController : MonoBehaviour
{
    public static HelpUIController Instance;

    [SerializeField]
    private TMPro.TextMeshProUGUI _helpText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }
    }

    public void ShowHelpText(string text)
    {
        _helpText.text = text;
    }

    public void SetHelpTextEnabled(bool enabled)
    {
        _helpText.enabled = enabled;
    }
}