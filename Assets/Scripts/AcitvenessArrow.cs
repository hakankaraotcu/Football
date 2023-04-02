using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcitvenessArrow : MonoBehaviour
{
    [SerializeField] private Vector3 rotation;

    private void Awake()
    {
        rotation = new Vector3(-30, 90, 0);
    }
    private void Update()
    {
        transform.rotation = Quaternion.Euler(rotation);
    }
}
