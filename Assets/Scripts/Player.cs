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

    [Header("Range")]
    [SerializeField] private float scanRange;

    [Header("AI")]
    [SerializeField] private float aiPassPower;
    [SerializeField] private float aiShootPower;


    private const float gravity = -9.81f;
    private float shootingForce;

    private int aNIMATION_LAYER_SHOOTING = 1;
    private int aNIMATION_LAYER_DANCING = 2;
    private int aNIMATION_LAYER_DISAPPOINTED = 3;
    private int aNIMATION_LAYER_JOGGING = 4;

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
    public Transform enemyZone;
    public Transform opponentGoal;
    public Transform teamGoal;

    public GameObject speedPowerEffect;
    public GameObject speedEffect;
    public GameObject shootPowerEffect;
    public GameObject shootEffect;

    [SerializeField] float maxPower;
    [SerializeField] float currentPower;

    public ShootingBar shootingBar;

    public Ball Ball
    {
        get => ball;
        set
        {
            ball = value;
            ResetAnimator();
        }
    }

    public int ANIMATION_LAYER_SHOOTING { get => aNIMATION_LAYER_SHOOTING; private set => aNIMATION_LAYER_SHOOTING = value; }
    public int ANIMATION_LAYER_DANCING { get => aNIMATION_LAYER_DANCING; private set => aNIMATION_LAYER_DANCING = value; }
    public int ANIMATION_LAYER_DISAPPOINTED { get => aNIMATION_LAYER_DISAPPOINTED; private set => aNIMATION_LAYER_DISAPPOINTED = value; }
    public int ANIMATION_LAYER_JOGGING { get => aNIMATION_LAYER_JOGGING; private set => aNIMATION_LAYER_JOGGING = value; }

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
                    PassOrChangeActivePlayer();
                    GetBall();
                }
                break;
            case PlayerType.AI:
                if (GameManager.GetInstance().controlable)
                {
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

    private void PassOrChangeActivePlayer()
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
                    Debug.Log("Pass");
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
        else
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("Change Active Player");
                Player allyPlayer = allyPlayers.OrderBy(p => Vector3.Distance(p.transform.position, teamGoal.transform.position)).FirstOrDefault();
                this.MakePlayerAI();
                allyPlayer.MakePlayerHuman();
            }
        }
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
        animator.SetLayerWeight(ANIMATION_LAYER_JOGGING, 0f);
        animator.SetBool("Walk", true);
        Vector3 direction = attackZone.GetComponent<Collider>().bounds.center;
        transform.position = Vector3.MoveTowards(transform.position, direction, 0.05f * walkingSpeed);
        transform.LookAt(direction);
        if (Vector3.Distance(transform.position, direction) < 0.1f)
        {
            if (this.ball == null)
            {
                animator.SetBool("Walk", false);
                transform.LookAt(FindBall().transform.position + new Vector3(0, 0, 1f));
                bool forward = Random.Range(0, 2) == 0;
                bool backward = Random.Range(0, 2) == 0;
                animator.SetLayerWeight(ANIMATION_LAYER_JOGGING, 1f);
                animator.SetBool("ForwardJog", forward);
                animator.SetBool("BackwardJog", backward);
            }
            else
            {
                StartCoroutine(AIShootAnimationEvent());
            }
        }

        Collider[] opponent = Physics.OverlapSphere(transform.position, scanRange);
        opponent = opponent.Where(c => c.GetComponent<Player>() != null && c.GetComponent<Player>().team != team).ToArray();
        Debug.Log("I have " + opponent.Length + " opponents in my scan range");
        if (opponent.Length > 0 && this.ball != null)
        {
            switch (this.ball == null)
            {
                case true:
                    if (opponent[0].GetComponent<Player>().ball != null)
                    {
                        AIPressOpponent(opponent[0].gameObject);
                    }
                    break;
                case false:
                    GameObject teammate = FindTeammateInAttackZone();
                    float possiblities = Random.Range(0, 15);
                    if (possiblities >= 0 && possiblities < 1)
                    {
                        AIPassToTeammate(teammate);
                    }
                    else if (possiblities >= 1 && possiblities < 6)
                    {
                        if (IsInAttackZone()) StartCoroutine(AIShootAnimationEvent());
                    }
                    break;
            }
        }
    }

    private void AIShootToGoal()
    {
        Vector3 direction = (opponentGoal.transform.position - transform.position);
        this.MakePlayerAI();
        ball.IsStickToPlayer = false;
        ball.GetComponent<Rigidbody>().AddForce(direction * aiShootPower);
        GameManager.GetInstance().ballState = BallState.Free;
        ball = null;
    }

    private void AIPassToTeammate(GameObject teammate)
    {
        Player targetPlayer = teammate.GetComponent<Player>();

        Vector3 direction = (targetPlayer.transform.position - transform.position).normalized;

        this.MakePlayerAI();
        ball.IsStickToPlayer = false;
        ball.GetComponent<Rigidbody>().AddForce(direction * aiPassPower);
        GameManager.GetInstance().ballState = BallState.Free;
        ball = null;
    }

    private void AIDefendZone()
    {
        animator.SetLayerWeight(ANIMATION_LAYER_JOGGING, 0f);
        animator.SetBool("Walk", true);
        Vector3 direction = defendZone.GetComponent<Collider>().bounds.center;
        transform.position = Vector3.MoveTowards(transform.position, direction, 0.05f * walkingSpeed);
        transform.LookAt(direction);
        if (Vector3.Distance(transform.position, direction) < 0.1f)
        {
            animator.SetBool("Walk", false);

            transform.LookAt(FindBall().transform.position + new Vector3(0, 0, 1f));
            bool forward = Random.Range(0, 2) == 0;
            bool backward = Random.Range(0, 2) == 0;
            animator.SetLayerWeight(ANIMATION_LAYER_JOGGING, 1f);
            animator.SetBool("ForwardJog", forward);
            animator.SetBool("BackwardJog", backward);

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
        animator.SetLayerWeight(ANIMATION_LAYER_JOGGING, 0f);
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
            if (player.GetComponent<Player>().ball != null && Random.Range(0, 100) < 8)
            {
                player.GetComponent<Player>().ball = null;
                AITakeBall();
            }
        }
    }

    private IEnumerator AIShootAnimationEvent()
    {
        canWalk = false;
        animator.Play("Shoot", ANIMATION_LAYER_SHOOTING, 0f);
        animator.SetLayerWeight(ANIMATION_LAYER_SHOOTING, 1f);
        yield return new WaitForSeconds(0.4f);
        Vector3 direction = (opponentGoal.transform.position - transform.position);
        ball.GetComponent<Rigidbody>().AddForce(direction * aiShootPower);
        GameManager.GetInstance().ballState = BallState.Free;
        ball.GetComponent<TrailRenderer>().enabled = true;
        shootingForce = 0f;
        ball.IsStickToPlayer = false;
        ball = null;
        yield return new WaitForSeconds(0.5f);
        this.MakePlayerAI();
        canWalk = true;
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
        ResetAnimator();
        playerType = PlayerType.AI;
        gameObject.tag = "Deactive Player";
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        activenessArrow.SetActive(false);
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    public void MakePlayerHuman()
    {
        ResetAnimator();
        playerType = PlayerType.Human;
        gameObject.tag = "Active Player";
        gameObject.layer = LayerMask.NameToLayer("Active Player");
        activenessArrow.SetActive(true);
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

    private Vector3 FindClosestTeammate(Vector3 position)
    {
        return allyPlayers.OrderBy(p => Vector3.Distance(position, p.transform.position)).FirstOrDefault().transform.position;
    }

    private GameObject FindTeammateInAttackZone()
    {
        GameObject x = allyPlayers.OrderBy(p => Vector3.Distance(p.transform.position, transform.position)).FirstOrDefault().gameObject;
        return x;
    }

    private bool IsInAttackZone()
    {
        return enemyZone.GetComponent<Collider>().bounds.Contains(transform.position);
    }

    public void ResetAnimator()
    {
        animator.SetLayerWeight(ANIMATION_LAYER_SHOOTING, 0);
        animator.SetLayerWeight(ANIMATION_LAYER_DANCING, 0);
        animator.SetLayerWeight(ANIMATION_LAYER_DISAPPOINTED, 0);
        animator.SetLayerWeight(ANIMATION_LAYER_JOGGING, 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, scanRange);
    }
}
