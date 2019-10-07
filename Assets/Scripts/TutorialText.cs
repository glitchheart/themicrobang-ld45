using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialText : MonoBehaviour
{
    private bool _enabled;

    public void ToggleTutorial()
    {
        _enabled = !_enabled;
        gameObject.SetActive(_enabled);
    }
}
