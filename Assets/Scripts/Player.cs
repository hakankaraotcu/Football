using UnityEngine;
using System.Linq;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Type")]
    [SerializeField] private PlayerType playerType;

    [Header("Team")]
    [SerializeField] public Team team;

    [Header("Movement")]
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;

    [Header("Shooting")]
    [SerializeField] private float shootingPower;

    [Header("Passing")]
    [SerializeField] private float passingPower;

    private const float gravity = -9.81f;
    private float shootingForce;

    private const int ANIMATION_LAYER_SHOOTING = 1;

    private Vector3 direction;
    private Vector3 rotation;

    private bool canWalk;

    private Ball ball;

    public Player[] players;

    private Animator animator;
    private CharacterController controller;

    private GameObject activenessArrow;

    public Ball Ball { get => ball; set => ball = value; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        players = FindObjectsOfType<Player>().Where(p => p != this && p.team == this.team).ToArray();
        activenessArrow = transform.GetChild(9).gameObject;

        switch (playerType)
        {
            case PlayerType.Human:
                MakePlayerHuman();
                break;
            case PlayerType.AI:
                MakePlayerAI();
                break;
        }
    }

    private void Start()
    {
        canWalk = true;
    }

    private void Update()
    {
        switch (playerType)
        {
            case PlayerType.Human:
                if(canWalk) MoveAndLook();
                Shoot();
                Pass();
                break;
            case PlayerType.AI:
                break;
        }
    }

    private void MoveAndLook()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool isWalking = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);
        bool isRunning = Input.GetKey(KeyCode.E) && isWalking && canWalk;
        animator.SetBool("Walk", isWalking);
        animator.SetBool("Run", isRunning);
        direction = new Vector3(-vertical, gravity, horizontal);
        rotation = new Vector3(-vertical, 0, horizontal);
        controller.Move(direction * Time.deltaTime * (Input.GetKey(KeyCode.E) ? runningSpeed : walkingSpeed));
        transform.LookAt(transform.position + rotation);
    }

    private void Shoot()
    {
        if (ball != null)
        {
            if (Input.GetKey(KeyCode.D))
            {
                shootingForce += Time.deltaTime * shootingPower;
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                StartCoroutine(ShootAnimationEvent());
            }
        }
        else
        {
            animator.SetLayerWeight(ANIMATION_LAYER_SHOOTING, Mathf.Lerp(animator.GetLayerWeight(ANIMATION_LAYER_SHOOTING), 0f, Time.deltaTime));
        }
    }

    private void Pass()
    {
        if (ball != null)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 passDirection = new Vector3(-vertical, 0, horizontal);

            Debug.DrawLine(transform.position, transform.position + new Vector3(-vertical, 0, horizontal) * 10, Color.red);

            Player targetPlayer = FindPlayerInDirection(passDirection);

            if (targetPlayer != null)
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    Vector3 direction = DirectionTo(targetPlayer);

                    this.MakePlayerAI();
                    ball.IsStickToPlayer = false;
                    ball.GetComponent<Rigidbody>().AddForce(direction * passingPower);
                    ball.TransformPlayer = targetPlayer.transform;
                    ball = null;
                }
            }
        }
    }

    private Player FindPlayerInDirection(Vector3 direction)
    {
        return players.OrderBy(p => Vector3.Angle(direction, DirectionTo(p))).FirstOrDefault();
    }

    private Vector3 DirectionTo(Player p)
    {
        return Vector3.Normalize(p.transform.position - ball.transform.position);
    }

    public void MakePlayerAI()
    {
        playerType = PlayerType.AI;
        gameObject.tag = "Deactive Player";
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        activenessArrow.SetActive(false);
    }

    public void MakePlayerHuman()
    {
        playerType = PlayerType.Human;
        gameObject.tag = "Active Player";
        activenessArrow.SetActive(true);
    }

    private IEnumerator ShootAnimationEvent()
    {
        canWalk = false;
        animator.Play("Shoot", ANIMATION_LAYER_SHOOTING, 0f);
        animator.SetLayerWeight(ANIMATION_LAYER_SHOOTING, 1f);
        yield return new WaitForSeconds(0.4f);
        Vector3 target = transform.forward + transform.up * 0.07f;
        ball.GetComponent<Rigidbody>().AddForce(target * shootingForce, ForceMode.Impulse);
        shootingForce = 0f;
        ball.IsStickToPlayer = false;
        ball = null;
        yield return new WaitForSeconds(0.5f);
        canWalk = true;
    }
}
