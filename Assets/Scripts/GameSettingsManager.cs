using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    [Header("Configuraciones Guardadas")]
    public string lenguaje = "ES"; // "ES" o "EN"
    public float volumenMusica = 0.4f;
    public float volumenSFX = 0.5f;
    public string nombreP1 = "Jugador 1";
    public string nombreP2 = "Jugador 2";
    public int avatarP1 = 0;
    public int avatarP2 = 1;
    public bool multijugadorActivo = false;
    public bool modoOscuro = false;

    // Delegados para notificar cambios
    public delegate void OnSettingsChanged();
    public event OnSettingsChanged SettingsChangedEvent;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CargarConfiguraciones();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CargarConfiguraciones()
    {
        lenguaje = PlayerPrefs.GetString("Lenguaje", "ES");
        volumenMusica = PlayerPrefs.GetFloat("VolumenMusica", 0.4f);
        volumenSFX = PlayerPrefs.GetFloat("VolumenSFX", 0.5f);
        nombreP1 = PlayerPrefs.GetString("NombreP1", "Jugador 1");
        nombreP2 = PlayerPrefs.GetString("NombreP2", "Jugador 2");
        avatarP1 = PlayerPrefs.GetInt("AvatarP1", 0);
        avatarP2 = PlayerPrefs.GetInt("AvatarP2", 1);
        multijugadorActivo = PlayerPrefs.GetInt("MultijugadorActivo", 0) == 1;
        modoOscuro = PlayerPrefs.GetInt("ModoOscuro", 0) == 1;
    }

    public void GuardarConfiguraciones()
    {
        PlayerPrefs.SetString("Lenguaje", lenguaje);
        PlayerPrefs.SetFloat("VolumenMusica", volumenMusica);
        PlayerPrefs.SetFloat("VolumenSFX", volumenSFX);
        PlayerPrefs.SetString("NombreP1", nombreP1);
        PlayerPrefs.SetString("NombreP2", nombreP2);
        PlayerPrefs.SetInt("AvatarP1", avatarP1);
        PlayerPrefs.SetInt("AvatarP2", avatarP2);
        PlayerPrefs.SetInt("MultijugadorActivo", multijugadorActivo ? 1 : 0);
        PlayerPrefs.SetInt("ModoOscuro", modoOscuro ? 1 : 0);
        PlayerPrefs.Save();

        // Notificar cambios
        SettingsChangedEvent?.Invoke();
    }

    // Método para obtener el color del avatar según su índice
    public Color GetAvatarColor(int index)
    {
        switch (index)
        {
            case 0: return new Color(1f, 0.7f, 0.75f); // Sweet Cherry (Rosa)
            case 1: return new Color(0.7f, 0.85f, 1f); // Cotton Candy (Celeste)
            case 2: return new Color(0.85f, 0.75f, 1f); // Marshmallow (Lila)
            case 3: return new Color(1f, 0.9f, 0.7f);   // Honey Berry (Amarillo)
            default: return Color.white;
        }
    }

    // Nombre descriptivo del avatar
    public string GetAvatarName(int index)
    {
        if (lenguaje == "ES")
        {
            switch (index)
            {
                case 0: return "Cereza Dulce";
                case 1: return "Algodón de Azúcar";
                case 2: return "Malvavisco";
                case 3: return "Miel y Baya";
                default: return "Desconocido";
            }
        }
        else
        {
            switch (index)
            {
                case 0: return "Sweet Cherry";
                case 1: return "Cotton Candy";
                case 2: return "Marshmallow";
                case 3: return "Honey Berry";
                default: return "Unknown";
            }
        }
    }
}
