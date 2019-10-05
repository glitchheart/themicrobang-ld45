using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpUIController : Controller<HelpUIController>
{
    [SerializeField]
    private TMPro.TextMeshProUGUI _helpText;

    public void ShowHelpText(string text)
    {
        _helpText.text = text;
    }

    public void SetHelpTextEnabled(bool enabled)
    {
        _helpText.enabled = enabled;
    }
}