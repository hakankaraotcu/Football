using UnityEngine;

public class OutHandler : MonoBehaviour
{
    public GameObject ballPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            switch(gameObject.tag)
            {
                case "Throw":
                    Vector3 collisionPoint = other.ClosestPointOnBounds(transform.position);
                    GameManager.GetInstance().Throw(other.GetComponent<Ball>().LastPlayer, collisionPoint);
                    break;
                case "Out":
                    GameManager.GetInstance().Out(other.GetComponent<Ball>().LastPlayer);
                    break;
            }
        }
    }
}