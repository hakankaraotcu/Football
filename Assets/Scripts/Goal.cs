using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            switch (transform.tag)
            {
                case "RightGoal":
                    GameManager.GetInstance().OnHomeTeamScored();
                    break;
                case "LeftGoal":
                    GameManager.GetInstance().OnAwayTeamScored();
                    break;
            }
        }
    }
}
