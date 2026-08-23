using UnityEngine;
using System.Collections.Generic;

public class DynamicBackgroundManager : MonoBehaviour
{
    private Camera cam;
    private Transform bgContainer;
    private List<Transform> hillsLayer1 = new List<Transform>();
    private List<Transform> hillsLayer2 = new List<Transform>();
    private List<Transform> clouds = new List<Transform>();

    private float lastCamX;
    private float viewWidth;
    private float viewHeight;

    private string sortingLayerName = "Default";
    private int sortingLayerID = 0;

    private void DetectarSortingLayerPlataformas()
    {
        Renderer[] allRenderers = FindObjectsOfType<Renderer>(true);
        foreach (Renderer r in allRenderers)
        {
            if (r.gameObject.name.Contains("DynamicBackground") || r.gameObject.name.Contains("GradientSky") || 
                r.gameObject.name.Contains("Hill") || r.gameObject.name.Contains("Cloud") || 
                r.gameObject.CompareTag("Player") || r.gameObject.name.ToLower().Contains("player") ||
                r.gameObject.name.Contains("PrecipiceGlow") || r.gameObject.name.Contains("CollectibleCoin") ||
                r.gameObject.name.Contains("ObstacleSpike"))
            {
                continue;
            }

            string name = r.gameObject.name.ToLower();
            if (name.Contains("ground") || name.Contains("platform") || name.Contains("tilemap") || 
                name.Contains("brick") || name.Contains("floor") || name.Contains("suelo") || 
                name.Contains("plataforma") || name.Contains("grid") || r.gameObject.CompareTag("Ground") || 
                r.gameObject.CompareTag("Platform"))
            {
                sortingLayerName = r.sortingLayerName;
                sortingLayerID = r.sortingLayerID;
                Debug.Log($"Detectada Sorting Layer de plataforma: {sortingLayerName} (ID: {sortingLayerID})");
                return;
            }
        }

        foreach (Renderer r in allRenderers)
        {
            if (r.gameObject.name.Contains("DynamicBackground") || r.gameObject.name.Contains("GradientSky") || 
                r.gameObject.name.Contains("Hill") || r.gameObject.name.Contains("Cloud") || 
                r.gameObject.CompareTag("Player") || r.gameObject.name.ToLower().Contains("player") ||
                r.gameObject.name.Contains("PrecipiceGlow") || r.gameObject.name.Contains("CollectibleCoin") ||
                r.gameObject.name.Contains("ObstacleSpike"))
            {
                continue;
            }

            if (r is SpriteRenderer || r.GetType().Name.Contains("TilemapRenderer"))
            {
                sortingLayerName = r.sortingLayerName;
                sortingLayerID = r.sortingLayerID;
                Debug.Log($"Fallback a Sorting Layer de renderer estático: {sortingLayerName}");
                return;
            }
        }
    }

