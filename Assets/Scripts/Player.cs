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
    [SerializeField] public float runningSpeed;

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

    public Player[] allyPlayers;
    public Player[] opponentPlayers;

    private Animator animator;
    private CharacterController controller;

    private GameObject activenessArrow;

    public Transform defendZone;
    public Transform attackZone;

    public GameObject speedPowerEffect;
    public GameObject speedEffect;
    public GameObject shootPowerEffect;
    public GameObject shootEffect;

    [SerializeField] float maxPower;
    [SerializeField] float currentPower;

    public ShootingBar shootingBar;

    public Ball Ball { get => ball; set => ball = value; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

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
        allyPlayers = FindObjectsOfType<Player>().Where(p => p != this && p.team == this.team).ToArray();
        opponentPlayers = FindObjectsOfType<Player>().Where(p => p != this && p.team != this.team).ToArray();
    }

    private void Update()
    {
        switch (playerType)
        {
            case PlayerType.Human:
                if (GameManager.GetInstance().controlable)
                {
                    if (speedEffect != null)
                    {
                        Transform effectTransform = speedEffect.transform;
                        effectTransform.position = gameObject.transform.position;
                    }
                    else if (shootEffect != null)
                    {
                        Transform effectTransform = shootEffect.transform;
                        effectTransform.position = gameObject.transform.position;
                    }
                    if (canWalk) MoveAndLook();
                    Shoot();
                    Pass();
                    GetBall();
                }
                break;
            case PlayerType.AI:
                switch (GameManager.GetInstance().ballState)
                {
                    case BallState.Free:
                        Player nearestPlayer = GameManager.GetInstance().FindClosestPlayerToBall();
                        if (nearestPlayer == this)
                        {
                            AIGoBall();
                            AIGetBall();
                        }
                        break;
                    case BallState.RedTeam:
                        if (team == Team.Red)
                        {
                            AIAttackZone();
                        }
                        else
                        {
                            AIDefendZone();
                        }
                        break;
                    case BallState.BlueTeam:
                        if (team == Team.Blue)
                        {
                            AIAttackZone();
                        }
                        else
                        {
                            AIDefendZone();
                        }
                        break;
                }
                break;
        }
    }

    private void GetBall()
    {
        float distanceToBall = Vector3.Distance(transform.position, FindBall().transform.position);
        if (distanceToBall < 1f)
        {
            FindBall().GetComponent<Ball>().IsStickToPlayer = true;
            FindBall().GetComponent<Ball>().TransformPlayer = transform;
            GameManager.GetInstance().ballState = team == Team.Red ? BallState.RedTeam : BallState.BlueTeam;
            this.ball = FindBall().GetComponent<Ball>();
            ball.GetComponent<TrailRenderer>().enabled = false;
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
                if (currentPower < maxPower)
                {
                    currentPower++;
                    shootingForce++;
                }
                //shootingForce += Time.deltaTime * shootingPower;
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                currentPower = 0;
                StartCoroutine(ShootAnimationEvent());
            }
            GameObject.Find("ShootBar").GetComponent<ShootingBar>().SetPower(currentPower);
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
                    Vector3 direction = DirectionToPlayer(targetPlayer);

                    this.MakePlayerAI();
                    ball.IsStickToPlayer = false;
                    ball.GetComponent<Rigidbody>().AddForce(direction * passingPower);
                    GameManager.GetInstance().ballState = BallState.Free;
                    ball.TransformPlayer = targetPlayer.transform;
                    ball = null;
                }
            }
        }
    }

    private Player FindPlayerInDirection(Vector3 direction)
    {
        return allyPlayers.OrderBy(p => Vector3.Angle(direction, DirectionToPlayer(p))).FirstOrDefault();
    }

    private Vector3 DirectionToPlayer(Player p)
    {
        return Vector3.Normalize(p.transform.position - ball.transform.position);
    }


    private Vector3 DirectionToBall()
    {
        return Vector3.Normalize(GameManager.GetInstance().Ball.transform.position - transform.position);
    }

    public void MakePlayerAI()
    {
        playerType = PlayerType.AI;
        gameObject.tag = "Deactive Player";
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        activenessArrow.SetActive(false);
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    public void MakePlayerHuman()
    {
        playerType = PlayerType.Human;
        gameObject.tag = "Active Player";
        gameObject.layer = LayerMask.NameToLayer("Active Player");
        activenessArrow.SetActive(true);
    }

    private IEnumerator ShootAnimationEvent()
    {
        canWalk = false;
        animator.Play("Shoot", ANIMATION_LAYER_SHOOTING, 0f);
        animator.SetLayerWeight(ANIMATION_LAYER_SHOOTING, 1f);
        yield return new WaitForSeconds(0.4f);
        Vector3 target = transform.forward + transform.up * 0.07f;
        ball.GetComponent<TrailRenderer>().enabled = true;
        ball.GetComponent<Rigidbody>().AddForce(target * shootingForce, ForceMode.Impulse);
        shootingForce = 0f;
        ball.IsStickToPlayer = false;
        ball = null;
        yield return new WaitForSeconds(0.5f);
        canWalk = true;
    }

    private GameObject FindBall()
    {
        return GameManager.GetInstance().Ball;
    }

    public void PowerUpSpeed(float multiplier, float duration)
    {
        StartCoroutine(IncreaseDecreaseSpeed(multiplier, duration));
    }

    IEnumerator IncreaseDecreaseSpeed(float multiplier, float duration)
    {
        speedEffect = Instantiate(speedPowerEffect, transform.position, transform.rotation);

        runningSpeed *= multiplier;

        yield return new WaitForSeconds(duration);

        Destroy(speedEffect);

        runningSpeed /= multiplier;
    }

    public void PowerUpShoot(float multiplier, float duration)
    {
        StartCoroutine(IncreaseDecreaseShoot(multiplier, duration));
    }

    IEnumerator IncreaseDecreaseShoot(float multiplier, float duration)
    {
        shootEffect = Instantiate(shootPowerEffect, transform.position, transform.rotation);

        shootingPower *= multiplier;

        yield return new WaitForSeconds(duration);

        Destroy(shootEffect);

        shootingPower /= multiplier;
    }

    public Vector3 RandomPointInBound(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            0,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    private void AIGoBall()
    {
        animator.SetBool("Walk", true);
        Vector3 direction = DirectionToBall().normalized;
        direction.y = gravity;
        Vector3 rotation = DirectionToBall();
        controller.Move(direction * Time.deltaTime * walkingSpeed);
        transform.LookAt(transform.position + rotation);
    }

    private void AIGetBall()
    {
        float distanceToBall = Vector3.Distance(transform.position, FindBall().transform.position);
        if (distanceToBall < 1f)
        {
            AITakeBall();
        }
    }

    private void AITakeBall()
    {
        FindBall().GetComponent<Ball>().IsStickToPlayer = true;
        FindBall().GetComponent<Ball>().TransformPlayer = transform;
        GameManager.GetInstance().ballState = team == Team.Red ? BallState.RedTeam : BallState.BlueTeam;
        this.ball = FindBall().GetComponent<Ball>();
    }


    private void AIAttackZone()
    {
        animator.SetBool("Walk", true);
        Vector3 direction = attackZone.GetComponent<Collider>().bounds.center;
        transform.position = Vector3.MoveTowards(transform.position, direction, 0.05f * walkingSpeed);
        transform.LookAt(direction);
        if (Vector3.Distance(transform.position, direction) < 0.1f)
        {
            animator.SetBool("Walk", false);
        }
    }

    private void AIDefendZone()
    {
        animator.SetBool("Walk", true);
        Vector3 direction = defendZone.GetComponent<Collider>().bounds.center;
        transform.position = Vector3.MoveTowards(transform.position, direction, 0.05f * walkingSpeed);
        transform.LookAt(direction);
        if (Vector3.Distance(transform.position, direction) < 0.1f)
        {
            animator.SetBool("Walk", false);
        }

        if (defendZone.GetComponent<Zone>().activePlayer != null)
        {
            if (defendZone.GetComponent<Zone>().activePlayer != this && defendZone.GetComponent<Zone>().activePlayer.GetComponent<Player>().team != this.team && defendZone.GetComponent<Zone>().activePlayer.GetComponent<Player>().ball != null)
            {
                AIPressOpponent(defendZone.GetComponent<Zone>().activePlayer.gameObject);
            }
        }
    }

    private void AIPressOpponent(GameObject player)
    {
        animator.SetBool("Walk", true);
        Vector3 direction = player.transform.position;
        transform.LookAt(direction);
        transform.position = Vector3.MoveTowards(transform.position, direction, 0.07f * walkingSpeed);
        if (Vector3.Distance(transform.position, direction) < 0.1f)
        {
            animator.SetBool("Walk", false);
        }

        if (Vector3.Distance(transform.position, direction) < 0.5f)
        {
            if (player.GetComponent<Player>().ball != null && Random.Range(0, 100) < 10)
            {
                player.GetComponent<Player>().ball = null;
                AITakeBall();
            }
        }
    }
}
