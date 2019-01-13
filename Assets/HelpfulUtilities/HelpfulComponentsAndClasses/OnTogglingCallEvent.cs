using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;

public class OnTogglingCallEvent : MonoBehaviour {

    [SerializeField]
    [FormerlySerializedAs("OnEnableCallBacks")]
    UnityEvent _onEnableCallBacks;

    [SerializeField]
    [FormerlySerializedAs("OnDisableCallBacks")]
    UnityEvent _onDisableCallBacks;

    [SerializeField]
    [FormerlySerializedAs("OnDestroyCallBacks")]
    UnityEvent _onDestroyCallBacks;

    [SerializeField]
    bool _OutputCallbackInfo;

    void OnEnable () {
        if (_onEnableCallBacks != null && _onEnableCallBacks.GetPersistentEventCount() > 0)
        {
            _onEnableCallBacks.Invoke();

            if (_OutputCallbackInfo)
                _onEnableCallBacks.DebugListOfAllMethodsToString();
        }
    }
	
	void OnDisable () {
        if (_onDisableCallBacks != null && _onDisableCallBacks.GetPersistentEventCount() > 0)
        {
            _onDisableCallBacks.Invoke();

            if (_OutputCallbackInfo)
                _onDisableCallBacks.DebugListOfAllMethodsToString();
        }
    }

    void OnApplicationQuit()
    {
        _onDisableCallBacks = null;
        _onDestroyCallBacks = null;
        _onEnableCallBacks = null;
    }

    void OnDestroy()
    {
        if (_onDestroyCallBacks != null && _onDestroyCallBacks.GetPersistentEventCount() > 0)
        {
            _onDestroyCallBacks.Invoke();

            if (_OutputCallbackInfo)
                _onDestroyCallBacks.DebugListOfAllMethodsToString();
        }
    }
}