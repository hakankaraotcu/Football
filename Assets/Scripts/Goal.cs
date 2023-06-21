using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Debug.Log("Goal");
            Physics.IgnoreCollision(other, GetComponent<Collider>());
            GameManager.GetInstance().ScoreGoal(other.GetComponent<Ball>().LastPlayer);
            other.GetComponent<Ball>().SlowDown();
        }
    }
}
