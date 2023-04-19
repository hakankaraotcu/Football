using UnityEngine;

public class OutHandler : MonoBehaviour
{
    public GameObject ball;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            switch (gameObject.tag)
            {
                case "Out":
                    Debug.Log("Out");
                    Destroy(other.gameObject);
                    Instantiate(ball);
                    break;
                case "Throw":
                    Debug.Log("Throw");
                    Destroy(other.gameObject);
                    Instantiate(ball);
                    break;
            }
        }
    }
}