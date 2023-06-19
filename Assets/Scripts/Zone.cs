using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public List<GameObject> playersInZone;
    public GameObject activePlayer;

    private void Awake()
    {
        playersInZone = new List<GameObject>();
        activePlayer = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Active Player") || other.CompareTag("Deactive Player"))
        {
            playersInZone.Add(other.gameObject);
        }

        if(other.CompareTag("Active Player"))
        {
            activePlayer = other.gameObject;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Active Player"))
        {
            activePlayer = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Active Player") || other.CompareTag("Deactive Player")))
        {
            playersInZone.Remove(other.gameObject);
        }

        if (other.CompareTag("Active Player"))
        {
            activePlayer = null;
        }
    }
}
