using UnityEngine;

public class Ball : MonoBehaviour
{
    private bool isStickToPlayer;

    private Transform transformPlayer;
    private Transform ballStickPosition;

    private float speed;

    private Vector3 previousLocation;

    private Player currentPlayer;

    private Player lastPlayer;
    public Player LastPlayer { get => lastPlayer; private set => lastPlayer = value; }

    public bool IsStickToPlayer
    {
        get => isStickToPlayer;
        set
        {
            isStickToPlayer = value;
            if (!value)
            {
                lastPlayer = currentPlayer;
                currentPlayer = null;
            }
            else
            {
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
        }
    }

    public Transform TransformPlayer
    {
        get => transformPlayer;
        set
        {
            transformPlayer = value;
            ballStickPosition = transformPlayer.GetChild(8);
            currentPlayer = value.GetComponent<Player>();
            lastPlayer = currentPlayer;
            if(currentPlayer.team == Team.Red) currentPlayer.MakePlayerHuman();
            GameManager.GetInstance().ballState = currentPlayer.team == Team.Red ? BallState.RedTeam : BallState.BlueTeam;
        }
    }


    private void Update()
    {
        if(isStickToPlayer)
        {
            Vector2 currentLocation = new Vector2(transform.position.x, transform.position.z);
            speed = Vector2.Distance(currentLocation, previousLocation) / Time.deltaTime;
            transform.position = ballStickPosition.position;
            transform.Rotate(new Vector3(transformPlayer.right.x, 0, transformPlayer.right.z), speed, Space.World);
            previousLocation = currentLocation;
        }
    }
}
