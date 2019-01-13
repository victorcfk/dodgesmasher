using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class TutorialManager : MonoBehaviour {

    [SerializeField]
    UnityEvent DragGuide;
    [SerializeField]
    UnityEvent AimAndSlowDownGuide;
    [SerializeField]
    UnityEvent EnergyGuide;
    
    bool _alreadyDisplayedDrag = false;
    [ContextMenu("Drag")]
    public void DisplayDraggingGuide()
    {
        if (_alreadyDisplayedDrag)
        {
            return;
        }
        else
        {
            _alreadyDisplayedDrag = true;
            DragGuide.Invoke();
        }
    }

    bool _alreadyDisplayAimAndSlowGuide = false;
    [ContextMenu("Aim")]
    public void DisplayAimAndSlowGuide()
    {
        if (_alreadyDisplayAimAndSlowGuide)
        {
            return;
        }
        else
        {
            _alreadyDisplayAimAndSlowGuide = true;
            AimAndSlowDownGuide.Invoke();
        }
    }

    bool _alreadyDisplayEnergyGuide = false;
    [ContextMenu("Energy")]
    public void DisplayEnergyGuide()
    {
        if (_alreadyDisplayEnergyGuide)
        {
            return;
        }
        else
        {
            _alreadyDisplayEnergyGuide = true;
            EnergyGuide.Invoke();
        }
    }

    public void CompleteTutorialLevel()
    {
        GameManager.Instance.TutorialComplete();
    }
}
