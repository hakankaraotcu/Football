using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private bool isSticked;

    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform playerBallTransform;

    private float speed;
    
    private Vector3 previousLocation;

    private void Update()
    {

        if (!isSticked)
        {
            playerTransform = GameObject.FindWithTag("Active Player").transform;
            playerBallTransform = playerTransform.GetChild(8);
            float distanceToPlayer = Vector3.Distance(playerTransform.position, transform.position);
            if (distanceToPlayer < 0.5f)
            {
                isSticked = true;
            }
        }
        else
        {
            Vector2 currentPosition = new Vector2(transform.position.x, transform.position.z);
            speed = Vector2.Distance(currentPosition, previousLocation) / Time.deltaTime;
            transform.position = playerBallTransform.position;
            transform.Rotate(new Vector3(playerTransform.right.x, 0, playerTransform.right.z), speed, Space.World);
            previousLocation = currentPosition;
        }
    }
}
