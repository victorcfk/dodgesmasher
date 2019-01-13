using UnityEngine.UI;
using UnityEngine;

public class ToggleImageNegative : MonoBehaviour {

    [SerializeField]
    Image _imageToToggleNegative;

	// Use this for initialization
	public void ToggleImage (bool inToggleOff)
    {
        _imageToToggleNegative.enabled = !inToggleOff;

    }
	
}
