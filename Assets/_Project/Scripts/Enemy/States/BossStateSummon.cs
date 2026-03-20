using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class BossStateSummon : EnemyStateBase
{
    
    private float _summonTimer = 0f;
    private float _summonDuration = 2f;
    private GameObject[] _minionPrefabs;
    private int _minionCount;
    private float _summonRadius = 5f;
    private bool _hasSummoned = false;

    public BossStateSummon(EnemyBase enemy, GameObject[] minionPrefabs, int minionCount) : base(enemy)
    {
        _minionPrefabs = minionPrefabs;
        _minionCount = minionCount;
    }

    public override void Enter()
    {
        _summonTimer = 0f;
        _hasSummoned = false;
        Enemy.Agent.isStopped = true;
        Enemy.EnemyAnimator?.SetTrigger("summon");
        Debug.Log($"{Enemy.gameObject.name} is summoning minions!");
    }

    public override void Update()
    {
        _summonTimer += Time.deltaTime;

        if (!_hasSummoned && _summonTimer >= _summonDuration * 0.5f)
        {
            _hasSummoned = true;
            SpawnMinions();
        }

        if (_summonTimer >= _summonDuration)
        {
            Enemy.ChangeState(Enemy.ChaseState);
        }
    }

    private void SpawnMinions()
    {
        if (_minionPrefabs == null || _minionPrefabs.Length == 0) return;

        for (int i = 0; i < _minionCount; i++)
        {
            // Spawn in circle around boss
            float angle = i * (360f / _minionCount);
            Vector3 SpawnOffset = new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad) * _summonRadius,
                0f,
                Mathf.Cos(angle * Mathf.Deg2Rad) * _summonRadius
            );

            Vector3 spawnPosition = Enemy.transform.position + SpawnOffset;

            // Pick random minion prefab
            GameObject minionPrefab = _minionPrefabs[Random.Range(0, _minionPrefabs.Length)];

            GameObject.Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
        }

        Debug.Log($"Spawned {_minionCount} minions!");
    }

    public override void Exit()
    {
        Enemy.Agent.isStopped = false;
    }

}
