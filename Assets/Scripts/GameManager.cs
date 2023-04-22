using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

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

    [SerializeField] TextMeshProUGUI homeTeamScoreText;
    [SerializeField] TextMeshProUGUI awayTeamScoreText;

    private int homeTeamScore;
    private int awayTeamScore;

    [SerializeField] private Transform redTeamSpawn;
    [SerializeField] private Transform blueTeamSpawn;

    [SerializeField] private GameObject[] characters;
    [SerializeField] private GameObject ball;

    private Player[] players;

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
        homeTeamScore = 0;
        awayTeamScore = 0;

        List<GameObject> temp = characters.Where(x => true).ToList();

        for(int i = 0; i < redTeamSpawn.childCount; i++)
        {
            int randomIndex = Random.Range(0, temp.Count);
            GameObject character = Instantiate(temp[randomIndex], redTeamSpawn.GetChild(i).position, Quaternion.identity);
            character.GetComponent<Player>().MakePlayerAI();
            character.GetComponent<Player>().team = Team.Red;
            if (i == 0) character.GetComponent<Player>().MakePlayerHuman();
            temp.RemoveAt(randomIndex);
        }

        for (int i = 0; i < blueTeamSpawn.childCount; i++)
        {
            int randomIndex = Random.Range(0, temp.Count);
            GameObject character = Instantiate(temp[randomIndex], blueTeamSpawn.GetChild(i).position, Quaternion.identity);
            character.GetComponent<Player>().MakePlayerAI();
            character.GetComponent<Player>().team = Team.Blue;
            temp.RemoveAt(randomIndex);
        }

        players = FindObjectsOfType<Player>();

        foreach (Player player in players)
        {
            player.GetComponent<Player>().players = players.Where(x => x != player && player.team == x.team).ToArray();
        }

        GameObject ball = Instantiate(this.ball);
        FindObjectOfType<CameraFollow>().SetBall(ball.transform);
        
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

    public void OnHomeTeamScored()
    {
        homeTeamScore++;
        homeTeamScoreText.text = "H:" + homeTeamScore.ToString();
    }

    public void OnAwayTeamScored()
    {
        awayTeamScore++;
        awayTeamScoreText.text = "A:" + awayTeamScore.ToString();
    }

}
