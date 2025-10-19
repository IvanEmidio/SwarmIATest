using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(CharacterController))]
public class SwarmAgent : MonoBehaviour
{
    [Header("Attributes")]

    [SerializeField] private float speed;
    [SerializeField] private float detectionRadius = 10;
    [SerializeField] private float separationDistance = 2f;
    [SerializeField] private float awarenessRadius = 25f;

    [Header("Weights")]

    [SerializeField] private float separationWeight = 2f;
    [SerializeField] private float cohesionWeight = 1f;
    [SerializeField] private float playerWeight = 3f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;

    private float lastAttackTime = 0f;
    private Transform player;
    private CharacterController controller;
    private Vector3 currentVelocity;
    private bool isAggro = false;

    private enum MoveStyle {Direct, FlankLeft, FlankRight}
    private MoveStyle moveStyle;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        controller = GetComponent<CharacterController>();
        speed = Random.Range(2f, 6f);

        if(speed > 4f) moveStyle = MoveStyle.Direct;
        else moveStyle = (Random.value > 0.5f) ? MoveStyle.FlankLeft : MoveStyle.FlankRight;
        
    }

    
    void Update()
    {
    
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            player = p.transform;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        if (!isAggro && dist <= awarenessRadius) isAggro = true;
        if (!isAggro) return;

    
        Vector3 separation = GetSeparation();
        Vector3 cohesion = GetCohesion();

    
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 playerRight = player.right;

   
        float angle = Vector3.SignedAngle(player.forward, (transform.position - player.position).normalized, Vector3.up);

        Vector3 flankDir = toPlayer;

    
        if (Mathf.Abs(angle) >= 20f)
        {
            float flankStrength = Mathf.Clamp01(Mathf.Abs(angle) / 90f);
            flankDir += (angle > 0 ? playerRight : -playerRight) * flankStrength * 0.8f;
        }

    
        Vector3 noise = new Vector3(
            Mathf.PerlinNoise(Time.time * 1.5f + transform.GetInstanceID(), transform.position.x) - 0.5f,
            0,
            Mathf.PerlinNoise(Time.time * 1.5f + transform.GetInstanceID(), transform.position.z) - 0.5f
        ) * 0.5f;

    
        flankDir += noise;
        flankDir.Normalize();

    
        Vector3 moveDirection =
            (separation * separationWeight) +
            (cohesion * cohesionWeight) +
            (flankDir * playerWeight);

            Vector3 desiredDir = (moveDirection.sqrMagnitude > 0.0001f) ? moveDirection.normalized : transform.forward;

            currentVelocity = Vector3.Lerp(currentVelocity, desiredDir * speed, Time.deltaTime * 3f);

            if(dist <= attackRange)
            {
                currentVelocity = Vector3.zero;
            }
            else
            {
                controller.SimpleMove(currentVelocity);
            }
        
        Vector3 flatForward = new Vector3(currentVelocity.x, 0, currentVelocity.z);        
        if (flatForward.sqrMagnitude > 0.01f)
            transform.forward = Vector3.Lerp(transform.forward, flatForward.normalized, Time.deltaTime * 10f);

        if(dist <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            AttackPlayer();
        }

    }


    public Vector3 GetSeparation()
    {
        Vector3 force = Vector3.zero;
        Collider[] neighbors = Physics.OverlapSphere(transform.position, separationDistance);

        foreach (Collider col in neighbors)
        {
            if(col.gameObject != this.gameObject && col.CompareTag("Enemy"))
            {
                force += (transform.position - col.transform.position);
            }
        }

        return force.normalized;
    }

    public Vector3 GetCohesion()
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        Collider[] neighbors = Physics.OverlapSphere(transform.position, detectionRadius);

        foreach (Collider col in neighbors)
        {
            if(col.gameObject != this.gameObject && col.CompareTag("Enemy"))
            {
                center += col.transform.position;
                count++;
            }
        }
        if(count == 0) return Vector3.zero;

        center /= count;
        return(center - transform.position).normalized;
    }

    public void AttackPlayer()
    {
        Debug.Log($"{name} atacou o player!");
    }

}
