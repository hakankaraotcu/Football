using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance;

    private GameManager()
    {

    }

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public static GameManager GetInstance()
    {
        return instance;
    }

    #endregion

    [Header("UI")]
    [SerializeField] TextMeshProUGUI redTeamScoreText;
    [SerializeField] TextMeshProUGUI blueTeamScoreText;
    [SerializeField] TextMeshProUGUI goalCelebrationText;

    [Header("Score")]
    private int redTeamScore;
    private int blueTeamScore;

    [Header("Spawn Points")]
    [SerializeField] private Transform blueTeamDefenceSpawn;
    [SerializeField] private Transform blueTeamStartSpawn;
    [SerializeField] private Transform redTeamStartSpawn;
    [SerializeField] private Transform redTeamDefenceSpawn;


    [Header("Attack and Defend Zones")]
    [SerializeField] private Transform redTeamAttack;
    [SerializeField] private Transform redTeamDefend;
    [SerializeField] private Transform blueTeamAttack;
    [SerializeField] private Transform blueTeamDefend;
    [SerializeField] private Transform redZone;
    [SerializeField] private Transform blueZone;

    [Header("Goal Zones")]
    [SerializeField] private Transform redTeamGoal;
    [SerializeField] private Transform blueTeamGoal;

    [Header("Characters")]
    [SerializeField] private GameObject[] characters;
    [SerializeField] private GameObject ballPrefab;

    private GameObject ball;
    public GameObject Ball { get => ball; private set => ball = value; }

    private List<Player> players;
    private List<GameObject> redTeamPlayers;
    private List<GameObject> blueTeamPlayers;

    public List<Player> Players { get; private set; }

    public BallState ballState;

    public bool controlable;

    public bool isRedTeamStarting;

    private void OnEnable()
    {
        StartGame();
    }

    private void OnDisable()
    {
        FinishGame();
    }

    private void StartGame()
    {
        ballState = BallState.Free;
        this.controlable = true;
        this.isRedTeamStarting = true;
        this.ball = Instantiate(this.ballPrefab);
        FindObjectOfType<CameraFollow>().SetBall(ball.transform);

        players = new List<Player>();
        redTeamPlayers = new List<GameObject>();
        blueTeamPlayers = new List<GameObject>();

        redTeamScore = 0;
        blueTeamScore = 0;

        List<GameObject> temp = characters.Where(x => true).ToList();

        for (int i = 0; i < 4; i++)
        {
            int randomIndex = Random.Range(0, temp.Count);
            GameObject character = Instantiate(temp[randomIndex]);
            character.GetComponent<Player>().MakePlayerAI();
            character.GetComponent<Player>().team = Team.Red;
            character.GetComponent<Player>().attackZone = redTeamAttack.GetChild(i);
            character.GetComponent<Player>().defendZone = redTeamDefend.GetChild(i);
            character.GetComponent<Player>().enemyZone = blueZone;
            character.GetComponent<Player>().opponentGoal = blueTeamGoal;
            character.GetComponent<Player>().teamGoal = redTeamGoal;
            redTeamPlayers.Add(character);
            players.Add(character.GetComponent<Player>());
            if (i == 0) character.GetComponent<Player>().MakePlayerHuman();
            temp.RemoveAt(randomIndex);
        }

        for (int i = 0; i < 4; i++)
        {
            int randomIndex = Random.Range(0, temp.Count);
            GameObject character = Instantiate(temp[randomIndex]);
            character.GetComponent<Player>().MakePlayerAI();
            character.GetComponent<Player>().team = Team.Blue;
            character.GetComponent<Player>().attackZone = blueTeamAttack.GetChild(i);
            character.GetComponent<Player>().defendZone = blueTeamDefend.GetChild(i);
            character.GetComponent<Player>().enemyZone = redZone;
            character.GetComponent<Player>().opponentGoal = redTeamGoal;
            character.GetComponent<Player>().teamGoal = blueTeamGoal;
            blueTeamPlayers.Add(character);
            players.Add(character.GetComponent<Player>());
            temp.RemoveAt(randomIndex);
        }

        foreach (Player player in players)
        {
            player.GetComponent<Player>().allyPlayers = players.Where(x => x != player && player.team == x.team).ToArray();
        }

        StartRedTeam();
    }

    private void FinishGame()
    {
        Debug.Log("Finishing the Game");
        Destroy(FindObjectOfType<Ball>().gameObject);
        foreach (Player player in players)
        {
            Destroy(player.gameObject);
        }
    }

    public IEnumerator OnRedTeamScored()
    {
        isRedTeamStarting = false;

        foreach (GameObject player in redTeamPlayers)
        {
            player.GetComponent<Animator>().Play("Dance", player.GetComponent<Player>().ANIMATION_LAYER_DANCING, 0f);
            player.GetComponent<Animator>().SetLayerWeight(player.GetComponent<Player>().ANIMATION_LAYER_DANCING, 1f);
        }
        foreach (GameObject player in blueTeamPlayers)
        {
            player.GetComponent<Animator>().Play("Disappointed", player.GetComponent<Player>().ANIMATION_LAYER_DISAPPOINTED, 0f);
            player.GetComponent<Animator>().SetLayerWeight(player.GetComponent<Player>().ANIMATION_LAYER_DISAPPOINTED, 1f);
        }

        yield return new WaitForSeconds(2f);

        foreach (GameObject player in redTeamPlayers)
        {
            player.GetComponent<Animator>().SetLayerWeight(player.GetComponent<Player>().ANIMATION_LAYER_DANCING, 0f);
        }
        foreach (GameObject player in blueTeamPlayers)
        {
            player.GetComponent<Animator>().SetLayerWeight(player.GetComponent<Player>().ANIMATION_LAYER_DISAPPOINTED, 0f);
        }

        redTeamScore++;
        redTeamScoreText.text = "R:" + redTeamScore.ToString();
    }

    public IEnumerator OnBlueTeamScored()
    {
        isRedTeamStarting = true;

        foreach (GameObject player in blueTeamPlayers)
        {
            player.GetComponent<Animator>().Play("Dance", player.GetComponent<Player>().ANIMATION_LAYER_DANCING, 0f);
            player.GetComponent<Animator>().SetLayerWeight(player.GetComponent<Player>().ANIMATION_LAYER_DANCING, 1f);
        }
        foreach (GameObject player in redTeamPlayers)
        {
            player.GetComponent<Animator>().Play("Disappointed", player.GetComponent<Player>().ANIMATION_LAYER_DISAPPOINTED, 0f);
            player.GetComponent<Animator>().SetLayerWeight(player.GetComponent<Player>().ANIMATION_LAYER_DISAPPOINTED, 1f);
        }

        yield return new WaitForSeconds(2f);

        foreach (GameObject player in blueTeamPlayers)
        {
            player.GetComponent<Animator>().SetLayerWeight(player.GetComponent<Player>().ANIMATION_LAYER_DANCING, 0f);
        }
        foreach (GameObject player in redTeamPlayers)
        {
            player.GetComponent<Animator>().SetLayerWeight(player.GetComponent<Player>().ANIMATION_LAYER_DISAPPOINTED, 0f);
        }

        blueTeamScore++;
        blueTeamScoreText.text = "B:" + blueTeamScore.ToString();
    }

    public void Out(Player lastTouchedPlayer)
    {
        if (lastTouchedPlayer.team == Team.Red)
        {
            Invoke(nameof(StartBlueTeam), 1f);
        }
        else
        {
            Invoke(nameof(StartRedTeam), 1f);
        }
    }

    public void Throw(Player lastTouchedPlayer, Vector3 throwPoint)
    {
        if (lastTouchedPlayer.team == Team.Red)
        {
            Invoke(nameof(StartBlueTeam), 1f);
        }
        else
        {
            Invoke(nameof(StartRedTeam), 1f);
        }
    }

    public void ScoreGoal(GoalSituation situation)
    {
        switch (situation)
        {
            case GoalSituation.RedTeam:
                StartCoroutine(OnRedTeamScored());
                break;
            case GoalSituation.BlueTeam:
                StartCoroutine(OnBlueTeamScored());
                break;
            case GoalSituation.BlueOwn:
                StartCoroutine(OnRedTeamScored());
                break;
            case GoalSituation.RedOwn:
                StartCoroutine(OnBlueTeamScored());
                break;
        }
        OnGoalScored();
    }

    public void PlacePlayers()
    {
        Debug.Log("Placing Players");
        if (isRedTeamStarting) StartRedTeam();
        else StartBlueTeam();

        Destroy(ball);
        SpawnBall();
        ballState = BallState.Free;
    }

    public void StartRedTeam()
    {
        Debug.Log("Starting Red Team");
        foreach (GameObject player in redTeamPlayers)
        {
            player.GetComponent<Player>().MakePlayerAI();
            player.transform.position = redTeamStartSpawn.GetChild(redTeamPlayers.IndexOf(player)).position;
            player.transform.LookAt(Vector3.zero);
            if (redTeamPlayers.IndexOf(player) == 0) player.GetComponent<Player>().MakePlayerHuman();
        }

        foreach (GameObject player in blueTeamPlayers)
        {
            player.GetComponent<Player>().MakePlayerAI();
            player.transform.position = blueTeamDefenceSpawn.GetChild(blueTeamPlayers.IndexOf(player)).position;
            player.transform.LookAt(Vector3.zero);
        }
    }

    public void StartBlueTeam()
    {
        Debug.Log("Starting Blue Team");
        foreach (GameObject player in blueTeamPlayers)
        {
            player.GetComponent<Player>().MakePlayerAI();
            player.transform.position = blueTeamStartSpawn.GetChild(blueTeamPlayers.IndexOf(player)).position;
            player.transform.LookAt(Vector3.zero);
        }

        foreach (GameObject player in redTeamPlayers)
        {
            player.GetComponent<Player>().MakePlayerAI();
            player.transform.position = redTeamDefenceSpawn.GetChild(redTeamPlayers.IndexOf(player)).position;
            player.transform.LookAt(Vector3.zero);
            if (redTeamPlayers.IndexOf(player) == 0) player.GetComponent<Player>().MakePlayerHuman();
        }
    }

    private void SpawnBall()
    {
        ball = Instantiate(ballPrefab);
        FindObjectOfType<CameraFollow>().SetBall(ball.transform);
    }

    public Player FindClosestPlayerToBall()
    {
        float minDistance = Mathf.Infinity;
        Player closestPlayer = null;
        foreach (Player player in players)
        {
            float distance = Vector3.Distance(player.transform.position, ball.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPlayer = player;
            }
        }
        return closestPlayer.GetComponent<Player>();
    }

    public async void OnGoalScored()
    {
        controlable = false;

        Sequence sequence = DOTween.Sequence();
        goalCelebrationText.gameObject.SetActive(true);
        goalCelebrationText.text = "GOAL";
        sequence.Append(goalCelebrationText.transform.DOLocalMoveX(0, 1f)).AppendInterval(1f);
        sequence.Append(goalCelebrationText.transform.DOLocalMoveX(1500, 1f));
        await sequence.Play().AsyncWaitForCompletion();
        goalCelebrationText.gameObject.SetActive(false);
        goalCelebrationText.transform.localPosition = new Vector3(-1500, 0, 0);
        controlable = true;
        PlacePlayers();
    }
}
