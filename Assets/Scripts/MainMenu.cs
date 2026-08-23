using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Nombre de la escena de juego que se cargará al pulsar jugar.")]
    public string nombreEscenaJuego = "Level1";

    [Header("Paneles de Interfaz")]
    [Tooltip("El panel que muestra las opciones/controles del juego.")]
    public GameObject panelOpciones;

    void Start()
    {
        // Asegurarse de que el panel de opciones comience desactivado
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        // Asegurarse de que la música del AudioManager se reproduzca si existe
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM();
        }

        // Configurar los botones de inicio programáticamente para garantizar su funcionamiento
        ConfigurarBotonesMenu();
    }

    private void ConfigurarBotonesMenu()
    {
        Button[] botones = FindObjectsOfType<Button>(true);
        foreach (Button btn in botones)
        {
            string nombre = btn.gameObject.name.ToLower();
            
            // Botón Jugar / Play
            if (nombre.Contains("jugar") || nombre.Contains("play") || nombre.Contains("start") || nombre.Contains("inicio"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(Jugar);
                Debug.Log($"[MainMenu] Botón '{btn.gameObject.name}' configurado para Jugar.");
            }
            // Botón Salir / Quit
            else if (nombre.Contains("salir") || nombre.Contains("quit") || nombre.Contains("exit"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(Salir);
                Debug.Log($"[MainMenu] Botón '{btn.gameObject.name}' configurado para Salir.");
            }
            // Botón Opciones / Settings
            else if (nombre.Contains("opciones") || nombre.Contains("config") || nombre.Contains("settings"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(AbrirOpciones);
                Debug.Log($"[MainMenu] Botón '{btn.gameObject.name}' configurado para Opciones.");
            }
        }
    }

    // Carga la escena de juego principal
    public void Jugar()
    {
        string escenaCargar = nombreEscenaJuego;
        if (escenaCargar == "SampleScene" || string.IsNullOrEmpty(escenaCargar))
        {
            escenaCargar = "Level1";
        }
        SceneManager.LoadScene(escenaCargar);
    }

    // Muestra el panel de opciones
    public void AbrirOpciones()
    {
        if (GameSettingsUI.Instance != null)
        {
            GameSettingsUI.Instance.AbrirPanelAjustes();
        }
        else if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);
        }
    }

    // Oculta el panel de opciones
    public void CerrarOpciones()
    {
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }
    }

    // Cierra la aplicación compilada
    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
