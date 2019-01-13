using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;
using System;

public class ObjectSpawner : BaseSpawner<Transform>
{

    protected override IEnumerator SpawnObject(Transform obj, float inDelaySeconds, Vector3 spawnLoc, Quaternion spawnRot, Action<Transform> OnCreateCallback, Action<Transform> OnDestroyCallback)
    {
        yield return new WaitForSeconds(inDelaySeconds);

        Transform SpawnedObject = Instantiate(obj, spawnLoc, spawnRot);

        OnCreateCallback(SpawnedObject);
    }

    protected override void OnObjectCreatedSubAction(Transform obj)
    {
        //do nothing
    }

    protected override void OnObjectDestroyedSubAction(Transform obj)
    {
        Destroy(obj);
    }
}