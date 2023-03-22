using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    private Transform target;

    private void LateUpdate()
    {
        target = GameObject.FindWithTag("Active Player").transform;
        transform.position = target.position + offset;
    }
}