    // Helper para generar Texturas/Sprites circulares a nivel de código
    public static Sprite CreateCircleSprite(int radius, Color color)
    {
        int size = radius * 2;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Color transparent = new Color(0f, 0f, 0f, 0f);
        float rSquared = radius * radius;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - radius;
                float dy = y - radius;
                if (dx * dx + dy * dy <= rSquared)
                {
                    texture.SetPixel(x, y, color);
                }
                else
                {
                    texture.SetPixel(x, y, transparent);
                }
            }
        }
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    // Helper para generar Texturas/Sprites de Gradiente Vertical
    public static Sprite CreateGradientSprite(int width, int height, Color topColor, Color bottomColor)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            float t = (float)y / (height - 1);
            Color color = Color.Lerp(bottomColor, topColor, t);
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    void Start()
    {
        cam = Camera.main;
        if (cam == null) return;

        // Desactivar temporalmente si es el menú principal
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu" || sceneName == (GameManager.Instance != null ? GameManager.Instance.escenaMenu : ""))
        {
            // En el menú principal, un fondo estático es suficiente, pero podemos habilitarlo también
        }

        DetectarSortingLayerPlataformas();

        lastCamX = cam.transform.position.x;

        // Determinar dimensiones de pantalla en coordenadas del mundo
        viewHeight = cam.orthographicSize * 2f;
        viewWidth = viewHeight * cam.aspect;

        bgContainer = new GameObject("DynamicBackground").transform;
        bgContainer.SetParent(cam.transform, false);
        bgContainer.localPosition = new Vector3(0f, 0f, 15f); // Colocar detrás de todos los elementos del juego (Z = 15)

        GenerarFondoGradiente();
        GenerarColinas();
        GenerarNubes();
    }

    private void GenerarFondoGradiente()
    {
        GameObject gradObj = new GameObject("GradientSky", typeof(SpriteRenderer));
        gradObj.transform.SetParent(bgContainer, false);
        gradObj.transform.localPosition = new Vector3(0f, 0f, 5f); // Fondo lejano del contenedor

        SpriteRenderer sr = gradObj.GetComponent<SpriteRenderer>();
        bool dark = GameSettingsManager.Instance != null && GameSettingsManager.Instance.modoOscuro;
        Color top = dark ? new Color(0.06f, 0.05f, 0.10f) : new Color(0.957f, 0.761f, 0.761f);    // #F4C2C2 (Rosa Pastel Twilight / Azul Noche)
        Color bottom = dark ? new Color(0.12f, 0.09f, 0.18f) : new Color(0.910f, 0.627f, 0.749f); // #E8A0BF (Rosa Crepúsculo / Violeta Oscuro)
        sr.sprite = CreateGradientSprite(100, 100, top, bottom);
        sr.sortingLayerName = sortingLayerName;
        sr.sortingLayerID = sortingLayerID;
        sr.sortingOrder = -100; // Colocar en la capa más profunda para evitar tapar el nivel
        
        // Estirar para cubrir la pantalla completa
        sr.drawMode = SpriteDrawMode.Simple;
        gradObj.transform.localScale = new Vector3(viewWidth * 1.5f, viewHeight * 1.5f, 1f);
    }

    private void GenerarColinas()
    {
        // Creamos sprites circulares gigantes para simular colinas suaves
        bool dark = GameSettingsManager.Instance != null && GameSettingsManager.Instance.modoOscuro;
        Color hillColor1 = dark ? new Color(0.15f, 0.12f, 0.20f, 0.75f) : new Color(0.85f, 0.58f, 0.68f, 0.75f);
        Color hillColor2 = dark ? new Color(0.20f, 0.16f, 0.28f, 0.88f) : new Color(0.91f, 0.72f, 0.81f, 0.88f);
        Sprite hillSprite1 = CreateCircleSprite(256, hillColor1); // Capa lejana
        Sprite hillSprite2 = CreateCircleSprite(256, hillColor2);  // Capa media

        // Capa Lejana (Layer 1) - Parallax lento (0.15)
        // Spawnear colinas a lo largo de un rango horizontal amplio
        float startX = -50f;
        float endX = 200f;
        float stepX = 12f;

        GameObject layer1Parent = new GameObject("Hills_Far");
        layer1Parent.transform.SetParent(bgContainer, false);
        layer1Parent.transform.localPosition = new Vector3(0f, -viewHeight * 0.2f, 3f);

        for (float x = startX; x < endX; x += stepX + Random.Range(-2f, 2f))
        {
            GameObject hill = new GameObject("HillFar", typeof(SpriteRenderer));
            hill.transform.SetParent(layer1Parent.transform, false);
            hill.transform.localPosition = new Vector3(x, Random.Range(-2f, -0.5f), 0f);
            
            // Colinas de escala gigante
            float scale = Random.Range(3f, 4.5f);
            hill.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer sr = hill.GetComponent<SpriteRenderer>();
            sr.sprite = hillSprite1;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingLayerID = sortingLayerID;
            sr.sortingOrder = -95;

            hillsLayer1.Add(hill.transform);
        }

        // Capa Media (Layer 2) - Parallax medio (0.35)
        GameObject layer2Parent = new GameObject("Hills_Mid");
        layer2Parent.transform.SetParent(bgContainer, false);
        layer2Parent.transform.localPosition = new Vector3(0f, -viewHeight * 0.35f, 2f);

        stepX = 9f;
        for (float x = startX; x < endX; x += stepX + Random.Range(-1.5f, 1.5f))
        {
            GameObject hill = new GameObject("HillMid", typeof(SpriteRenderer));
            hill.transform.SetParent(layer2Parent.transform, false);
            hill.transform.localPosition = new Vector3(x, Random.Range(-1.5f, 0f), 0f);
            
            float scale = Random.Range(2.2f, 3.5f);
            hill.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer sr = hill.GetComponent<SpriteRenderer>();
            sr.sprite = hillSprite2;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingLayerID = sortingLayerID;
            sr.sortingOrder = -90;

            hillsLayer2.Add(hill.transform);
        }
    }

    private void GenerarNubes()
    {
        // Creamos una nube combinando 3 círculos blancos suaves o oscuros
        bool dark = GameSettingsManager.Instance != null && GameSettingsManager.Instance.modoOscuro;
        Color cloudColor = dark ? new Color(0.28f, 0.24f, 0.36f, 0.45f) : new Color(1f, 1f, 1f, 0.65f);
        Sprite cloudPart = CreateCircleSprite(64, cloudColor);

        GameObject cloudsParent = new GameObject("Clouds");
        cloudsParent.transform.SetParent(bgContainer, false);
        cloudsParent.transform.localPosition = new Vector3(0f, viewHeight * 0.15f, 4f);

        float startX = -40f;
        float endX = 180f;
        float stepX = 18f;

        for (float x = startX; x < endX; x += stepX + Random.Range(-4f, 4f))
        {
            GameObject cloudObj = new GameObject("CloudGroup");
            cloudObj.transform.SetParent(cloudsParent.transform, false);
            cloudObj.transform.localPosition = new Vector3(x, Random.Range(-1.5f, 1.5f), 0f);

            // Spawnear 3 círculos traslapados para darle aspecto esponjoso a la nube
            GameObject p1 = new GameObject("p1", typeof(SpriteRenderer));
            p1.transform.SetParent(cloudObj.transform, false);
            p1.transform.localPosition = Vector3.zero;
            p1.transform.localScale = new Vector3(1.5f, 1f, 1f);
            SpriteRenderer sr1 = p1.GetComponent<SpriteRenderer>();
            sr1.sprite = cloudPart;
            sr1.sortingLayerName = sortingLayerName;
            sr1.sortingLayerID = sortingLayerID;
            sr1.sortingOrder = -98;

            GameObject p2 = new GameObject("p2", typeof(SpriteRenderer));
            p2.transform.SetParent(cloudObj.transform, false);
            p2.transform.localPosition = new Vector3(-0.7f, -0.2f, 0f);
            p2.transform.localScale = new Vector3(1.1f, 0.8f, 1f);
            SpriteRenderer sr2 = p2.GetComponent<SpriteRenderer>();
            sr2.sprite = cloudPart;
            sr2.sortingLayerName = sortingLayerName;
            sr2.sortingLayerID = sortingLayerID;
            sr2.sortingOrder = -98;

            GameObject p3 = new GameObject("p3", typeof(SpriteRenderer));
            p3.transform.SetParent(cloudObj.transform, false);
            p3.transform.localPosition = new Vector3(0.7f, -0.2f, 0f);
            p3.transform.localScale = new Vector3(1.1f, 0.8f, 1f);
            SpriteRenderer sr3 = p3.GetComponent<SpriteRenderer>();
            sr3.sprite = cloudPart;
            sr3.sortingLayerName = sortingLayerName;
            sr3.sortingLayerID = sortingLayerID;
            sr3.sortingOrder = -98;

            cloudObj.transform.localScale = Vector3.one * Random.Range(0.8f, 1.4f);
            clouds.Add(cloudObj.transform);
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        float camX = cam.transform.position.x;
        float deltaX = camX - lastCamX;

        // Efecto Parallax: Desplazar las capas en dirección contraria al movimiento de la cámara
        // Capa 1 (Far Hills): mueve un 15% del delta de la cámara
        DesplazarCapa(hillsLayer1, deltaX * 0.85f); // Se mueve un 85% con la cámara -> 15% de parallax
        
        // Capa 2 (Mid Hills): mueve un 35% del delta de la cámara
        DesplazarCapa(hillsLayer2, deltaX * 0.65f); // Se mueve un 65% con la cámara -> 35% de parallax

        // Nubes: mueve un 5% de parallax + movimiento horizontal constante autónomo
        DesplazarNubes(clouds, deltaX * 0.95f, Time.deltaTime * 0.25f);

        lastCamX = camX;
    }

    private void DesplazarCapa(List<Transform> elementos, float offset)
    {
        foreach (Transform t in elementos)
        {
            if (t != null)
            {
                t.position = new Vector3(t.position.x + offset, t.position.y, t.position.z);
            }
        }
    }

    private void DesplazarNubes(List<Transform> elementos, float offsetParallax, float offsetAutonomo)
    {
        foreach (Transform t in elementos)
        {
            if (t != null)
            {
                // Parallax + Viento autónomo
                float nuevoX = t.position.x + offsetParallax + offsetAutonomo;
                
                // Si la nube se aleja demasiado hacia la izquierda de la cámara, envolverla hacia la derecha
                float camX = cam.transform.position.x;
                float limiteIzquierdo = camX - viewWidth * 1.2f;
                float limiteDerecho = camX + viewWidth * 1.5f;

                if (nuevoX < limiteIzquierdo)
                {
                    nuevoX = limiteDerecho;
                }

                t.position = new Vector3(nuevoX, t.position.y, t.position.z);
            }
        }
    }
}
