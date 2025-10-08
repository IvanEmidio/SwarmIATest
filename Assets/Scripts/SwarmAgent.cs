using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(CharacterController))]
public class SwarmAgent : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float detectionRadius = 10;
    [SerializeField] private float separationDistance = 2f;

    [Header("Weights")]

    [SerializeField] private float separationWeight = 2f;
    [SerializeField] private float cohesionWeight = 1f;
    [SerializeField] private float playerWeight = 3f;

    private Transform player;
    private CharacterController controller;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        controller = GetComponent<CharacterController>();
    }

    
    void Update()
    {
        if(player == null) return;

        Vector3 separation = GetSeparation();
        Vector3 cohesion = GetCohesion();
        Vector3 playerChase = (player.position - transform.position).normalized;

        Vector3 moveDirection = 
            (separation * separationWeight) +
            (cohesion * cohesionWeight) +
            (playerChase * playerWeight);

        controller.SimpleMove(moveDirection.normalized * speed);


        if(moveDirection.sqrMagnitude > 0.01f)
            transform.forward = moveDirection.normalized;
    }

    Vector3 GetSeparation()
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

    Vector3 GetCohesion()
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
}
