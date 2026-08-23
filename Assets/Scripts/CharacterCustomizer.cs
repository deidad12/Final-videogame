using UnityEngine;

public class CharacterCustomizer : MonoBehaviour
{
    public SkinData customSkin;
    private SpriteRenderer characterSr;
    private GameObject accessoryObj;
    private SpriteRenderer accessorySr;

    // Accesorios generados por código si no hay sprites asignados
    private static Sprite crownSprite;
    private static Sprite hatSprite;
    private static Sprite bowSprite;

    void Start()
    {
        characterSr = GetComponent<SpriteRenderer>();
        if (characterSr == null)
        {
            characterSr = GetComponentInChildren<SpriteRenderer>();
        }

        AplicarPersonalizacion();
    }

    public void AplicarPersonalizacion()
    {
        if (characterSr == null) return;

        int playerId = 1;
        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null)
        {
            playerId = pm.playerId;
        }

        // Si hay una SkinData asignada manualmente en el Inspector, tiene prioridad
        if (customSkin != null)
        {
            if (customSkin.characterSprite != null)
            {
                characterSr.sprite = customSkin.characterSprite;
            }
            characterSr.color = customSkin.colorTint;
            
            if (customSkin.accessorySprite != null)
            {
                CrearOActualizarAccesorio(customSkin.accessorySprite, customSkin.accessoryOffset, customSkin.accessoryScale);
            }
            return;
        }

        // Si no hay SkinData, cargamos desde las preferencias según el avatarIndex
        int avatarIndex = 0;
        if (playerId == 1)
        {
            avatarIndex = PlayerPrefs.GetInt("P1_Avatar", 0);
        }
        else
        {
            avatarIndex = PlayerPrefs.GetInt("P2_Avatar", 1);
        }

        // Configurar apariencia por defecto según el índice de avatar
        Color tintColor = Color.white;
        Sprite selectedAccessory = null;
        Vector3 offset = new Vector3(0f, 0.45f, -0.1f);
        Vector3 scale = Vector3.one * 0.7f;

        switch (avatarIndex)
        {
            case 0: // Avatar 0: Clásico
                tintColor = new Color(1.0f, 0.75f, 0.8f); // Rosa Pastel
                selectedAccessory = null; // Sin accesorio
                break;
            case 1: // Avatar 1: Gorro
                tintColor = new Color(0.7f, 0.85f, 1.0f); // Celeste Pastel
                selectedAccessory = GetHatSprite();
                offset = new Vector3(0f, 0.48f, -0.1f);
                break;
            case 2: // Avatar 2: Corona de Realeza
                tintColor = new Color(0.85f, 0.75f, 1.0f); // Lavanda
                selectedAccessory = GetCrownSprite();
                offset = new Vector3(0f, 0.52f, -0.1f);
                scale = Vector3.one * 0.8f;
                break;
            case 3: // Avatar 3: Lazo/Moño
                tintColor = new Color(0.7f, 0.95f, 0.8f); // Menta Pastel
                selectedAccessory = GetBowSprite();
                offset = new Vector3(0.18f, 0.42f, -0.1f); // Desplazado a un lado de la cabeza
                scale = Vector3.one * 0.6f;
                break;
        }

        characterSr.color = tintColor;

