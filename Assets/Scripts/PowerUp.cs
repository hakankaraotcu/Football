using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public float multiplier = 1.5f;
    public float duration = 4f;

    public GameObject pickupEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Active Player"))
        {
            Pickup(other);
        }
    }

    private void Pickup(Collider player)
    {
        Player activePlayer = player.GetComponent<Player>();

        if (gameObject.CompareTag("SpeedPowerUp"))
        {
            activePlayer.PowerUpSpeed(multiplier, duration);
        }
        else if (gameObject.CompareTag("ShootPowerUp"))
        {
            activePlayer.PowerUpShoot(multiplier, duration);
        }

        Destroy(transform.parent.gameObject);
        GameObject.Find("Spawner").GetComponent<RandomSpawner>().SpawnNewPowerUp();
    }
}
