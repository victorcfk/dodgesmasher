using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;
using System;

public abstract class BaseSpawner<T> : MonoBehaviour where T: UnityEngine.Component 
{

    //=================================

    [FormerlySerializedAs("OnAllEnemiesCreated")]
    public Action OnAllObjectsCreated;

    [SerializeField]
    [FormerlySerializedAs("_onAllEnemiesCreatedSceneEvent")]
    protected UnityEvent _onAllObjectsCreatedSceneEvent;

    //=================================

    [FormerlySerializedAs("OnAllEnemiesKilled")]
    public Action OnAllObjectsDestroyed;

    [SerializeField]
    [FormerlySerializedAs("_onAllEnemiesKilledSceneEvent")]
    protected UnityEvent _onAllObjectsDestroyedSceneEvent;

    //=================================

    [SerializeField]
    protected Transform[] _listOfTransforms;

    [Space]
    [FormerlySerializedAs("EnemiesToSpawn")]
    [SerializeField]
    protected List<T> _objectsToSpawn;

    [InspectorReadOnly]
    [SerializeField]
    protected List<T> _objectsAlreadySpawned;
    public List<T> ObjectsAlreadySpawned
    {
        get
        {
            return _objectsAlreadySpawned;
        }
    }
    public float PercentageOfObjectsLeft
    {
        get
        {
            return _objectsAlreadySpawned.Count/_objectsToSpawn.Count;
        }
    }

    public float TimeBetweenSpawns = 0.3f;


    [SerializeField]
    [FormerlySerializedAs("InitialDelaySecs")]
    public float InitialDelayBeforeSpawning = 0;

    protected enum SpawningStatus
    {
        SPAWNING,
        ALL_SPAWNED_SOME_ALIVE,
        ALL_SPAWNED_ALL_DEAD
    }
    [SerializeField]
    protected SpawningStatus _spawningStatus;

    public void SpawnObjects()
    {
        _spawningStatus = SpawningStatus.SPAWNING;

        _objectsAlreadySpawned.Clear();

        //we assume the number of locations provided are unique and more than the number of enemies
        Transform[] locations = _listOfTransforms;

        for (int i = 0; i < _objectsToSpawn.Count; i++)
        {
            ObjectSpawningBehaviour(_objectsToSpawn[i], InitialDelayBeforeSpawning + TimeBetweenSpawns * i, locations[i].position, Quaternion.identity);
        }
    }

    public void SpawnObjects(List<Transform> inSpawnLocations, List<T> inSpawnPrefabs, bool inTakePrefabRotation)
    {
        _spawningStatus = SpawningStatus.SPAWNING;

        _objectsToSpawn = inSpawnPrefabs.ShallowCopy();
        _objectsAlreadySpawned.Clear();
        
        for (int i = 0; i < inSpawnPrefabs.Count; i++)
        {
            Quaternion rotationToUse = inTakePrefabRotation ? (inSpawnPrefabs[i].transform.rotation) : (Quaternion.identity);

            ObjectSpawningBehaviour(inSpawnPrefabs[i], InitialDelayBeforeSpawning + TimeBetweenSpawns * i, inSpawnLocations[i].position, rotationToUse);
        }
    }

    protected void ObjectSpawningBehaviour(T inObjectToSpawn, float inDelaySeconds, Vector3 inSpawnPosition, Quaternion inSpawnRotation)
    {
        StartCoroutine(
            SpawnObject(
                inObjectToSpawn,
                inDelaySeconds,
                inSpawnPosition,
                inSpawnRotation,
                OnObjectCreated,
                OnObjectDestroyed));
    }

    protected abstract IEnumerator SpawnObject
        (T obj,
        float inDelaySeconds,
        Vector3 spawnLoc,
        Quaternion spawnRot,
        Action<T> OnCreateCallback,
        Action<T> OnDestroyCallback);
   

    void OnObjectCreated(T obj)
    {
        _objectsAlreadySpawned.Add(obj);

        OnObjectCreatedSubAction(obj);

        if (_spawningStatus == SpawningStatus.SPAWNING && _objectsAlreadySpawned.Count == _objectsToSpawn.Count)
        {
            _spawningStatus = SpawningStatus.ALL_SPAWNED_SOME_ALIVE;

            if (OnAllObjectsCreated != null)
                OnAllObjectsCreated();

            _onAllObjectsCreatedSceneEvent.Invoke();
        }
    }

    protected abstract void OnObjectCreatedSubAction(T obj);

    protected bool allMyObjectsSpawned
    {
        get
        {
            return _objectsAlreadySpawned.Count == _objectsToSpawn.Count;
        }
    }

    protected bool allMyObjectsDestroyed
    {
        get
        {
            return _objectsAlreadySpawned.Count == 0;
        }
    }

    protected void OnObjectDestroyed(T obj)
    {
        _objectsAlreadySpawned.Remove(obj);

        OnObjectDestroyedSubAction(obj);

        if (_spawningStatus == SpawningStatus.ALL_SPAWNED_SOME_ALIVE && _objectsAlreadySpawned.Count == 0)
        {
            _spawningStatus = SpawningStatus.ALL_SPAWNED_ALL_DEAD;

            if (OnAllObjectsDestroyed != null)
                OnAllObjectsDestroyed();

            _onAllObjectsDestroyedSceneEvent.Invoke();
        }
    }
    protected abstract void OnObjectDestroyedSubAction(T obj);

    public void DestroyObject(T inObj)
    {
        if (_objectsAlreadySpawned.Contains(inObj))
            OnObjectDestroyed(inObj);
    }

    public void DestroyAllSpawnedObjects()
    {
        foreach (T obj in _objectsAlreadySpawned.ShallowCopy())
        {
            DestroyObject(obj);
        }
    }
}