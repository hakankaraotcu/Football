using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private float cameraSpeed;
    private Transform target;
    private Transform ball;

    private void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball").transform;
    }

    private void LateUpdate()
    {
        try
        {
            target = GameObject.FindWithTag("Active Player").transform;
            if (target.GetComponent<Player>().Ball == null)
            {
                Debug.Log("Following player in if");
                transform.position = Vector3.Lerp(transform.position, ball.position + offset, Time.deltaTime * cameraSpeed);
            }
            else
            {
                Debug.Log("Following ball in else");
                transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.deltaTime * cameraSpeed);
            }
        }
        catch (System.NullReferenceException)
        {
            Debug.Log("Following ball in catch");
            transform.position = Vector3.Lerp(transform.position, ball.position + offset, Time.deltaTime * cameraSpeed);
        }
    }
}
