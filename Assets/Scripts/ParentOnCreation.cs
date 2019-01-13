using UnityEngine;

public class ParentOnCreation : MonoBehaviour {

    [SerializeField]
    GameObject[] allChildren;

	// Use this for initialization
	void Awake () {

        for(int i=0; i< allChildren.Length; i++)
            Instantiate(allChildren[i], transform.position, transform.rotation).transform.SetParent(this.transform);
	}
	
}
