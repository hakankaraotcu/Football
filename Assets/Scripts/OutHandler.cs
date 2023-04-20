using UnityEngine;

public class OutHandler : MonoBehaviour
{
    public GameObject ballPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            GameObject ball = null;
            switch (gameObject.tag)
            {
                case "Out":
                    Debug.Log("Out");
                    Destroy(other.gameObject);
                    ball = Instantiate(ballPrefab);
                    break;
                case "Throw":
                    Debug.Log("Throw");
                    Destroy(other.gameObject);
                    ball = Instantiate(ballPrefab);
                    break;
            }
            FindObjectOfType<CameraFollow>().SetBall(ball.transform);
        }
    }
}