        if (selectedAccessory != null)
        {
            CrearOActualizarAccesorio(selectedAccessory, offset, scale);
        }
        else if (accessoryObj != null)
        {
            Destroy(accessoryObj);
        }
    }

    private void CrearOActualizarAccesorio(Sprite sprite, Vector3 offset, Vector3 scale)
    {
        if (accessoryObj == null)
        {
            // Crear el GameObject hijo para renderizar el accesorio (sombrero/corona)
            accessoryObj = new GameObject("CharacterAccessory");
            accessoryObj.transform.SetParent(transform, false);
            accessorySr = accessoryObj.AddComponent<SpriteRenderer>();
        }

        accessoryObj.transform.localPosition = offset;
        accessoryObj.transform.localScale = scale;

        if (accessorySr != null)
        {
            accessorySr.sprite = sprite;
            // Asegurarse de que el accesorio se renderice encima del cuerpo del personaje
            accessorySr.sortingLayerName = characterSr.sortingLayerName;
            accessorySr.sortingLayerID = characterSr.sortingLayerID;
            accessorySr.sortingOrder = characterSr.sortingOrder + 1;
        }
    }

    // --- GENERADORES PROCEDURALES DE TEXTURAS DE ACCESORIOS ---

    private Sprite GetCrownSprite()
    {
        if (crownSprite != null) return crownSprite;

        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color transparent = new Color(0f, 0f, 0f, 0f);
        Color gold = new Color(1.0f, 0.85f, 0.1f); // Oro
        Color ruby = new Color(0.9f, 0.1f, 0.2f); // Rubí central

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Dibujar forma de corona (base rectangular + 3 picos)
                bool enBase = (y >= 4 && y <= 10 && x >= 4 && x <= 28);
                bool enPicoIzquierdo = (y > 10 && y <= 24 && x >= 4 && x <= 10 && (y - 10) >= (x - 4));
                bool enPicoDerecho = (y > 10 && y <= 24 && x >= 22 && x <= 28 && (y - 10) >= (28 - x));
                bool enPicoCentral = (y > 10 && y <= 28 && x >= 12 && x <= 20 && (y - 10) >= Mathf.Abs(x - 16) * 2f);

                if (enBase || enPicoIzquierdo || enPicoDerecho || enPicoCentral)
                {
                    // Añadir detalle de rubí en el centro
                    if (y >= 7 && y <= 12 && x >= 14 && x <= 18)
                    {
                        texture.SetPixel(x, y, ruby);
                    }
                    else
                    {
                        texture.SetPixel(x, y, gold);
                    }
                }
                else
                {
                    texture.SetPixel(x, y, transparent);
                }
            }
        }
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        crownSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return crownSprite;
    }

    private Sprite GetHatSprite()
    {
        if (hatSprite != null) return hatSprite;

        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color transparent = new Color(0f, 0f, 0f, 0f);
        Color hatColor = new Color(0.2f, 0.2f, 0.3f); // Gris oscuro
        Color ribbonColor = new Color(1.0f, 0.4f, 0.5f); // Cinta rosa

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Ala del sombrero (base ancha)
                bool enAla = (y >= 4 && y <= 8 && x >= 2 && x <= 30);
                // Copa del sombrero
                bool enCopa = (y > 8 && y <= 26 && x >= 7 && x <= 25);
                // Cinta decorativa
                bool enCinta = (y >= 9 && y <= 12 && x >= 7 && x <= 25);

                if (enAla || enCopa)
                {
                    if (enCinta)
                    {
                        texture.SetPixel(x, y, ribbonColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, hatColor);
                    }
                }
                else
                {
                    texture.SetPixel(x, y, transparent);
                }
            }
        }
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        hatSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return hatSprite;
    }

    private Sprite GetBowSprite()
    {
        if (bowSprite != null) return bowSprite;

        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color transparent = new Color(0f, 0f, 0f, 0f);
        Color bowColor = new Color(0.95f, 0.5f, 0.65f); // Rosa fucsia pastel
        Color centerColor = new Color(1.0f, 0.85f, 0.9f); // Nudo central rosa claro

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Dibujar un moño (dos triángulos laterales y un círculo en el medio)
                float dx = x - 16;
                float dy = y - 16;
                bool enNudo = (dx * dx + dy * dy <= 16f); // Círculo de radio 4 en el centro
                bool enLazoIzquierdo = (dx < -2 && y >= 8 && y <= 24 && Mathf.Abs(dy) <= Mathf.Abs(dx) * 0.7f);
                bool enLazoDerecho = (dx > 2 && y >= 8 && y <= 24 && Mathf.Abs(dy) <= Mathf.Abs(dx) * 0.7f);

                if (enNudo)
                {
                    texture.SetPixel(x, y, centerColor);
                }
                else if (enLazoIzquierdo || enLazoDerecho)
                {
                    texture.SetPixel(x, y, bowColor);
                }
                else
                {
                    texture.SetPixel(x, y, transparent);
                }
            }
        }
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        bowSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return bowSprite;
    }
}
