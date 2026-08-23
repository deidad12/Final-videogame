using UnityEngine;

public class FallZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si el objeto que entró al trigger es el jugador
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RespawnPlayer(collision.gameObject);
            }
        }
    }
}
