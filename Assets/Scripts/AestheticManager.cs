using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AestheticManager : MonoBehaviour
{
    [Header("Paleta de Colores Pastel")]
    public Color colorFondoCamara = new Color(0.957f, 0.761f, 0.761f); // #F4C2C2 (Rosa Pastel Twilight)
    public Color colorPaneles = new Color(1.0f, 0.92f, 0.95f, 0.92f); // Rosa/Lila translúcido
    public Color colorTextoHUD = new Color(0.05f, 0.05f, 0.05f); // Negro suave para contraste ultra-claro
    public Color colorTextoDerrota = new Color(0.55f, 0.08f, 0.18f); // Rojo frambuesa de alto contraste
    
    [Header("Estilo de Botones")]
    public Color colorBotonNormal = new Color(1.0f, 0.88f, 0.92f);
    public Color colorBotonResaltado = new Color(1.0f, 0.78f, 0.85f);
    public Color colorBotonPresionado = new Color(0.9f, 0.7f, 0.78f);
    
    void Start()
    {
        AplicarEstiloCamara();
        AplicarEstiloUI();
    }

    void OnEnable()
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.SettingsChangedEvent += OnSettingsChanged;
        }
    }

    void OnDisable()
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.SettingsChangedEvent -= OnSettingsChanged;
        }
    }

    void OnSettingsChanged()
    {
        AplicarEstiloCamara();
        AplicarEstiloUI();
    }

    public void AplicarEstiloCamara()
    {
        Color targetBg = colorFondoCamara;
        if (GameSettingsManager.Instance != null && GameSettingsManager.Instance.modoOscuro)
        {
            targetBg = new Color(0.07f, 0.06f, 0.11f); // Muy oscuro (#13111C)
        }

        Camera[] cameras = Camera.allCameras;
        foreach (Camera cam in cameras)
        {
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = targetBg;
            }
        }
    }

    public void AplicarEstiloUI()
    {
        bool dark = false;
        if (GameSettingsManager.Instance != null)
        {
            dark = GameSettingsManager.Instance.modoOscuro;
        }

        Color panColor = dark ? new Color(0.12f, 0.10f, 0.16f, 0.95f) : colorPaneles;
        Color outlineColor = dark ? new Color(0.30f, 0.25f, 0.35f) : new Color(0.85f, 0.7f, 0.75f);
        Color txtColor = dark ? new Color(0.92f, 0.92f, 0.95f) : colorTextoHUD;
        Color btnNormal = dark ? new Color(0.22f, 0.18f, 0.28f) : colorBotonNormal;
        Color btnHighlight = dark ? new Color(0.32f, 0.28f, 0.40f) : colorBotonResaltado;
        Color btnPressed = dark ? new Color(0.15f, 0.12f, 0.20f) : colorBotonPresionado;
        Color txtDerrota = dark ? new Color(0.95f, 0.35f, 0.40f) : colorTextoDerrota;
        Color txtVictoria = dark ? new Color(0.40f, 0.85f, 0.50f) : new Color(0.1f, 0.5f, 0.25f);

        // Buscar todas las imágenes de la interfaz
        Image[] imagenes = FindObjectsOfType<Image>(true);
        foreach (Image img in imagenes)
        {
            string nombre = img.gameObject.name.ToLower();
            
            // Si es un panel de fondo de menú o de fin de juego
            if (nombre.Contains("panel") || nombre.Contains("victoria") || nombre.Contains("derrota") || nombre.Contains("menu") || nombre.Contains("opciones") || nombre.Contains("background") || nombre.Contains("box") || nombre.Contains("frame") || nombre.Contains("blocker"))
            {
                // Comprobamos si tiene un botón adjunto; si es así, no le ponemos color de panel
                if (img.GetComponent<Button>() == null)
                {
                    img.color = panColor;
                    
                    // Agregar un borde sutil para embellecer
                    Outline outline = img.gameObject.GetComponent<Outline>();
                    if (outline == null)
                    {
                        outline = img.gameObject.AddComponent<Outline>();
                    }
                    outline.effectColor = outlineColor;
                    outline.effectDistance = new Vector2(2, -2);
                }
            }
        }

        // Buscar y estilizar todos los botones
        Button[] botones = FindObjectsOfType<Button>(true);
        foreach (Button btn in botones)
        {
            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock colores = btn.colors;
            colores.normalColor = btnNormal;
            colores.highlightedColor = btnHighlight;
            colores.pressedColor = btnPressed;
            colores.selectedColor = btnNormal;
            btn.colors = colores;

            // Estilizar el texto del botón
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.color = txtColor;
                btnText.fontWeight = FontWeight.Bold;
            }
        }

        // Buscar y formatear los textos del juego
        TextMeshProUGUI[] textos = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI txt in textos)
        {
            string nombre = txt.gameObject.name.ToLower();
            
            if (nombre.Contains("derrota") || nombre.Contains("muerto") || nombre.Contains("died") || nombre.Contains("defeat"))
            {
                txt.color = txtDerrota;
            }
            else if (nombre.Contains("victoria") || nombre.Contains("ganado") || nombre.Contains("win") || nombre.Contains("victory"))
            {
                txt.color = txtVictoria;
            }
            else
            {
                if (txt.GetComponentInParent<Button>() == null)
                {
                    txt.color = txtColor;
                }
            }
        }
    }
}
