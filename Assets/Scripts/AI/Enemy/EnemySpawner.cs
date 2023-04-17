using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public Transform player;
    public int numberOfEnemiesToSpawn = 5;
    public float spawnDelay = 1f;
    public List<Enemy> EnemyPrefabs = new List<Enemy>();
    public SpawnMethod spawnMethod = SpawnMethod.RoundRobin;

    private NavMeshTriangulation _triangulation;
    private Dictionary<int, ObjectPool> EnemyObjectPools = new Dictionary<int, ObjectPool>();

    private void Awake()
    {
        for (int i = 0; i < EnemyPrefabs.Count; i++)
        {
            EnemyObjectPools.Add(i, ObjectPool.CreateInstance(EnemyPrefabs[i], numberOfEnemiesToSpawn));
        }
    }

    private void Start()
    {
        _triangulation = NavMesh.CalculateTriangulation();

        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        WaitForSeconds Wait = new WaitForSeconds(spawnDelay);
        
        int SpawnedEnemies = 0;
        
        while (SpawnedEnemies < numberOfEnemiesToSpawn)
        {
            if (spawnMethod == SpawnMethod.RoundRobin)
            {
                SpawnRoundRobinEnemy(SpawnedEnemies);
            }
            else if (spawnMethod == SpawnMethod.Random)
            {
                SpawnRandomEnemy();
            }

            SpawnedEnemies++;
            
            yield return Wait;
        }
    }
    
    private void SpawnRoundRobinEnemy(int SpawnedEnemies)
    {
        int SpawnIndex = SpawnedEnemies % EnemyPrefabs.Count;

        DoSpawnEnemy(SpawnIndex);
    }
    
    private void SpawnRandomEnemy()
    {
        DoSpawnEnemy(Random.Range(0, EnemyPrefabs.Count));
    }

    private void DoSpawnEnemy(int SpawnIndex)
    {
        PoolableObject poolableObject = EnemyObjectPools[SpawnIndex].GetObject();

        if (poolableObject != null)
        {
            Enemy enemy = poolableObject.GetComponent<Enemy>();
            
            int VertexIndex = Random.Range(0, _triangulation.vertices.Length);
            NavMeshHit hit;
            
            if (NavMesh.SamplePosition(_triangulation.vertices[VertexIndex], out hit, 2f, -1))
            {
                enemy.agent.Warp(hit.position);
                enemy.movement.Target = player;
                enemy.agent.enabled = true;
                enemy.movement.StartChasing();
            }
            else
            {
                Debug.LogError($"Unable to place NavMeshAgent at {_triangulation.vertices[VertexIndex]}");
            }
        } 
        else
        {
            Debug.LogError($"Unable to fetch enemy of type {SpawnIndex} from pool.)");
        }
    }
    
    public enum SpawnMethod
    {
        RoundRobin,
        Random
    }
}
