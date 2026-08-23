using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 direccion;
    private float velocidad;
    private float tiempoDeVida = 5f;

    public void Inicializar(Vector2 dir, float vel)
    {
        direccion = dir.normalized;
        velocidad = vel;
        
        // Rotar el proyectil según su dirección de movimiento para que mire hacia adelante
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);
        
        Destroy(gameObject, tiempoDeVida);
    }

    void Update()
    {
        transform.Translate(direccion * velocidad * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RespawnPlayer(collision.gameObject);
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground") || collision.CompareTag("Platform"))
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // En caso de que el proyectil use un Collider regular que no sea Trigger
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerMovement>() != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RespawnPlayer(collision.gameObject);
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            Destroy(gameObject);
        }
    }
}
