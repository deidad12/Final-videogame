using UnityEngine;
using System.Collections.Generic;

public class DynamicLevelPopulator : MonoBehaviour
{
    private Sprite coinSprite;
    private Sprite spikeSprite;
    private Sprite speedSprite;
    private Sprite jumpSprite;
    private Sprite heartSprite;
    private Sprite portalSprite;

    private static bool templatesGenerated = false;

    private string sortingLayerName = "Default";
    private int sortingLayerID = 0;

    private float probabilidadPincho = 0.3f;
    private float probabilidadPowerUp = 0.15f;
    private float distanciaPasoPinchos = 3f;

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
                return;
            }
        }
    }

    // Helper para generar Texturas/Sprites triangulares para los pinchos
    public static Sprite CreateTriangleSprite(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color transparent = new Color(0f, 0f, 0f, 0f);

        for (int y = 0; y < size; y++)
        {
            float halfWidth = (size / 2f) * (1f - (float)y / size);
            float left = (size / 2f) - halfWidth;
            float right = (size / 2f) + halfWidth;

            for (int x = 0; x < size; x++)
            {
                if (x >= left && x <= right)
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
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    // Genera una moneda más realista: degradado radial dorado + aro metálico + brillo (highlight),
    // en lugar de un círculo plano de un solo color.
    public static Sprite CreateCoinSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color transparent = new Color(0f, 0f, 0f, 0f);

        Color centro = new Color(1.0f, 0.93f, 0.55f);   // Amarillo dorado brillante
        Color medio = new Color(1.0f, 0.82f, 0.25f);     // Dorado
        Color borde = new Color(0.75f, 0.55f, 0.08f);    // Dorado oscuro / bronce (aro)
        Color brillo = new Color(1f, 1f, 0.92f, 0.85f);  // Highlight blanco-amarillento

        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);
        Vector2 highlightCenter = new Vector2(radius * 0.68f, radius * 1.32f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center.x;
                float dy = y - center.y;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float t = dist / radius;

                if (t > 1f)
                {
                    texture.SetPixel(x, y, transparent);
                    continue;
                }

                Color c;
                if (t > 0.82f)
                {
                    // Aro metálico exterior
                    c = borde;
                }
                else
                {
                    // Degradado radial del cuerpo de la moneda
                    float tBody = Mathf.InverseLerp(0f, 0.82f, t);
                    c = Color.Lerp(centro, medio, tBody);
                }

                // Brillo especular en la esquina superior-izquierda
                float distBrillo = Vector2.Distance(new Vector2(x, y), highlightCenter);
                float brilloT = 1f - Mathf.Clamp01(distBrillo / (radius * 0.35f));
                if (brilloT > 0f)
                {
                    c = Color.Lerp(c, brillo, brilloT * brillo.a * 0.9f);
                }

                texture.SetPixel(x, y, c);
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    void Start()
    {
        // Los límites del nivel anterior (si los hubiera) ya no son válidos hasta que
        // este nivel calcule los suyos propios.
        LevelBounds.Clear();

        // Ignorar en menú principal
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu" || sceneName == (GameManager.Instance != null ? GameManager.Instance.escenaMenu : ""))
        {
            return;
        }

        // Configurar dificultad según la escena
        if (sceneName.Contains("Level1"))
        {
            probabilidadPincho = 0.2f;
            probabilidadPowerUp = 0.2f;
            distanciaPasoPinchos = 4f; // Más espacio
        }
        else if (sceneName.Contains("Level2"))
        {
            probabilidadPincho = 0.5f; // Bastante más pinchos
            probabilidadPowerUp = 0.15f;
            distanciaPasoPinchos = 2.2f; // Más apretados
        }
        else if (sceneName.Contains("Level3"))
        {
            probabilidadPincho = 0.65f; // Muchos pinchos
            probabilidadPowerUp = 0.1f;
            distanciaPasoPinchos = 1.8f; // Súper apretados
        }
        else
        {
            probabilidadPincho = 0.3f;
            probabilidadPowerUp = 0.15f;
            distanciaPasoPinchos = 3f;
        }

        DetectarSortingLayerPlataformas();
        GenerarSpritesMecánicas();
        PopularNivel();
    }

    private void GenerarSpritesMecánicas()
    {
        // Generar sprites usando el helper procedural
        coinSprite = CreateCoinSprite(28); // Moneda con degradado dorado, aro y brillo (más realista)
        spikeSprite = CreateTriangleSprite(48, new Color(0.6f, 0.25f, 0.35f));                        // Frambuesa Oscuro
        speedSprite = DynamicBackgroundManager.CreateCircleSprite(24, new Color(0.4f, 1.0f, 0.4f));  // Verde Brillante
        jumpSprite = DynamicBackgroundManager.CreateCircleSprite(24, new Color(1.0f, 0.9f, 0.3f));   // Amarillo Claro
        heartSprite = DynamicBackgroundManager.CreateCircleSprite(24, new Color(1.0f, 0.4f, 0.5f));  // Coral/Rojo
        portalSprite = DynamicBackgroundManager.CreateCircleSprite(64, new Color(0.85f, 0.75f, 1.0f, 0.8f)); // Portal Lila Pastel
    }

    private void PopularNivel()
    {
        // Buscar todas las superficies con Colisionadores 2D
        Collider2D[] todosLosColliders = FindObjectsOfType<Collider2D>();
        List<Collider2D> plataformas = new List<Collider2D>();

        foreach (Collider2D col in todosLosColliders)
        {
            // Filtrar colliders que actúen como suelos estáticos (no triggers y no dinámicos)
            if (!col.isTrigger && col.gameObject.activeInHierarchy)
            {
                // Ignorar jugadores, proyectiles y zonas de muerte
                if (col.CompareTag("Player") || col.gameObject.name.ToLower().Contains("player") || 
                    col.gameObject.name.ToLower().Contains("projectile") || col.gameObject.name.ToLower().Contains("fallzone"))
                {
                    continue;
                }

                // Ignorar rigidbodies dinámicos
                Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
                {
                    continue;
                }

                // Si tiene un ancho respetable y es una plataforma horizontal (evitar columnas verticales)
                if (col.bounds.size.x >= 0.5f && col.bounds.size.x >= col.bounds.size.y)
                {
                    plataformas.Add(col);
                }
            }
        }

        if (plataformas.Count == 0)
        {
            Debug.LogWarning("No se encontraron plataformas estáticas para popular.");
            return;
        }

        // Ordenar plataformas por su posición X para determinar el inicio y fin del nivel
        plataformas.Sort((a, b) => a.bounds.center.x.CompareTo(b.bounds.center.x));

        // Crear las paredes y techo límites para contener al jugador en la zona de juego
        float minX = plataformas[0].bounds.min.x;
        float maxX = plataformas[plataformas.Count - 1].bounds.max.x;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        foreach (Collider2D plat in plataformas)
        {
            if (plat.bounds.min.y < minY) minY = plat.bounds.min.y;
            if (plat.bounds.max.y > maxY) maxY = plat.bounds.max.y;
        }
        CrearParedesLimites(minX, maxX, minY, maxY);

        // Encontrar plataforma más a la derecha para poner la meta (VictoryZone) o el jefe
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool esNivelJefe = (sceneName.Contains("Level3") || sceneName == (GameManager.Instance != null ? GameManager.Instance.escenaNivel3 : "Level3"));
        
        Collider2D plataformaMeta = plataformas[plataformas.Count - 1];

        if (plataformaMeta != null)
        {
            if (esNivelJefe)
            {
                BossController jefeExistente = FindObjectOfType<BossController>();
                if (jefeExistente == null)
                {
                    SpawnearJefe(plataformaMeta);
                }
            }
            else
            {
                VictoryZone metaExistente = FindObjectOfType<VictoryZone>();
                if (metaExistente == null)
                {
                    SpawnearMeta(plataformaMeta);
                }
            }
        }

        // Crear indicadores de precipicios y zonas de caída (KillZone) entre plataformas consecutivas
        for (int i = 0; i < plataformas.Count - 1; i++)
        {
            float finPlat = plataformas[i].bounds.max.x;
            float inicioSiguiente = plataformas[i + 1].bounds.min.x;
            float gap = inicioSiguiente - finPlat;

            if (gap > 0.5f)
            {
                float centroHueco = finPlat + gap / 2f;
                float altoY = plataformas[i].bounds.max.y;
                SpawnearVisualPrecipicio(centroHueco, altoY - 3f, gap, 8f);
            }
        }

        // Poblar cada plataforma con obstáculos (pinchos), monedas y powerups
        int indexPlataforma = 0;
        foreach (Collider2D plat in plataformas)
        {
            // Omitir la primera plataforma (donde spawnea el jugador) y la última (donde está la meta/jefe) para dar un inicio y fin seguro
            if (indexPlataforma == 0 || indexPlataforma == plataformas.Count - 1)
            {
                indexPlataforma++;
                continue;
            }

            Bounds limites = plat.bounds;
            float ancho = limites.size.x;
            float altoY = limites.max.y; // El borde superior del suelo

            // Dependiendo del ancho de la plataforma, decidimos qué colocar
            if (ancho >= 3f && ancho < 6f)
            {
                // Plataforma pequeña: Fila de monedas o un pincho
                if (Random.value > probabilidadPincho)
                {
                    // Spawnear 2 monedas flotantes
                    SpawnearMoneda(limites.center.x - 0.75f, altoY + 1.2f);
                    SpawnearMoneda(limites.center.x + 0.75f, altoY + 1.2f);
                }
                else
                {
                    // Spawnear un pincho en el centro
                    SpawnearPincho(limites.center.x, altoY);
                }
            }
            else if (ancho >= 6f && ancho < 12f)
            {
                // Plataforma mediana: Combinación de pinchos y un arco de monedas
                float centroX = limites.center.x;
                
                // Pincho en los extremos o centro
                if (Random.value < probabilidadPincho)
                {
                    SpawnearPincho(limites.min.x + 1.5f, altoY);
                    SpawnearPincho(limites.max.x - 1.5f, altoY);
                    
                    // Arco de monedas en el centro
                    SpawnearArcoMonedas(centroX, altoY + 1f, 3);
                }
                else
                {
                    // Obstáculo en el centro
                    SpawnearPincho(centroX, altoY);
                    
                    // Monedas en los costados
                    SpawnearMoneda(limites.min.x + 1.5f, altoY + 1.2f);
                    SpawnearMoneda(limites.max.x - 1.5f, altoY + 1.2f);

                    // Powerup ocasional
                    if (Random.value < probabilidadPowerUp)
                    {
                        SpawnearPowerUp(centroX, altoY + 2.5f);
                    }
                }
            }
            else if (ancho >= 12f)
            {
                // Plataforma larga: Varias monedas, múltiples pinchos y powerups garantizados
                float step = distanciaPasoPinchos;
                float startX = limites.min.x + 2f;
                float endX = limites.max.x - 2f;

                for (float x = startX; x <= endX; x += step)
                {
                    float rand = Random.value;
                    if (rand < probabilidadPincho)
                    {
                        // Pincho
                        SpawnearPincho(x, altoY);
                    }
                    else if (rand < 0.75f)
                    {
                        // Monedas flotantes
                        SpawnearMoneda(x, altoY + 1.2f);
                        if (x + 1f <= endX) SpawnearMoneda(x + 1f, altoY + 1.2f);
                    }
                    else if (rand < 0.75f + probabilidadPowerUp)
                    {
                        // PowerUp
                        SpawnearPowerUp(x, altoY + 1.2f);
                    }
                }
            }

            indexPlataforma++;
        }
    }

    private void TrySetTag(GameObject go, string tagName)
    {
        try
        {
            go.tag = tagName;
        }
        catch (System.Exception)
        {
            Debug.LogWarning($"El tag '{tagName}' no está definido en el proyecto. Se usará el nombre del objeto para detección de colisiones.");
        }
    }

    private void SpawnearVisualPrecipicio(float x, float y, float width, float height)
    {
        // 1. Crear un indicador visual del precipicio (Glow vertical azul/celeste)
        GameObject visual = new GameObject("PrecipiceGlow", typeof(SpriteRenderer));
        visual.transform.position = new Vector3(x, y, 0f);
        
        SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
        // Generar un sprite de gradiente vertical azul/celeste translúcido de 100x100 para estiramiento 1:1
        Color topColor = new Color(0.3f, 0.6f, 1f, 0.45f); // Celeste brillante
        Color bottomColor = new Color(0.1f, 0.2f, 0.5f, 0.05f); // Azul oscuro casi invisible abajo
        sr.sprite = DynamicBackgroundManager.CreateGradientSprite(100, 100, topColor, bottomColor);
        sr.drawMode = SpriteDrawMode.Simple;
        
        // Escalar para cubrir el ancho del hueco y descender verticalmente
        visual.transform.localScale = new Vector3(width, height, 1f);
        sr.sortingLayerName = sortingLayerName;
        sr.sortingLayerID = sortingLayerID;
        sr.sortingOrder = -5; // Detrás del jugador y plataformas, pero en frente del fondo

        // 2. Crear una zona de muerte por caída (KillZone / FallZone) al fondo del hueco
        GameObject killZone = new GameObject("FallZone_Gap", typeof(BoxCollider2D), typeof(FallZone));
        killZone.transform.position = new Vector3(x, y - height / 2f + 0.5f, 0f);
        
        BoxCollider2D bc = killZone.GetComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(width * 1.5f, 1.5f);
    }

    private void SpawnearMoneda(float x, float y)
    {
        GameObject coin = new GameObject("CollectibleCoin", typeof(RectTransform), typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(CollectibleItem));
        coin.transform.position = new Vector3(x, y, 0f);
        
        SpriteRenderer sr = coin.GetComponent<SpriteRenderer>();
        sr.sprite = coinSprite;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingLayerID = sortingLayerID;
        sr.sortingOrder = 5;

        CircleCollider2D cc = coin.GetComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 0.25f;

        CollectibleItem item = coin.GetComponent<CollectibleItem>();
        item.tipo = CollectibleItem.CollectibleType.Candy;
        item.valor = 1;

        TrySetTag(coin, "Coin");

        // Añadir una pequeña animación de rotación/giro al script
        coin.AddComponent<CoinAnimation>();
    }

    private void SpawnearArcoMonedas(float centerX, float baseHeight, int count)
    {
        float width = 3f;
        float height = 1.2f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / (count - 1);
            // Parábola
            float x = Mathf.Lerp(centerX - width/2f, centerX + width/2f, t);
            float y = baseHeight + 4f * height * t * (1f - t); // y = h * 4 * t * (1-t)
            SpawnearMoneda(x, y);
        }
    }

    private void SpawnearPincho(float x, float y)
    {
        // En los pinchos colocamos una colisión trigger con tag Hazard
        GameObject spike = new GameObject("ObstacleSpike", typeof(SpriteRenderer), typeof(PolygonCollider2D), typeof(ObstacleSpikes));
        spike.transform.position = new Vector3(x, y + 0.24f, 0f); // Offset del centro del triángulo (alto 0.48 / 2 = 0.24)
        
        TrySetTag(spike, "Hazard");
        
        SpriteRenderer sr = spike.GetComponent<SpriteRenderer>();
        sr.sprite = spikeSprite;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingLayerID = sortingLayerID;
        sr.sortingOrder = 4;

        // PolygonCollider2D se ajusta automáticamente a la forma del sprite
        PolygonCollider2D pc = spike.GetComponent<PolygonCollider2D>();
        pc.isTrigger = true;
        
        // Ajustamos los puntos del colisionador de forma manual a un triángulo
        Vector2[] points = new Vector2[3];
        points[0] = new Vector2(0f, 0.24f);      // Punta superior
        points[1] = new Vector2(-0.24f, -0.24f);  // Esquina inferior izquierda
        points[2] = new Vector2(0.24f, -0.24f);   // Esquina inferior derecha
        pc.points = points;
    }

    private void SpawnearPowerUp(float x, float y)
    {
        GameObject power = new GameObject("PowerUpItem", typeof(RectTransform), typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(CollectibleItem));
        power.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = power.GetComponent<SpriteRenderer>();
        
        CircleCollider2D cc = power.GetComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 0.25f;

        CollectibleItem item = power.GetComponent<CollectibleItem>();
        item.duracionEfecto = 5f;

        float r = Random.value;
        if (r < 0.4f)
        {
            // Speed Boost
            sr.sprite = speedSprite;
            item.tipo = CollectibleItem.CollectibleType.SpeedBoost;
            item.multiplicadorEfecto = 1.5f;
            power.name = "SpeedPowerUp";
        }
        else if (r < 0.8f)
        {
            // Jump Boost
            sr.sprite = jumpSprite;
            item.tipo = CollectibleItem.CollectibleType.JumpBoost;
            item.multiplicadorEfecto = 1.3f;
            power.name = "JumpPowerUp";
        }
        else
        {
            // Heart (Vida Extra)
            sr.sprite = heartSprite;
            item.tipo = CollectibleItem.CollectibleType.ExtraLife;
            power.name = "HeartPowerUp";
        }
        sr.sortingLayerName = sortingLayerName;
        sr.sortingLayerID = sortingLayerID;
        sr.sortingOrder = 5;

        // Añadir animación oscilante flotante
        power.AddComponent<PowerUpFloatingAnimation>();
    }

    private void SpawnearMeta(Collider2D plataforma)
    {
        Bounds limites = plataforma.bounds;
        float x = limites.max.x - 1.5f; // Un poco antes de que termine el suelo
        float y = limites.max.y + 0.6f;

        GameObject portal = new GameObject("VictoryPortal", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(VictoryZone));
        portal.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = portal.GetComponent<SpriteRenderer>();
        sr.sprite = portalSprite;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingLayerID = sortingLayerID;
        sr.sortingOrder = 3;

        BoxCollider2D bc = portal.GetComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(1.2f, 1.5f);

        // Darle un brillo animado al portal
        portal.AddComponent<PortalPulseAnimation>();

        // --- ENTRADA AL OTRO NIVEL (ARCHWAY & CHECKERS) ---
        // 1. Columnas Laterales
        Color colorMarco = new Color(0.95f, 0.82f, 0.90f); // Rosa lila marco
        Sprite marcoSprite = DynamicBackgroundManager.CreateCircleSprite(16, colorMarco); // Circular suave

        GameObject colIzq = new GameObject("Arch_LeftColumn", typeof(SpriteRenderer));
        colIzq.transform.SetParent(portal.transform, false);
        colIzq.transform.localPosition = new Vector3(-0.6f, 0f, 0.1f);
        colIzq.transform.localScale = new Vector3(0.18f, 1.5f, 1f);
        SpriteRenderer srCI = colIzq.GetComponent<SpriteRenderer>();
        srCI.sprite = marcoSprite;
        srCI.sortingLayerName = sortingLayerName;
        srCI.sortingLayerID = sortingLayerID;
        srCI.sortingOrder = 4;

        GameObject colDer = new GameObject("Arch_RightColumn", typeof(SpriteRenderer));
        colDer.transform.SetParent(portal.transform, false);
        colDer.transform.localPosition = new Vector3(0.6f, 0f, 0.1f);
        colDer.transform.localScale = new Vector3(0.18f, 1.5f, 1f);
        SpriteRenderer srCD = colDer.GetComponent<SpriteRenderer>();
        srCD.sprite = marcoSprite;
        srCD.sortingLayerName = sortingLayerName;
        srCD.sortingLayerID = sortingLayerID;
        srCD.sortingOrder = 4;

        // 2. Dintel Superior
        GameObject dintel = new GameObject("Arch_Lintel", typeof(SpriteRenderer));
        dintel.transform.SetParent(portal.transform, false);
        dintel.transform.localPosition = new Vector3(0f, 0.75f, 0.1f);
        dintel.transform.localScale = new Vector3(1.38f, 0.18f, 1f);
        SpriteRenderer srD = dintel.GetComponent<SpriteRenderer>();
        srD.sprite = marcoSprite;
        srD.sortingLayerName = sortingLayerName;
        srD.sortingLayerID = sortingLayerID;
        srD.sortingOrder = 4;

        // 3. Línea de Meta Cuadriculada (Checkered Finish Line)
        GameObject finishLine = new GameObject("FinishLineCheckers", typeof(SpriteRenderer));
        finishLine.transform.SetParent(portal.transform, false);
        finishLine.transform.localPosition = new Vector3(0f, -0.6f, 0.1f);
        finishLine.transform.localScale = new Vector3(1.4f, 0.2f, 1f);
        SpriteRenderer srFL = finishLine.GetComponent<SpriteRenderer>();
        srFL.sprite = CreateCheckeredSprite(64, 16, 8, Color.white, new Color(0.2f, 0.15f, 0.2f));
        srFL.sortingLayerName = sortingLayerName;
        srFL.sortingLayerID = sortingLayerID;
        srFL.sortingOrder = 4;
    }

    // Helper para generar Texturas/Sprites de Tablero de Ajedrez (Meta)
    public static Sprite CreateCheckeredSprite(int width, int height, int checkSize, Color c1, Color c2)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int checkX = x / checkSize;
                int checkY = y / checkSize;
                Color c = ((checkX + checkY) % 2 == 0) ? c1 : c2;
                texture.SetPixel(x, y, c);
            }
        }
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    private void SpawnearJefe(Collider2D plataforma)
    {
        Bounds limites = plataforma.bounds;
        float centerX = limites.center.x - 2f;
        float y = limites.max.y + 1f;

        GameObject bossObj = new GameObject("Boss", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(BossController));
        bossObj.transform.position = new Vector3(centerX, y, 0f);
        bossObj.tag = "Enemy";

        SpriteRenderer sr = bossObj.GetComponent<SpriteRenderer>();
        // Usamos un sprite circular grande teñido de frambuesa/rojo para el Jefe
        sr.sprite = DynamicBackgroundManager.CreateCircleSprite(48, new Color(0.85f, 0.2f, 0.3f));
        sr.sortingLayerName = sortingLayerName;
        sr.sortingLayerID = sortingLayerID;
        sr.sortingOrder = 4;

        Rigidbody2D rb = bossObj.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;

        BoxCollider2D bc = bossObj.GetComponent<BoxCollider2D>();
        bc.size = new Vector2(0.96f, 0.96f);

        // Crear límites de patrullaje para el jefe en la plataforma
        GameObject limIzq = new GameObject("Boss_LimitLeft");
        limIzq.transform.position = new Vector3(limites.min.x + 1f, y, 0f);
        
        GameObject limDer = new GameObject("Boss_LimitRight");
        limDer.transform.position = new Vector3(limites.max.x - 1f, y, 0f);

        BossController boss = bossObj.GetComponent<BossController>();
        boss.limiteIzquierdo = limIzq.transform;
        boss.limiteDerecho = limDer.transform;
        boss.spriteRenderer = sr;
        boss.vidaMaxima = 3;
        boss.velocidad = 2.5f;
        boss.fuerzaSalto = 7f;
    }

    private void CrearParedesLimites(float minX, float maxX, float minY, float maxY)
    {
        float grosor = 2f;
        float altura = (maxY - minY) + 30f;
        float centroY = minY + (maxY - minY) / 2f;

        // Colocamos las paredes un poco más pegadas al nivel (0.1 en vez de 0.5) para que
        // la cámara (ver LevelBounds/CameraFollow) no deje ver franjas vacías de más allá del límite.
        float margenPared = 0.1f;
        float paredIzqX = minX - (grosor / 2f) - margenPared;
        float paredDerX = maxX + (grosor / 2f) + margenPared;

        // 1. Pared Izquierda (Left Wall)
        GameObject paredIzquierda = new GameObject("Boundary_LeftWall", typeof(BoxCollider2D));
        paredIzquierda.transform.position = new Vector3(paredIzqX, centroY, 0f);
        BoxCollider2D bcIzq = paredIzquierda.GetComponent<BoxCollider2D>();
        bcIzq.size = new Vector2(grosor, altura);
        TrySetTag(paredIzquierda, "Ground");

        // 2. Pared Derecha (Right Wall)
        GameObject paredDerecha = new GameObject("Boundary_RightWall", typeof(BoxCollider2D));
        paredDerecha.transform.position = new Vector3(paredDerX, centroY, 0f);
        BoxCollider2D bcDer = paredDerecha.GetComponent<BoxCollider2D>();
        bcDer.size = new Vector2(grosor, altura);
        TrySetTag(paredDerecha, "Ground");

        // 3. Suelo Límite (Floor) — evita que el jugador caiga al vacío eterno si se escapa
        // por debajo de la última plataforma conocida (p. ej. atravesando un hueco mal calculado).
        float sueloY = minY - 8f;
        float anchoSuelo = (maxX - minX) + (grosor * 2f) + 2f;
        GameObject suelo = new GameObject("Boundary_Floor", typeof(BoxCollider2D), typeof(FallZone));
        suelo.transform.position = new Vector3(minX + (maxX - minX) / 2f, sueloY, 0f);
        BoxCollider2D bcSuelo = suelo.GetComponent<BoxCollider2D>();
        bcSuelo.isTrigger = true;
        bcSuelo.size = new Vector2(anchoSuelo, grosor);

        // 4. Techo Límite (Ceiling Wall)
        float techoY = maxY + 12f;
        float anchoTecho = anchoSuelo;
        GameObject techo = new GameObject("Boundary_Ceiling", typeof(BoxCollider2D));
        techo.transform.position = new Vector3(minX + (maxX - minX) / 2f, techoY + (grosor / 2f), 0f);
        BoxCollider2D bcTecho = techo.GetComponent<BoxCollider2D>();
        bcTecho.size = new Vector2(anchoTecho, grosor);
        TrySetTag(techo, "Ground");

        // Publicar los límites del nivel para que la cámara (CameraFollow) no muestre
        // vacío más allá de las paredes.
        LevelBounds.Set(paredIzqX + grosor / 2f, paredDerX - grosor / 2f, sueloY, techoY);

        Debug.Log($"[DynamicLevelPopulator] Límites de nivel creados: Izq({paredIzqX}), Der({paredDerX}), Suelo({sueloY}), Techo({techoY + grosor / 2f})");
    }
}

