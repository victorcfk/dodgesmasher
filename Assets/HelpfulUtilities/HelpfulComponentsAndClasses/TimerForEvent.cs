using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerForEvent : MonoBehaviour {

    [SerializeField]
    UnityEvent OnCountDownReached;

    [SerializeField]
    float TimeToDoStuff = 5;
    float timer;

    [SerializeField]
    bool resetSelf = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        timer += Time.deltaTime;

        if (timer >= TimeToDoStuff)
        {
            OnCountDownReached.Invoke();

            if (resetSelf)
            {
                timer = 0;
                enabled = false;
            }

        }
	}
}
