using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private bool isStickToPlayer;
    
    [SerializeField] private Transform transformPlayer;
    private Transform ballStickPosition;

    private float speed;

    private Vector3 previousLocation;

    private Player player;

    public bool IsStickToPlayer { get => isStickToPlayer; set => isStickToPlayer = value; }

    private void Start()
    {
        ballStickPosition = transformPlayer.GetChild(8);
        player = transformPlayer.GetComponent<Player>();
    }

    private void Update()
    {
        if (!isStickToPlayer)
        {
            float distanceToPlayer = Vector3.Distance(transformPlayer.position, transform.position);
            if (distanceToPlayer < 1f)
            {
                IsStickToPlayer = true;
                player.Ball = this;
            }
        }
        else
        {
            Vector2 currentLocation = new Vector2(transform.position.x, transform.position.z);
            speed = Vector2.Distance(currentLocation, previousLocation) / Time.deltaTime;
            transform.position = ballStickPosition.position;
            transform.Rotate(new Vector3(transformPlayer.right.x, 0, transformPlayer.right.z), speed, Space.World);
            previousLocation = currentLocation;
        }
    }
}
