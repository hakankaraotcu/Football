using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;

    [Header("Shooting")]
    [SerializeField] private float shootingPower;

    private const float gravity = -9.81f;
    private float speed;
    private float shootingForce;
    private float timeShot = -1;

    private const int ANIMATION_LAYER_SHOOTING = 1;

    private Vector3 direction;
    private Vector3 rotation;
    private Vector3 previousLocation;

    private Transform ballStickPoint;

    private bool isBallSticked;

    private Ball ball;

    private Animator animator;
    private CharacterController controller;

    public Ball Ball { get => ball; set => ball = value; }

    private void Awake()
    {
        isBallSticked = false;
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        //ball = GameObject.FindWithTag("Ball");
        ballStickPoint = transform.GetChild(8);

        shootingForce = 0;
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

    private void PickUpBallAndRotate()
    {
        if (!isBallSticked)
        {
            float distance = Vector3.Distance(transform.position, ball.transform.position);

            if (distance < 1.5f)
            {
                ball.transform.position = ballStickPoint.position;
                ball.transform.parent = ballStickPoint;
                isBallSticked = true;

                ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
                ball.GetComponent<Rigidbody>().angularVelocity= Vector3.zero;
            }
        }

        else
        {
            Vector2 currentPosition = new Vector2(transform.position.x, transform.position.z);
            speed = Vector2.Distance(currentPosition, previousLocation) / Time.deltaTime;
            transform.Rotate(new Vector3(transform.right.x, 0, transform.right.z), speed, Space.World);
            previousLocation = currentPosition;
        }
    }

    private void Shoot()
    {
        if (ball != null)
        {
            if (Input.GetKey(KeyCode.D))
            {
                Debug.Log("Shooting force is :" + shootingForce);
                shootingForce += Time.deltaTime * shootingPower;
                timeShot = Time.time;
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                Debug.Log("Ball shooted with force :" + shootingForce);
                animator.Play("Shoot", ANIMATION_LAYER_SHOOTING, 0f);
                animator.SetLayerWeight(ANIMATION_LAYER_SHOOTING, 1f);
                Vector3 target = transform.forward + transform.up;
                ball.GetComponent<Rigidbody>().AddForce(target * shootingForce, ForceMode.Impulse);

                shootingForce = 0f;
                ball.IsStickToPlayer = false; 
                ball = null;
            }
        }
        else
        {
            animator.SetLayerWeight(ANIMATION_LAYER_SHOOTING, Mathf.Lerp(animator.GetLayerWeight(ANIMATION_LAYER_SHOOTING), 0f, Time.deltaTime * 10f));
        }

    }
}
