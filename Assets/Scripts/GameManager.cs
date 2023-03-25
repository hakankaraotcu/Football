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

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        homeTeamScore = 0;
        awayTeamScore = 0;
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
