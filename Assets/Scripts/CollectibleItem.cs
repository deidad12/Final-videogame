using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public enum CollectibleType { Candy, SpeedBoost, JumpBoost, ExtraLife }

    [Header("Tipo de Coleccionable")]
    public CollectibleType tipo = CollectibleType.Candy;

    [Header("Configuración del Coleccionable")]
    public int valor = 1;
    public float duracionEfecto = 5f;
    public float multiplicadorEfecto = 1.5f;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            switch (tipo)
            {
                case CollectibleType.Candy:
                    // Color original
                    break;
                case CollectibleType.SpeedBoost:
                    sr.color = new Color(0.4f, 1.0f, 0.4f); // Verde
                    break;
                case CollectibleType.JumpBoost:
                    sr.color = new Color(1.0f, 0.9f, 0.3f); // Amarillo
                    break;
                case CollectibleType.ExtraLife:
                    sr.color = new Color(1.0f, 0.4f, 0.5f); // Rosa fuerte / Rojo
                    break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMovement pm = collision.GetComponent<PlayerMovement>();
        if (pm == null)
        {
            pm = collision.GetComponentInParent<PlayerMovement>();
        }
        if (pm != null || collision.CompareTag("Player"))
        {
            if (pm == null) pm = FindObjectOfType<PlayerMovement>(); // Fallback
            Recoger(pm);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerMovement pm = collision.gameObject.GetComponent<PlayerMovement>();
        if (pm == null)
        {
            pm = collision.gameObject.GetComponentInParent<PlayerMovement>();
        }
        if (pm != null || collision.gameObject.CompareTag("Player"))
        {
            if (pm == null) pm = FindObjectOfType<PlayerMovement>(); // Fallback
            Recoger(pm);
        }
    }

    private void Recoger(PlayerMovement pm)
    {
        if (GameManager.Instance != null)
        {
            switch (tipo)
            {
                case CollectibleType.Candy:
                    GameManager.Instance.AddCollectible(valor);
                    break;
                case CollectibleType.SpeedBoost:
                    if (pm != null) pm.ActivarSpeedBoost(duracionEfecto, multiplicadorEfecto);
                    GameManager.Instance.AddScore(200);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayCollectible();
                    break;
                case CollectibleType.JumpBoost:
                    if (pm != null) pm.ActivarJumpBoost(duracionEfecto, 1.3f); // 1.3x para salto es balanceado
                    GameManager.Instance.AddScore(200);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayCollectible();
                    break;
                case CollectibleType.ExtraLife:
                    GameManager.Instance.AddLife();
                    GameManager.Instance.AddScore(300);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayCollectible();
                    break;
            }
        }
        
        // Destruir el objeto tras ser recogido
        Destroy(gameObject);
    }
}
