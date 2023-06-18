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

    [SerializeField] TextMeshProUGUI redTeamScoreText;
    [SerializeField] TextMeshProUGUI blueTeamScoreText;
    [SerializeField] TextMeshProUGUI goalCelebrationText;

    private int redTeamScore;
    private int blueTeamScore;

    [SerializeField] private Transform redTeamSpawn;
    [SerializeField] private Transform blueTeamSpawn;
    [SerializeField] private Transform redTeamAttack;
    [SerializeField] private Transform redTeamDefend;
    [SerializeField] private Transform blueTeamAttack;
    [SerializeField] private Transform blueTeamDefend;

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
        this.ball = Instantiate(this.ballPrefab);
        FindObjectOfType<CameraFollow>().SetBall(ball.transform);

        players = new List<Player>();
        redTeamPlayers = new List<GameObject>();
        blueTeamPlayers = new List<GameObject>();

        redTeamScore = 0;
        blueTeamScore = 0;

        List<GameObject> temp = characters.Where(x => true).ToList();

        for (int i = 0; i < redTeamSpawn.childCount; i++)
        {
            int randomIndex = Random.Range(0, temp.Count);
            GameObject character = Instantiate(temp[randomIndex], redTeamSpawn.GetChild(i).position, Quaternion.identity);
            character.transform.LookAt(Vector3.zero);
            character.GetComponent<Player>().MakePlayerAI();
            character.GetComponent<Player>().team = Team.Red;
            character.GetComponent<Player>().attackZone = redTeamAttack.GetChild(i);
            character.GetComponent<Player>().defendZone = redTeamDefend.GetChild(i);
            redTeamPlayers.Add(character);
            players.Add(character.GetComponent<Player>());
            if (i == 0) character.GetComponent<Player>().MakePlayerHuman();
            temp.RemoveAt(randomIndex);
        }

        for (int i = 0; i < blueTeamSpawn.childCount; i++)
        {
            int randomIndex = Random.Range(0, temp.Count);
            GameObject character = Instantiate(temp[randomIndex], blueTeamSpawn.GetChild(i).position, Quaternion.identity);
            character.transform.LookAt(Vector3.zero);
            character.GetComponent<Player>().MakePlayerAI();
            character.GetComponent<Player>().team = Team.Blue;
            character.GetComponent<Player>().attackZone = blueTeamAttack.GetChild(i);
            character.GetComponent<Player>().defendZone = blueTeamDefend.GetChild(i);
            blueTeamPlayers.Add(character);
            players.Add(character.GetComponent<Player>());
            temp.RemoveAt(randomIndex);
        }

        foreach (Player player in players)
        {
            player.GetComponent<Player>().allyPlayers = players.Where(x => x != player && player.team == x.team).ToArray();
        }
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

    public void OnRedTeamScored()
    {
        redTeamScore++;
        redTeamScoreText.text = "R:" + redTeamScore.ToString();
    }

    public void OnBlueTeamScored()
    {
        blueTeamScore++;
        blueTeamScoreText.text = "B:" + blueTeamScore.ToString();
    }

    public void Out(Player lastTouchedPlayer)
    {
        Invoke(nameof(PlacePlayers), 1f);
    }

    public void Throw(Player lastTouchedPlayer, Vector3 throwPoint)
    {
        Invoke(nameof(PlacePlayers), 1f);
    }

    public void ScoreGoal(Player lastTouchedPlayer)
    {
        switch (lastTouchedPlayer.team)
        {
            case Team.Red:
                OnRedTeamScored();
                break;
            case Team.Blue:
                OnBlueTeamScored();
                break;
        }

        StartCoroutine(OnGoalScored());
    }

    public void PlacePlayers()
    {
        foreach (GameObject player in redTeamPlayers)
        {
            player.GetComponent<Player>().MakePlayerAI();
            player.transform.position = redTeamSpawn.GetChild(redTeamPlayers.IndexOf(player)).position;
            player.transform.LookAt(Vector3.zero);
            if (redTeamPlayers.IndexOf(player) == 0) player.GetComponent<Player>().MakePlayerHuman();
        }

        foreach (GameObject player in blueTeamPlayers)
        {
            player.transform.position = blueTeamSpawn.GetChild(blueTeamPlayers.IndexOf(player)).position;
            player.transform.LookAt(Vector3.zero);
        }

        Destroy(ball);
        SpawnBall();
        ballState = BallState.Free;
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

    public IEnumerator OnGoalScored()
    {
        controlable = false;
        goalCelebrationText.gameObject.SetActive(true);
        goalCelebrationText.text = "GOAL";
        goalCelebrationText.transform.DOLocalMoveX(0, 1f);
        yield return new WaitForSeconds(2f);
        goalCelebrationText.transform.DOLocalMoveX(1500, 1f);
        yield return new WaitForSeconds(1.5f);
        goalCelebrationText.gameObject.SetActive(false);
        goalCelebrationText.transform.localPosition = new Vector3(-1500, 0, 0);
        PlacePlayers();
        controlable = true;
    }
}
