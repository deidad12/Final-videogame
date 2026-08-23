using UnityEngine;
using System.Collections;

public class DestructibleBlock : MonoBehaviour
{
    [Header("Configuración del Bloque")]
    public bool destruible = false; // ¿Se rompe o se activa como un bloque de interrogación?
    public GameObject prefabContenido; // Coleccionable que saldrá del bloque al golpearlo
    public Sprite spriteUsado; // Imagen del bloque después de ser golpeado

    [Header("Efecto Rebote")]
    public float alturaRebote = 0.25f;
    public float velocidadRebote = 8f;

    private bool golpeado = false;
    private Vector3 posicionOriginal;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        posicionOriginal = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    public void HitBlock()
    {
        if (golpeado) return;

        if (destruible)
        {
            RomperBloque();
        }
        else
        {
            golpeado = true;
            
            // Reproducir sonido de golpe de bloque
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBlock();
            }

            // Ejecutar animación física de rebote
            StartCoroutine(EfectoRebote());

            // Aparecer contenido sobre el bloque
            if (prefabContenido != null)
            {
                Instantiate(prefabContenido, transform.position + Vector3.up * 0.8f, Quaternion.identity);
            }
            
            // Cambiar imagen del bloque para indicar que ya se usó
            if (spriteUsado != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = spriteUsado;
            }
        }
    }

    IEnumerator EfectoRebote()
    {
        float timer = 0f;
        while (timer < Mathf.PI)
        {
            timer += Time.deltaTime * velocidadRebote;
            float despl = Mathf.Sin(timer) * alturaRebote;
            transform.position = posicionOriginal + Vector3.up * despl;
            yield return null;
        }
        transform.position = posicionOriginal;
    }

    void RomperBloque()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBlock();
        }
        
        // Destruir el objeto bloque
        Destroy(gameObject);
    }
}
