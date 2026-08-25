using UnityEngine;

public class VictoryZone : MonoBehaviour
{
    void Start()
    {
        AsegurarMarcaVisual();
    }

    // Garantiza que la zona de victoria tenga SIEMPRE una marca visual clara
    // (bandera a cuadros + poste), sin importar si fue colocada a mano en la escena
    // o generada automáticamente por DynamicLevelPopulator.
    private void AsegurarMarcaVisual()
    {
        if (transform.Find("VictoryFlagVisual") != null) return; // Ya tiene una marca
        if (GetComponentInChildren<SpriteRenderer>() != null) return; // Ya tiene su propio sprite (portal generado)

        string sortingLayerName = "Default";
        int sortingLayerID = 0;
        SpriteRenderer refRenderer = FindObjectOfType<SpriteRenderer>();
        if (refRenderer != null)
        {
            sortingLayerName = refRenderer.sortingLayerName;
            sortingLayerID = refRenderer.sortingLayerID;
        }

        GameObject marca = new GameObject("VictoryFlagVisual");
        marca.transform.SetParent(transform, false);
        marca.transform.localPosition = Vector3.zero;

        // Poste
        GameObject poste = new GameObject("FlagPole", typeof(SpriteRenderer));
        poste.transform.SetParent(marca.transform, false);
        poste.transform.localPosition = new Vector3(0f, -0.5f, 0f);
        poste.transform.localScale = new Vector3(0.08f, 2f, 1f);
        SpriteRenderer srPoste = poste.GetComponent<SpriteRenderer>();
        srPoste.sprite = DynamicBackgroundManager.CreateCircleSprite(16, new Color(0.35f, 0.30f, 0.35f));
        srPoste.sortingLayerName = sortingLayerName;
        srPoste.sortingLayerID = sortingLayerID;
        srPoste.sortingOrder = 3;

        // Bandera a cuadros (checkered)
        GameObject bandera = new GameObject("FlagCheckered", typeof(SpriteRenderer));
        bandera.transform.SetParent(marca.transform, false);
        bandera.transform.localPosition = new Vector3(0.55f, 0.35f, 0f);
        bandera.transform.localScale = new Vector3(1.1f, 0.75f, 1f);
        SpriteRenderer srBandera = bandera.GetComponent<SpriteRenderer>();
        srBandera.sprite = DynamicLevelPopulator.CreateCheckeredSprite(32, 24, 6, Color.white, new Color(0.15f, 0.1f, 0.18f));
        srBandera.sortingLayerName = sortingLayerName;
        srBandera.sortingLayerID = sortingLayerID;
        srBandera.sortingOrder = 4;

        marca.AddComponent<VictoryFlagWave>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si el objeto que entró al trigger es el jugador
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Victory();
            }
        }
    }
}

// Pequeña animación de ondeo para que la bandera de meta se note más
public class VictoryFlagWave : MonoBehaviour
{
    void Update()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 3f) * 6f);
    }
}
