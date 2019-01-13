using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using System;

public class EnemySpawner : BaseSpawner<Enemy>
{
    [SerializeField]
    float _delayBeforeAIBecomesActive = 1;
    [Space]
    [SerializeField]
    [FormerlySerializedAs("EnemySpawnPsys")]
    ParticleSystem _objectPreSpawnPsys;

    public Action<int> OnEnemyDeathGainScore;

    protected override IEnumerator SpawnObject(Enemy inEnemy, float inDelaySeconds, Vector3 inSpawnPos, Quaternion inSpawnRot, Action<Enemy> inOnCreateCallback, Action<Enemy> inOnDestroyCallback)
    {
        yield return new WaitForSeconds(inDelaySeconds);

        //Dependent on the spawn position, orient the facing of the spawned object
        if(inSpawnPos.x<0)
        {
            inSpawnRot = Quaternion.LookRotation(Vector3.forward, Vector3.right);
        }
        else
        if (inSpawnPos.x > 0)
        {
            inSpawnRot = Quaternion.LookRotation(Vector3.forward,Vector3.left);
        }
        else
        if(inSpawnPos.y > 0)
        {
            inSpawnRot = Quaternion.LookRotation(Vector3.forward, Vector3.down);
        }
        else
        {
            inSpawnRot = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }

        Enemy spawnedObject = Instantiate(inEnemy, inSpawnPos, inSpawnRot);

        inOnCreateCallback(spawnedObject);

        spawnedObject.OnDeath += inOnDestroyCallback;

        spawnedObject.gameObject.SetActive(false);

        yield return new WaitForSeconds(_objectPreSpawnPsys.main.duration);

        spawnedObject.gameObject.SetActive(true);

        yield return new WaitForSeconds(_delayBeforeAIBecomesActive);

        //Hot fix for the scenario where the object gets destroyed before this
        if (spawnedObject != null)
        {
            AIShootBasic aib = spawnedObject.GetComponent<AIShootBasic>();

            if(aib != null)
                aib.enabled = true;
        }
    }

    protected override void OnObjectCreatedSubAction(Enemy inSpawnedEnemy)
    {
        if (_objectPreSpawnPsys)
            Instantiate(_objectPreSpawnPsys, inSpawnedEnemy.transform.position, inSpawnedEnemy.transform.rotation);
    }

    protected override void OnObjectDestroyedSubAction(Enemy inDestroyedEnemy)
    {
        Destroy(inDestroyedEnemy.gameObject);

        if(OnEnemyDeathGainScore != null)
            OnEnemyDeathGainScore(inDestroyedEnemy.ScoreOnKill);
    }
}