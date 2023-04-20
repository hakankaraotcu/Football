using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private float cameraSpeed;
    private Transform target;
    private Transform ball;

    private void LateUpdate()
    {
        target = GameObject.FindWithTag("Active Player").transform;
        if (target.GetComponent<Player>().Ball == null)
        {
            transform.position = Vector3.Lerp(transform.position, ball.position + offset, Time.deltaTime * cameraSpeed);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.deltaTime * cameraSpeed);
        }
    }

    public void SetBall(Transform ball)
    {
        this.ball = ball;
    }
}
