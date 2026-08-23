using UnityEngine;

public class ObstacleSpikes : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si el jugador colisiona físicamente con los pinchos
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerMovement>() != null)
        {
            DañarJugador(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Por si los pinchos están configurados como Trigger (zona de muerte)
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null)
        {
            DañarJugador(collision.gameObject);
        }
    }

    private void DañarJugador(GameObject player)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RespawnPlayer(player);
        }
    }
}
