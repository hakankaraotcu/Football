using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public List<GameObject> playersInZone;

    private void Awake()
    {
        playersInZone = new List<GameObject>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Active Player") || other.CompareTag("Deactive Player"))
        {
            playersInZone.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Active Player") || other.CompareTag("Deactive Player")))
        {
            playersInZone.Remove(other.gameObject);
        }
    }
}