// Contenedor simple y ligero con los límites de mundo del nivel actual, publicados por
// DynamicLevelPopulator y consumidos por CameraFollow para no dejar ver fuera de los límites.
// Se recalcula automáticamente en cada carga de escena (no persiste entre niveles).
public static class LevelBounds
{
    public static bool Valid { get; private set; }
    public static float MinX { get; private set; }
    public static float MaxX { get; private set; }
    public static float MinY { get; private set; }
    public static float MaxY { get; private set; }

    public static void Set(float minX, float maxX, float minY, float maxY)
    {
        MinX = minX;
        MaxX = maxX;
        MinY = minY;
        MaxY = maxY;
        Valid = minX < maxX && minY < maxY;
    }

    public static void Clear()
    {
        Valid = false;
    }
}

// --- CLASES AUXILIARES DE ANIMACIÓN PROCEDURAL PARA ELEMENTOS ---

public class CoinAnimation : MonoBehaviour
{
    private float spinSpeed = 180f;
    void Update()
    {
        // Rotar sobre el eje Y para efecto de moneda clásica
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
    }
}

public class PowerUpFloatingAnimation : MonoBehaviour
{
    private float floatSpeed = 3f;
    private float floatStrength = 0.15f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Flotar suavemente de arriba a abajo
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatStrength;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}

public class PortalPulseAnimation : MonoBehaviour
{
    private float pulseSpeed = 2f;
    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        // Escalar de forma pulsante para simular un portal de energía
        float scaleMultiplier = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.12f;
        transform.localScale = baseScale * scaleMultiplier;
    }
}
