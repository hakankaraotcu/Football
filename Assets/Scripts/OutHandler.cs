using UnityEngine;

public class OutHandler : MonoBehaviour
{
    public GameObject ballPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            GameManager.GetInstance().Out(other.GetComponent<Ball>().LastPlayer, gameObject.tag);
        }
    }
}