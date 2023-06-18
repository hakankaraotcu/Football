using System.Collections;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public GameObject[] powerUps;

    private void Start()
    {
        StartCoroutine(SpawnAfterTime());
    }

    public void SpawnNewPowerUp()
    {
        StartCoroutine(SpawnAfterTime());
    }

    IEnumerator SpawnAfterTime()
    {
        yield return new WaitForSeconds(Random.Range(10, 15));
        Vector3 randomSpawnPosition = new Vector3(Random.Range(-12, 17), -8, Random.Range(-23, 15));
        Instantiate(powerUps[Random.Range(0, powerUps.Length)], randomSpawnPosition, Quaternion.identity);
    }


}
