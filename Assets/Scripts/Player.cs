using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;

    private const float gravity = -9.81f;
    private float speed;

    private Vector3 direction;
    private Vector3 rotation;
    private Vector3 previousLocation;
    
    private Transform ballStickPoint;

    private bool isBallSticked;

    private GameObject ball;

    private Animator animator;
    private CharacterController controller;

    private void Awake()
    {
        isBallSticked = false;
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        ball = GameObject.FindWithTag("Ball");
        ballStickPoint = transform.GetChild(8);
    }

    private void Update()
    {
        MoveAndLook();
        //PickUpBallAndRotate();
        Shoot();
    }

    private void MoveAndLook()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        bool isWalking = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);
        bool isRunning = Input.GetKey(KeyCode.E) && isWalking;
        animator.SetBool("Walk", isWalking);
        animator.SetBool("Run", isRunning);
        direction = new Vector3(-vertical, gravity, horizontal);
        rotation = new Vector3(-vertical, 0, horizontal);
        Vector3 gravityVector = new Vector3(0, gravity, 0);
        controller.Move(direction * Time.deltaTime * (Input.GetKey(KeyCode.E) ? runningSpeed : walkingSpeed));
        transform.LookAt(transform.position + rotation);
    }

    private void Shoot()
    {

    }

    private void PickUpBallAndRotate()
    {
        if (!isBallSticked)
        {
            float distance = Vector3.Distance(transform.position, ball.transform.position);

            if (distance < 0.5f)
            {
                ball.transform.position = ballStickPoint.position;
                ball.transform.parent = ballStickPoint;
                isBallSticked = true;
            }
        }
    }
}
