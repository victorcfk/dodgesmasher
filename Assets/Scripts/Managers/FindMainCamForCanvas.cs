using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindMainCamForCanvas : MonoBehaviour {

    [SerializeField]
    Canvas _targetCanvas;
    
	// Use this for initialization
	void Start () {
        _targetCanvas.worldCamera = GameManager.Instance.GameCamera;
    }
}
