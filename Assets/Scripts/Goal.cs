using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            GoalSituation goalSituation = GoalSituation.RedTeam;
            Debug.Log("Goal");
            Physics.IgnoreCollision(other, GetComponent<Collider>());
            Team team = other.GetComponent<Ball>().LastPlayer.team;
            LayerMask goal = gameObject.layer;
            if(team == Team.Red && goal == LayerMask.NameToLayer("Red Goal")) goalSituation = GoalSituation.RedOwn;
            if(team == Team.Blue && goal == LayerMask.NameToLayer("Red Goal")) goalSituation = GoalSituation.BlueTeam;
            if(team == Team.Red && goal == LayerMask.NameToLayer("Blue Goal")) goalSituation = GoalSituation.RedTeam;
            if(team == Team.Blue && goal == LayerMask.NameToLayer("Blue Goal")) goalSituation = GoalSituation.BlueOwn;
            GameManager.GetInstance().ScoreGoal(goalSituation);
            other.GetComponent<Ball>().SlowDown();
        }
    }
}
