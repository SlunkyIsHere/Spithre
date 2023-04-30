using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "EnemyConfiguration", menuName = "ScriptableObject/EnemyConfiguration")]
public class EnemyScriptableObject : ScriptableObject
{
    public Enemy prefab;
    public EnemyState DefaultState;
    
    public int Health = 100;
    public AttackScriptableObject AttackConfiguration;

    public float IdleLocationRadius = 4f;
    public float IdleMoveSpeedMultiplier = 0.5f;
    [Range(2, 10)] public int Waypoints = 4;
    public float LineOfSightDistance = 6f;
    public float FieldOfView = 90f;
    
    public float AIUpdateInterval = 0.1f;
    
    public float Acceleration = 8f;
    public float AngularSpeed = 120f;
    
    public int AreaMask = -1;
    public int AvoidancePriority = 50;
    public float BaseOffset = 0f;
    public float Height = 2f;
    public ObstacleAvoidanceType ObstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
    public float Radius = 0.5f;
    public float Speed = 3.5f;
    public float StoppingDistance = 0.5f;

    public void SetupEnemy(Enemy enemy)
    {
        enemy.agent.speed = Speed;
        enemy.agent.angularSpeed = AngularSpeed;
        enemy.agent.acceleration = Acceleration;
        enemy.agent.areaMask = AreaMask;
        enemy.agent.avoidancePriority = AvoidancePriority;
        enemy.agent.stoppingDistance = StoppingDistance;
        enemy.agent.radius = Radius;
        enemy.agent.height = Height;
        enemy.agent.baseOffset = BaseOffset;
        enemy.agent.obstacleAvoidanceType = ObstacleAvoidanceType;
      
        enemy.movement.UpdateSpeed = AIUpdateInterval;
        enemy.movement.DefaultState = DefaultState;
        enemy.movement.IdleMoveSpeedModifier = IdleMoveSpeedMultiplier;
        enemy.movement.IdleLocationRadius = IdleLocationRadius;
        enemy.movement.Waypoints = new Vector3[Waypoints];
        enemy.movement.lineOfSightChecker.fieldOfView = FieldOfView;
        enemy.movement.lineOfSightChecker.collider.radius = LineOfSightDistance;
        enemy.movement.lineOfSightChecker.lineOfSightLayers = AttackConfiguration.LineOfSightLayers;
      
        enemy.health = Health;

        AttackConfiguration.SetupEnemy(enemy);
    }
}
