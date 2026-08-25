using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameSettingsUI : MonoBehaviour
{
    public static GameSettingsUI Instance { get; private set; }

    private GameObject panelBloqueador;
    private GameObject panelConfig;
    private TMP_InputField inputP1;
    private TMP_InputField inputP2;
    private TextMeshProUGUI txtMultijugadorEstado;
    private TextMeshProUGUI txtDarkModeEstado;
    private TextMeshProUGUI txtInstrucciones;
    private TextMeshProUGUI txtRecords;
    
    private Image imgAvatarP1;
    private Image imgAvatarP2;

    private Slider sliderMusica;
    private Slider sliderSFX;

    private int tempAvatarP1;
    private int tempAvatarP2;
    private bool tempMultiplayer;
    private bool tempDarkMode;
    private float tempVolMusica;
    private float tempVolSFX;
    private GameObject panelConfirmacion;

    private float originalVolMusica;
    private float originalVolSFX;
    private bool originalMultiplayer;
    private bool originalDarkMode;
    private int originalAvatarP1;
    private int originalAvatarP2;
    private string originalNombreP1;
    private string originalNombreP2;

    private Button btnOpcionesMenuOriginal;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (Instance != this) return;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        ConfigurarBotonMenuPrincipal();
        CrearBotonAjustesFlotante();
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        ConfigurarBotonMenuPrincipal();
        CrearBotonAjustesFlotante();
    }

    // Encuentra el botón "Opciones" original del MainMenu e intercepta su comportamiento
    private void ConfigurarBotonMenuPrincipal()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu" || sceneName == (GameManager.Instance != null ? GameManager.Instance.escenaMenu : ""))
        {
            Button[] botones = FindObjectsOfType<Button>(true);
            foreach (Button btn in botones)
            {
                string nombre = btn.gameObject.name.ToLower();
                if (nombre.Contains("opciones") || nombre.Contains("config") || nombre.Contains("settings"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => { if (Instance != null) Instance.AbrirPanelAjustes(); });
                    btnOpcionesMenuOriginal = btn;
                }
            }
        }
    }

    // Crea un botón flotante de ajustes (⚙️) en las escenas de juego en ejecución
    private void CrearBotonAjustesFlotante()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu" || sceneName == (GameManager.Instance != null ? GameManager.Instance.escenaMenu : ""))
        {
            return; // No en el menú principal
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // Comprobar si ya existe
        if (canvas.transform.Find("BotonAjustesFlotante") != null) return;

        GameObject btnObj = new GameObject("BotonAjustesFlotante", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(canvas.transform, false);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(15f, -15f);
        rect.sizeDelta = new Vector2(45f, 45f);

        Image img = btnObj.GetComponent<Image>();
        img.color = new Color(1.0f, 0.88f, 0.92f); // Rosa pastel
        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.85f, 0.7f, 0.75f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { if (Instance != null) Instance.AbrirPanelAjustes(); });

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI txt = textObj.GetComponent<TextMeshProUGUI>();
        txt.text = "⚙️";
        txt.fontSize = 24;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = new Color(0.25f, 0.15f, 0.2f);

        RectTransform txtRect = textObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;
    }

    public void AbrirPanelAjustes()
    {
        // Si ya había un panel de ajustes abierto (p. ej. por un doble clic, o por
        // el botón "Opciones" y el botón flotante disparándose casi a la vez),
        // lo destruimos primero. Sin esto, quedaba un panel bloqueador invisible
        // "fantasma" en el Canvas que interceptaba los clics de otros botones.
        if (panelBloqueador != null)
        {
            Destroy(panelBloqueador);
            panelBloqueador = null;
            panelConfig = null;
        }

        // Detener tiempo si estamos en juego
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName != "MainMenu" && sceneName != (GameManager.Instance != null ? GameManager.Instance.escenaMenu : ""))
        {
            Time.timeScale = 0f;
        }

        // Cargar variables temporales y respaldar originales
        if (GameSettingsManager.Instance != null)
        {
            originalVolMusica = GameSettingsManager.Instance.volumenMusica;
            originalVolSFX = GameSettingsManager.Instance.volumenSFX;
            originalMultiplayer = GameSettingsManager.Instance.multijugadorActivo;
            originalDarkMode = GameSettingsManager.Instance.modoOscuro;
            originalAvatarP1 = GameSettingsManager.Instance.avatarP1;
            originalAvatarP2 = GameSettingsManager.Instance.avatarP2;
            originalNombreP1 = GameSettingsManager.Instance.nombreP1;
            originalNombreP2 = GameSettingsManager.Instance.nombreP2;

            tempAvatarP1 = originalAvatarP1;
            tempAvatarP2 = originalAvatarP2;
            tempMultiplayer = originalMultiplayer;
            tempDarkMode = originalDarkMode;
            tempVolMusica = originalVolMusica;
            tempVolSFX = originalVolSFX;
        }

        CrearPanelAjustesUI();
    }

    private void CrearPanelAjustesUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            // Si no hay Canvas en la escena por alguna razón, creamos uno
            GameObject canvasObj = new GameObject("CanvasSettings", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // 1. Panel Bloqueador
        panelBloqueador = new GameObject("BlockerPanel", typeof(RectTransform), typeof(Image));
        panelBloqueador.transform.SetParent(canvas.transform, false);
        RectTransform blockerRect = panelBloqueador.GetComponent<RectTransform>();
        blockerRect.anchorMin = Vector2.zero;
        blockerRect.anchorMax = Vector2.one;
        blockerRect.sizeDelta = Vector2.zero;
        
        Image blockerImg = panelBloqueador.GetComponent<Image>();
        blockerImg.color = new Color(0.2f, 0.15f, 0.18f, 0.6f); // Oscuro translúcido pastel

        // 2. Caja Central de Configuración (Expandida para dar espacio a los récords)
        panelConfig = new GameObject("ConfigBox", typeof(RectTransform), typeof(Image));
        panelConfig.transform.SetParent(panelBloqueador.transform, false);
        RectTransform boxRect = panelConfig.GetComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0.5f, 0.5f);
        boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.pivot = new Vector2(0.5f, 0.5f);
        boxRect.sizeDelta = new Vector2(580f, 560f);

        Image boxImg = panelConfig.GetComponent<Image>();
        boxImg.color = new Color(1.0f, 0.92f, 0.95f, 0.96f); // Rosa suave
        Outline outline = panelConfig.AddComponent<Outline>();
        outline.effectColor = new Color(0.85f, 0.7f, 0.75f);
        outline.effectDistance = new Vector2(3f, -3f);

        // --- TÍTULO ---
        CrearTexto("Titulo", panelConfig.transform, new Vector2(0f, 490f), new Vector2(580f, 50f), 
                   LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Opciones") : "Ajustes y Perfil", 
                   22, TextAlignmentOptions.Center, true);

        // --- BOTÓN CERRAR / VOLVER (X) --- Permite regresar sin necesidad de guardar
        GameObject btnClose = new GameObject("BtnCloseX", typeof(RectTransform), typeof(Image), typeof(Button));
        btnClose.transform.SetParent(panelConfig.transform, false);
        RectTransform rClose = btnClose.GetComponent<RectTransform>();
        rClose.anchorMin = new Vector2(1f, 1f);
        rClose.anchorMax = new Vector2(1f, 1f);
        rClose.pivot = new Vector2(1f, 1f);
        rClose.anchoredPosition = new Vector2(-12f, -12f);
        rClose.sizeDelta = new Vector2(34f, 34f);
        btnClose.GetComponent<Image>().color = new Color(1f, 0.82f, 0.86f);
        btnClose.AddComponent<Outline>().effectColor = new Color(0.85f, 0.7f, 0.75f);

        GameObject txtCloseObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtCloseObj.transform.SetParent(btnClose.transform, false);
        TextMeshProUGUI txtClose = txtCloseObj.GetComponent<TextMeshProUGUI>();
        txtClose.text = "X";
        txtClose.fontSize = 16;
        txtClose.fontWeight = FontWeight.Bold;
        txtClose.color = new Color(0.25f, 0.15f, 0.2f);
        txtClose.alignment = TextAlignmentOptions.Center;
        RectTransform rtClose = txtCloseObj.GetComponent<RectTransform>();
        rtClose.anchorMin = Vector2.zero; rtClose.anchorMax = Vector2.one; rtClose.sizeDelta = Vector2.zero;

        btnClose.GetComponent<Button>().onClick.AddListener(MostrarConfirmacionSalir);

        // --- IDIOMA ---
        CrearTexto("LangLabel", panelConfig.transform, new Vector2(30f, 440f), new Vector2(150f, 30f), "Idioma / Language:", 14, TextAlignmentOptions.Left, true);
        CrearBotonIdioma("BtnES", panelConfig.transform, new Vector2(180f, 440f), "Español", "ES");
        CrearBotonIdioma("BtnEN", panelConfig.transform, new Vector2(290f, 440f), "English", "EN");

        // --- VOLUMEN MÚSICA ---
        string txtMusica = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Volumen Música") : "Volumen Música";
        CrearTexto("VolMusicaLabel", panelConfig.transform, new Vector2(30f, 400f), new Vector2(150f, 30f), txtMusica + ":", 14, TextAlignmentOptions.Left, true);
        sliderMusica = CrearSlider("SliderMusica", panelConfig.transform, new Vector2(180f, 400f), new Vector2(200f, 20f), GameSettingsManager.Instance != null ? GameSettingsManager.Instance.volumenMusica : 0.4f);
        sliderMusica.onValueChanged.AddListener((val) => {
            tempVolMusica = val;
            if (GameSettingsManager.Instance != null) GameSettingsManager.Instance.volumenMusica = val;
            ActualizarVolumenesFisicos();
        });

        // --- VOLUMEN SFX ---
        string txtSFX = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Volumen SFX") : "Volumen SFX";
        CrearTexto("VolSFXLabel", panelConfig.transform, new Vector2(30f, 360f), new Vector2(150f, 30f), txtSFX + ":", 14, TextAlignmentOptions.Left, true);
        sliderSFX = CrearSlider("SliderSFX", panelConfig.transform, new Vector2(180f, 360f), new Vector2(200f, 20f), GameSettingsManager.Instance != null ? GameSettingsManager.Instance.volumenSFX : 0.5f);
        sliderSFX.onValueChanged.AddListener((val) => {
            tempVolSFX = val;
            if (GameSettingsManager.Instance != null) GameSettingsManager.Instance.volumenSFX = val;
            ActualizarVolumenesFisicos();
        });

        // --- TOGGLE MULTIJUGADOR ---
        string txtMulti = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Multijugador:") : "Multijugador:";
        CrearTexto("MultiplayerLabel", panelConfig.transform, new Vector2(30f, 320f), new Vector2(100f, 30f), txtMulti, 14, TextAlignmentOptions.Left, true);
        
        GameObject btnMulti = new GameObject("BtnMultiplayer", typeof(RectTransform), typeof(Image), typeof(Button));
        btnMulti.transform.SetParent(panelConfig.transform, false);
        RectTransform rMulti = btnMulti.GetComponent<RectTransform>();
        rMulti.anchorMin = new Vector2(0f, 0f);
        rMulti.anchorMax = new Vector2(0f, 0f);
        rMulti.pivot = new Vector2(0f, 0.5f);
        rMulti.anchoredPosition = new Vector2(140f, 320f);
        rMulti.sizeDelta = new Vector2(110f, 30f);
        btnMulti.GetComponent<Image>().color = new Color(1f, 0.88f, 0.92f);
        btnMulti.AddComponent<Outline>().effectColor = new Color(0.85f, 0.7f, 0.75f);
        
        GameObject txtMultiStateObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtMultiStateObj.transform.SetParent(btnMulti.transform, false);
        txtMultijugadorEstado = txtMultiStateObj.GetComponent<TextMeshProUGUI>();
        txtMultijugadorEstado.fontSize = 12;
        txtMultijugadorEstado.color = new Color(0.25f, 0.15f, 0.2f);
        txtMultijugadorEstado.alignment = TextAlignmentOptions.Center;
        RectTransform rtMS = txtMultiStateObj.GetComponent<RectTransform>();
        rtMS.anchorMin = Vector2.zero; rtMS.anchorMax = Vector2.one; rtMS.sizeDelta = Vector2.zero;

        ActualizarTextoBotonMultiplayer();

        btnMulti.GetComponent<Button>().onClick.AddListener(() => {
            tempMultiplayer = !tempMultiplayer;
            if (GameSettingsManager.Instance != null) GameSettingsManager.Instance.multijugadorActivo = tempMultiplayer;
            ActualizarTextoBotonMultiplayer();
            ActualizarUIFisicaPerfiles();
        });

        // --- TOGGLE MODO OSCURO ---
        string txtDark = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Modo Oscuro:") : "Modo Oscuro:";
        CrearTexto("DarkModeLabel", panelConfig.transform, new Vector2(290f, 320f), new Vector2(120f, 30f), txtDark, 14, TextAlignmentOptions.Left, true);

        GameObject btnDark = new GameObject("BtnDarkMode", typeof(RectTransform), typeof(Image), typeof(Button));
        btnDark.transform.SetParent(panelConfig.transform, false);
        RectTransform rDark = btnDark.GetComponent<RectTransform>();
        rDark.anchorMin = new Vector2(0f, 0f);
        rDark.anchorMax = new Vector2(0f, 0f);
        rDark.pivot = new Vector2(0f, 0.5f);
        rDark.anchoredPosition = new Vector2(420f, 320f);
        rDark.sizeDelta = new Vector2(110f, 30f);
        btnDark.GetComponent<Image>().color = new Color(1f, 0.88f, 0.92f);
        btnDark.AddComponent<Outline>().effectColor = new Color(0.85f, 0.7f, 0.75f);

        GameObject txtDarkStateObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtDarkStateObj.transform.SetParent(btnDark.transform, false);
        txtDarkModeEstado = txtDarkStateObj.GetComponent<TextMeshProUGUI>();
        txtDarkModeEstado.fontSize = 12;
        txtDarkModeEstado.color = new Color(0.25f, 0.15f, 0.2f);
        txtDarkModeEstado.alignment = TextAlignmentOptions.Center;
        RectTransform rtDS = txtDarkStateObj.GetComponent<RectTransform>();
        rtDS.anchorMin = Vector2.zero; rtDS.anchorMax = Vector2.one; rtDS.sizeDelta = Vector2.zero;

        ActualizarTextoBotonDarkMode();

        btnDark.GetComponent<Button>().onClick.AddListener(() => {
            tempDarkMode = !tempDarkMode;
            if (GameSettingsManager.Instance != null)
            {
                GameSettingsManager.Instance.modoOscuro = tempDarkMode;
                AestheticManager am = FindFirstObjectByType<AestheticManager>();
                if (am != null)
                {
                    am.AplicarEstiloUI();
                }
            }
            ActualizarTextoBotonDarkMode();
        });

        // --- SECCIONES DE PERFIL JUGADOR ---
        CrearSeccionPerfiles();

        // --- INSTRUCCIONES ---
        CrearSeccionInstrucciones();

        // --- RÉCORDS LOCALES ---
        CrearSeccionRecords();

        // --- BOTÓN GUARDAR Y CERRAR ---
        GameObject btnSave = new GameObject("BtnSave", typeof(RectTransform), typeof(Image), typeof(Button));
        btnSave.transform.SetParent(panelConfig.transform, false);
        RectTransform rSave = btnSave.GetComponent<RectTransform>();
        rSave.anchorMin = new Vector2(0.5f, 0f);
        rSave.anchorMax = new Vector2(0.5f, 0f);
        rSave.pivot = new Vector2(0.5f, 0f);
        rSave.anchoredPosition = new Vector2(0f, 15f);
        rSave.sizeDelta = new Vector2(200f, 40f);
        btnSave.GetComponent<Image>().color = new Color(1.0f, 0.82f, 0.86f); // Rosa más subido
        btnSave.AddComponent<Outline>().effectColor = new Color(0.85f, 0.7f, 0.75f);
        
        GameObject txtSaveObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtSaveObj.transform.SetParent(btnSave.transform, false);
        TextMeshProUGUI txtSave = txtSaveObj.GetComponent<TextMeshProUGUI>();
        txtSave.text = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Guardar y Cerrar") : "Guardar y Cerrar";
        txtSave.fontSize = 14;
        txtSave.fontWeight = FontWeight.Bold;
        txtSave.color = new Color(0.25f, 0.15f, 0.2f);
        txtSave.alignment = TextAlignmentOptions.Center;
        RectTransform rtS = txtSaveObj.GetComponent<RectTransform>();
        rtS.anchorMin = Vector2.zero; rtS.anchorMax = Vector2.one; rtS.sizeDelta = Vector2.zero;

        btnSave.GetComponent<Button>().onClick.AddListener(GuardarYCerrar);

        // Estilizar con AestheticManager si existe
        AestheticManager am = FindObjectOfType<AestheticManager>();
        if (am != null)
        {
            am.AplicarEstiloUI();
        }
    }

    private void CrearSeccionPerfiles()
    {
        // --- JUGADOR 1 ---
        // Nombre P1 Label
        string lblP1 = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Nombre P1:") : "Nombre P1:";
        CrearTexto("LabelP1Name", panelConfig.transform, new Vector2(30f, 270f), new Vector2(90f, 25f), lblP1, 12, TextAlignmentOptions.Left, true);
        
        // Input P1
        inputP1 = CrearInputField("InputP1", panelConfig.transform, new Vector2(120f, 270f), new Vector2(130f, 25f), 
                                   GameSettingsManager.Instance != null ? GameSettingsManager.Instance.nombreP1 : "Jugador 1");

        // Avatar P1 Label
        string lblAvP1 = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Avatar P1:") : "Avatar P1:";
        CrearTexto("LabelP1Avatar", panelConfig.transform, new Vector2(30f, 235f), new Vector2(90f, 25f), lblAvP1, 12, TextAlignmentOptions.Left, true);

        // Avatar P1 Color Preview
        GameObject p1AvObj = new GameObject("P1AvatarImg", typeof(RectTransform), typeof(Image));
        p1AvObj.transform.SetParent(panelConfig.transform, false);
        RectTransform rtAv1 = p1AvObj.GetComponent<RectTransform>();
        rtAv1.anchorMin = Vector2.zero; rtAv1.anchorMax = Vector2.zero; rtAv1.pivot = new Vector2(0f, 0.5f);
        rtAv1.anchoredPosition = new Vector2(120f, 235f);
        rtAv1.sizeDelta = new Vector2(25f, 25f);
        imgAvatarP1 = p1AvObj.GetComponent<Image>();
        imgAvatarP1.color = GameSettingsManager.Instance != null ? GameSettingsManager.Instance.GetAvatarColor(tempAvatarP1) : Color.white;
        p1AvObj.AddComponent<Outline>().effectColor = Color.white;

        // Botón Cambiar Avatar P1
        GameObject btnP1Av = new GameObject("BtnP1Av", typeof(RectTransform), typeof(Image), typeof(Button));
        btnP1Av.transform.SetParent(panelConfig.transform, false);
        RectTransform rtB1 = btnP1Av.GetComponent<RectTransform>();
        rtB1.anchorMin = Vector2.zero; rtB1.anchorMax = Vector2.zero; rtB1.pivot = new Vector2(0f, 0.5f);
        rtB1.anchoredPosition = new Vector2(155f, 235f);
        rtB1.sizeDelta = new Vector2(95f, 25f);
        btnP1Av.GetComponent<Image>().color = new Color(1f, 0.88f, 0.92f);
        btnP1Av.AddComponent<Outline>().effectColor = new Color(0.85f, 0.7f, 0.75f);
        
        GameObject txtB1Obj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtB1Obj.transform.SetParent(btnP1Av.transform, false);
        TextMeshProUGUI txtB1 = txtB1Obj.GetComponent<TextMeshProUGUI>();
        txtB1.fontSize = 10; txtB1.color = new Color(0.25f, 0.15f, 0.2f); txtB1.alignment = TextAlignmentOptions.Center;
        txtB1.text = GameSettingsManager.Instance != null ? GameSettingsManager.Instance.GetAvatarName(tempAvatarP1) : "Avatar 1";
        RectTransform rtT1 = txtB1Obj.GetComponent<RectTransform>();
        rtT1.anchorMin = Vector2.zero; rtT1.anchorMax = Vector2.one; rtT1.sizeDelta = Vector2.zero;

        Button botonAvatarP1 = btnP1Av.GetComponent<Button>();

        botonAvatarP1.onClick.RemoveAllListeners();

        botonAvatarP1.onClick.AddListener(() =>
        {
            tempAvatarP1 = (tempAvatarP1 + 1) % 4;

            if (imgAvatarP1 != null && GameSettingsManager.Instance != null)
            {
                imgAvatarP1.color = GameSettingsManager.Instance.GetAvatarColor(tempAvatarP1);
            }

            if (txtB1 != null && GameSettingsManager.Instance != null)
            {
                txtB1.text = GameSettingsManager.Instance.GetAvatarName(tempAvatarP1);
            }
        });

        // --- JUGADOR 2 (Oculto o Visible dinámicamente) ---
        // Label
        string lblP2 = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Nombre P2:") : "Nombre P2:";
        CrearTexto("LabelP2Name", panelConfig.transform, new Vector2(300f, 270f), new Vector2(90f, 25f), lblP2, 12, TextAlignmentOptions.Left, true);
        
        // Input P2
        inputP2 = CrearInputField("InputP2", panelConfig.transform, new Vector2(390f, 270f), new Vector2(130f, 25f), 
                                   GameSettingsManager.Instance != null ? GameSettingsManager.Instance.nombreP2 : "Jugador 2");

        // Avatar P2 Label
        string lblAvP2 = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("Avatar P2:") : "Avatar P2:";
        CrearTexto("LabelP2Avatar", panelConfig.transform, new Vector2(300f, 235f), new Vector2(90f, 25f), lblAvP2, 12, TextAlignmentOptions.Left, true);

        // Avatar P2 Color Preview
        GameObject p2AvObj = new GameObject("P2AvatarImg", typeof(RectTransform), typeof(Image));
        p2AvObj.transform.SetParent(panelConfig.transform, false);
        RectTransform rtAv2 = p2AvObj.GetComponent<RectTransform>();
        rtAv2.anchorMin = Vector2.zero; rtAv2.anchorMax = Vector2.zero; rtAv2.pivot = new Vector2(0f, 0.5f);
        rtAv2.anchoredPosition = new Vector2(390f, 235f);
        rtAv2.sizeDelta = new Vector2(25f, 25f);
        imgAvatarP2 = p2AvObj.GetComponent<Image>();
        imgAvatarP2.color = GameSettingsManager.Instance != null ? GameSettingsManager.Instance.GetAvatarColor(tempAvatarP2) : Color.white;
        p2AvObj.AddComponent<Outline>().effectColor = Color.white;

        // Botón Cambiar Avatar P2
        GameObject btnP2Av = new GameObject("BtnP2Av", typeof(RectTransform), typeof(Image), typeof(Button));
        btnP2Av.transform.SetParent(panelConfig.transform, false);
        RectTransform rtB2 = btnP2Av.GetComponent<RectTransform>();
        rtB2.anchorMin = Vector2.zero; rtB2.anchorMax = Vector2.zero; rtB2.pivot = new Vector2(0f, 0.5f);
        rtB2.anchoredPosition = new Vector2(425f, 235f);
        rtB2.sizeDelta = new Vector2(95f, 25f);
        btnP2Av.GetComponent<Image>().color = new Color(1f, 0.88f, 0.92f);
        btnP2Av.AddComponent<Outline>().effectColor = new Color(0.85f, 0.7f, 0.75f);
        
        GameObject txtB2Obj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtB2Obj.transform.SetParent(btnP2Av.transform, false);
        TextMeshProUGUI txtB2 = txtB2Obj.GetComponent<TextMeshProUGUI>();
        txtB2.fontSize = 10; txtB2.color = new Color(0.25f, 0.15f, 0.2f); txtB2.alignment = TextAlignmentOptions.Center;
        txtB2.text = GameSettingsManager.Instance != null ? GameSettingsManager.Instance.GetAvatarName(tempAvatarP2) : "Avatar 2";
        RectTransform rtT2 = txtB2Obj.GetComponent<RectTransform>();
        rtT2.anchorMin = Vector2.zero; rtT2.anchorMax = Vector2.one; rtT2.sizeDelta = Vector2.zero;

        Button botonAvatarP2 = btnP2Av.GetComponent<Button>();
        botonAvatarP2.onClick.RemoveAllListeners();
        botonAvatarP2.onClick.AddListener(() => {
            tempAvatarP2 = (tempAvatarP2 + 1) % 4;
            if (imgAvatarP2 != null && GameSettingsManager.Instance != null)
            {
                imgAvatarP2.color = GameSettingsManager.Instance.GetAvatarColor(tempAvatarP2);
            }
            if (txtB2 != null && GameSettingsManager.Instance != null)
            {
                txtB2.text = GameSettingsManager.Instance.GetAvatarName(tempAvatarP2);
            }
        });

        ActualizarUIFisicaPerfiles();
    }

    private void ActualizarUIFisicaPerfiles()
    {
        // Activa o desactiva visualmente los inputs de P2
        bool multi = tempMultiplayer;
        
        Transform lP2N = panelConfig.transform.Find("LabelP2Name");
        Transform iP2N = panelConfig.transform.Find("InputP2");
        Transform lP2A = panelConfig.transform.Find("LabelP2Avatar");
        Transform iP2A = panelConfig.transform.Find("P2AvatarImg");
        Transform bP2A = panelConfig.transform.Find("BtnP2Av");

        if (lP2N != null) lP2N.gameObject.SetActive(multi);
        if (iP2N != null) iP2N.gameObject.SetActive(multi);
        if (lP2A != null) lP2A.gameObject.SetActive(multi);
        if (iP2A != null) iP2A.gameObject.SetActive(multi);
        if (bP2A != null) bP2A.gameObject.SetActive(multi);
    }

    private void CrearSeccionInstrucciones()
    {
        // Borde caja instrucciones
        GameObject borderObj = new GameObject("InstructionsFrame", typeof(RectTransform), typeof(Image));
        borderObj.transform.SetParent(panelConfig.transform, false);
        RectTransform rt = borderObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.zero; rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(30f, 145f);
        rt.sizeDelta = new Vector2(520f, 75f);
        
        Image img = borderObj.GetComponent<Image>();
        img.color = new Color(1f, 0.95f, 0.97f, 0.9f); // Blanco rosa suave
        borderObj.AddComponent<Outline>().effectColor = new Color(0.85f, 0.7f, 0.75f);

        // Texto de Instrucciones
        string txtInst = LocalizationManager.Instance != null ? LocalizationManager.Instance.ObtenerTexto("InstruccionesContenido") : "Jugador 1: A/D Mover, W/Espacio Saltar\nJugador 2: Flechas Mover, Flecha Arriba Saltar\n\nObjetivo: Recolecta caramelos, esquiva trampas y derrota al Dr. Cocoa.";
        
        GameObject textObj = new GameObject("TxtInstructions", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(borderObj.transform, false);
        txtInstrucciones = textObj.GetComponent<TextMeshProUGUI>();
        txtInstrucciones.text = txtInst;
        txtInstrucciones.fontSize = 9;
        txtInstrucciones.color = new Color(0.25f, 0.15f, 0.2f);
        txtInstrucciones.alignment = TextAlignmentOptions.TopLeft;

        RectTransform rTxt = textObj.GetComponent<RectTransform>();
        rTxt.anchorMin = Vector2.zero;
        rTxt.anchorMax = Vector2.one;
        rTxt.anchoredPosition = Vector2.zero;
        rTxt.sizeDelta = new Vector2(-10f, -10f); // 5px padding
    }

    private void ActualizarTextoBotonMultiplayer()
    {
        if (txtMultijugadorEstado == null) return;

        if (LocalizationManager.Instance != null && GameSettingsManager.Instance != null)
        {
            txtMultijugadorEstado.text = tempMultiplayer ?
                (GameSettingsManager.Instance.lenguaje == "ES" ? "2 JUGADORES" : "2 PLAYERS") :
                (GameSettingsManager.Instance.lenguaje == "ES" ? "1 JUGADOR" : "1 PLAYER");
        }
        else
        {
            txtMultijugadorEstado.text = tempMultiplayer ? "2 JUGADORES" : "1 JUGADOR";
        }
    }

    private void ActualizarTextoBotonDarkMode()
    {
        if (txtDarkModeEstado == null) return;

        if (LocalizationManager.Instance != null && GameSettingsManager.Instance != null)
        {
            txtDarkModeEstado.text = tempDarkMode ?
                (GameSettingsManager.Instance.lenguaje == "ES" ? "OSCURO" : "DARK") :
                (GameSettingsManager.Instance.lenguaje == "ES" ? "CLARO" : "LIGHT");
        }
        else
        {
            txtDarkModeEstado.text = tempDarkMode ? "OSCURO" : "CLARO";
        }
    }

    private void ActualizarVolumenesFisicos()
    {
        // AudioManager actualiza su volumen (llamada directa: más eficiente que SendMessage)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ActualizarVolumenAjustes();
        }
    }

    // Cierra el panel de ajustes SIN guardar los cambios temporales (botón Volver / X).
    public void CerrarPanelSinGuardar()
    {
        try
        {
            // Restaurar todos los valores originales en GameSettingsManager
            if (GameSettingsManager.Instance != null)
            {
                GameSettingsManager.Instance.volumenMusica = originalVolMusica;
                GameSettingsManager.Instance.volumenSFX = originalVolSFX;
                GameSettingsManager.Instance.multijugadorActivo = originalMultiplayer;
                GameSettingsManager.Instance.modoOscuro = originalDarkMode;
                GameSettingsManager.Instance.avatarP1 = originalAvatarP1;
                GameSettingsManager.Instance.avatarP2 = originalAvatarP2;
                GameSettingsManager.Instance.nombreP1 = originalNombreP1;
                GameSettingsManager.Instance.nombreP2 = originalNombreP2;
            }

            // Reaplicar volumen físico y estilos estéticos originales
            ActualizarVolumenesFisicos();
            AestheticManager am = FindFirstObjectByType<AestheticManager>();
            if (am != null)
            {
                am.AplicarEstiloUI();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameSettingsUI] Error al cerrar el panel sin guardar: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            if (panelBloqueador != null)
            {
                Destroy(panelBloqueador);
                panelBloqueador = null;
                panelConfig = null;
            }
            Time.timeScale = 1f;
        }
    }

    private void MostrarConfirmacionSalir()
    {
        // Si ya está abierto, no hacer nada
        if (panelConfirmacion != null) return;

        // Crear panel bloqueador para la confirmación (cubrirá todo)
        panelConfirmacion = new GameObject("PanelConfirmacionSalir", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelConfirmacion.transform.SetParent(panelConfig.transform, false);

        RectTransform rtP = panelConfirmacion.GetComponent<RectTransform>();
        rtP.anchorMin = Vector2.zero;
        rtP.anchorMax = Vector2.one;
        rtP.sizeDelta = Vector2.zero;

        // Fondo oscuro semitransparente (difuminado premium)
        Image imgBg = panelConfirmacion.GetComponent<Image>();
        imgBg.color = new Color(0.12f, 0.08f, 0.1f, 0.85f);

        // Caja de diálogo de confirmación
        GameObject dialogBox = new GameObject("DialogBox", typeof(RectTransform), typeof(Image));
        dialogBox.transform.SetParent(panelConfirmacion.transform, false);

        RectTransform rtD = dialogBox.GetComponent<RectTransform>();
        rtD.anchorMin = new Vector2(0.5f, 0.5f);
        rtD.anchorMax = new Vector2(0.5f, 0.5f);
        rtD.pivot = new Vector2(0.5f, 0.5f);
        rtD.anchoredPosition = Vector2.zero;
        rtD.sizeDelta = new Vector2(340f, 180f);

        Image imgBox = dialogBox.GetComponent<Image>();
        imgBox.color = new Color(1f, 0.92f, 0.94f);
        dialogBox.AddComponent<Outline>().effectColor = new Color(0.85f, 0.7f, 0.75f);

        // Texto de confirmación
        bool esES = GameSettingsManager.Instance != null && GameSettingsManager.Instance.lenguaje == "ES";
        string strPregunta = esES ? "¿Deseas salir sin guardar los cambios?" : "Exit without saving?";
        string strSi = esES ? "Sí, salir" : "Yes, exit";
        string strNo = esES ? "No, cancelar" : "No, cancel";

        GameObject txtPreguntaObj = new GameObject("TxtPregunta", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtPreguntaObj.transform.SetParent(dialogBox.transform, false);
        TextMeshProUGUI txtPregunta = txtPreguntaObj.GetComponent<TextMeshProUGUI>();
        txtPregunta.text = strPregunta;
        txtPregunta.fontSize = 14;
        txtPregunta.fontWeight = FontWeight.Bold;
        txtPregunta.color = new Color(0.25f, 0.15f, 0.2f);
        txtPregunta.alignment = TextAlignmentOptions.Center;

        RectTransform rtTxt = txtPreguntaObj.GetComponent<RectTransform>();
        rtTxt.anchorMin = new Vector2(0f, 0.5f);
        rtTxt.anchorMax = new Vector2(1f, 1f);
        rtTxt.pivot = new Vector2(0.5f, 0.5f);
        rtTxt.anchoredPosition = new Vector2(0f, -20f);
        rtTxt.sizeDelta = new Vector2(-40f, 0f);

        // Botón SÍ, SALIR
        GameObject btnSi = new GameObject("BtnSi", typeof(RectTransform), typeof(Image), typeof(Button));
        btnSi.transform.SetParent(dialogBox.transform, false);
        RectTransform rtSi = btnSi.GetComponent<RectTransform>();
        rtSi.anchorMin = new Vector2(0.25f, 0.25f);
        rtSi.anchorMax = new Vector2(0.25f, 0.25f);
        rtSi.pivot = new Vector2(0.5f, 0.5f);
        rtSi.anchoredPosition = new Vector2(0f, -10f);
        rtSi.sizeDelta = new Vector2(110f, 32f);

        btnSi.GetComponent<Image>().color = new Color(1f, 0.75f, 0.78f);
        btnSi.AddComponent<Outline>().effectColor = new Color(0.85f, 0.55f, 0.6f);

        GameObject txtSiObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtSiObj.transform.SetParent(btnSi.transform, false);
        TextMeshProUGUI txtSi = txtSiObj.GetComponent<TextMeshProUGUI>();
        txtSi.text = strSi;
        txtSi.fontSize = 11;
        txtSi.fontWeight = FontWeight.Bold;
        txtSi.color = new Color(0.25f, 0.15f, 0.2f);
        txtSi.alignment = TextAlignmentOptions.Center;

        RectTransform rtTSi = txtSiObj.GetComponent<RectTransform>();
        rtTSi.anchorMin = Vector2.zero; rtTSi.anchorMax = Vector2.one; rtTSi.sizeDelta = Vector2.zero;

        btnSi.GetComponent<Button>().onClick.AddListener(() => {
            Destroy(panelConfirmacion);
            CerrarPanelSinGuardar();
        });

        // Botón NO, CANCELAR
        GameObject btnNo = new GameObject("BtnNo", typeof(RectTransform), typeof(Image), typeof(Button));
        btnNo.transform.SetParent(dialogBox.transform, false);
        RectTransform rtNo = btnNo.GetComponent<RectTransform>();
        rtNo.anchorMin = new Vector2(0.75f, 0.25f);
        rtNo.anchorMax = new Vector2(0.75f, 0.25f);
        rtNo.pivot = new Vector2(0.5f, 0.5f);
        rtNo.anchoredPosition = new Vector2(0f, -10f);
        rtNo.sizeDelta = new Vector2(110f, 32f);

        btnNo.GetComponent<Image>().color = new Color(0.88f, 1f, 0.9f);
        btnNo.AddComponent<Outline>().effectColor = new Color(0.7f, 0.85f, 0.75f);

        GameObject txtNoObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtNoObj.transform.SetParent(btnNo.transform, false);
        TextMeshProUGUI txtNo = txtNoObj.GetComponent<TextMeshProUGUI>();
        txtNo.text = strNo;
        txtNo.fontSize = 11;
        txtNo.fontWeight = FontWeight.Bold;
        txtNo.color = new Color(0.25f, 0.15f, 0.2f);
        txtNo.alignment = TextAlignmentOptions.Center;

        RectTransform rtTNo = txtNoObj.GetComponent<RectTransform>();
        rtTNo.anchorMin = Vector2.zero; rtTNo.anchorMax = Vector2.one; rtTNo.sizeDelta = Vector2.zero;

        btnNo.GetComponent<Button>().onClick.AddListener(() => {
            Destroy(panelConfirmacion);
        });

        // Aplicar estilos a los botones
        AestheticManager am = FindFirstObjectByType<AestheticManager>();
        if (am != null)
        {
            am.AplicarEstiloUI();
        }
    }

    private void GuardarYCerrar()
    {
        // Todo el guardado va protegido: si algo falla a mitad de camino (p. ej. una
        // referencia nula inesperada) el panel igual se cierra y el tiempo se reanuda,
        // en lugar de quedar el juego "congelado" con Time.timeScale en 0 y el botón
        // de Guardar sin dar ninguna respuesta visible (el error que reportaban).
        try
        {
            if (GameSettingsManager.Instance != null)
            {
                if (inputP1 != null && !string.IsNullOrEmpty(inputP1.text))
                    GameSettingsManager.Instance.nombreP1 = inputP1.text;
                if (inputP2 != null && !string.IsNullOrEmpty(inputP2.text))
                    GameSettingsManager.Instance.nombreP2 = inputP2.text;

                GameSettingsManager.Instance.avatarP1 = tempAvatarP1;
                GameSettingsManager.Instance.avatarP2 = tempAvatarP2;
                GameSettingsManager.Instance.multijugadorActivo = tempMultiplayer;
                GameSettingsManager.Instance.modoOscuro = tempDarkMode;

                GameSettingsManager.Instance.GuardarConfiguraciones();
            }

            // Si estamos en juego y cambiamos la configuración de multijugador, podría requerir recargar la escena
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool enJuego = sceneName != "MainMenu" && sceneName != (GameManager.Instance != null ? GameManager.Instance.escenaMenu : "");

            // Si en juego cambia multijugador, reiniciamos el nivel actual para spawnear al P2 o removerlo
            if (enJuego && GameManager.Instance != null)
            {
                // Comprobamos si el número real de jugadores activos es diferente del configurado
                int jugadoresActivos = FindObjectsOfType<PlayerMovement>().Length;
                int jugadoresDeseados = tempMultiplayer ? 2 : 1;

                if (jugadoresActivos != jugadoresDeseados)
                {
                    GameManager.Instance.ReiniciarJuego();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameSettingsUI] Error al guardar los ajustes: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            // Pase lo que pase, cerrar el panel y reanudar el tiempo.
            if (panelBloqueador != null)
            {
                Destroy(panelBloqueador);
                panelBloqueador = null;
                panelConfig = null;
            }
            Time.timeScale = 1f;
        }
    }

    // Métodos auxiliares para crear UI por script
    private TextMeshProUGUI CrearTexto(string nombre, Transform parent, Vector2 pos, Vector2 size, string cont, float fontSize, TextAlignmentOptions align, bool bold = false)
    {
        GameObject obj = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.zero; rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        TextMeshProUGUI txt = obj.GetComponent<TextMeshProUGUI>();
        txt.text = cont;
        txt.fontSize = fontSize;
        txt.color = new Color(0.25f, 0.15f, 0.2f);
        txt.alignment = align;
        if (bold) txt.fontWeight = FontWeight.Bold;

        return txt;
    }

    private void CrearBotonIdioma(string nombre, Transform parent, Vector2 pos, string txtLabel, string codigoLang)
    {
        GameObject btnObj = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.zero; rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(100f, 30f);

        Image img = btnObj.GetComponent<Image>();
        img.color = new Color(1f, 0.88f, 0.92f);
        btnObj.AddComponent<Outline>().effectColor = new Color(0.85f, 0.7f, 0.75f);

        GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI txt = txtObj.GetComponent<TextMeshProUGUI>();
        txt.text = txtLabel;
        txt.fontSize = 11;
        txt.color = new Color(0.25f, 0.15f, 0.2f);
        txt.alignment = TextAlignmentOptions.Center;

        RectTransform rTxt = txtObj.GetComponent<RectTransform>();
        rTxt.anchorMin = Vector2.zero; rTxt.anchorMax = Vector2.one; rTxt.sizeDelta = Vector2.zero;

        btnObj.GetComponent<Button>().onClick.AddListener(() => {
            if (GameSettingsManager.Instance != null)
            {
                GameSettingsManager.Instance.lenguaje = codigoLang;
                GameSettingsManager.Instance.GuardarConfiguraciones();
            }
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.TraducirInterfazActual();
            }
            // Actualizar textos del panel actual
            ActualizarTextosPanel();
        });
    }

    private void ActualizarTextosPanel()
    {
        if (panelConfig == null) return;
        
        // Actualizar textos generales
        Transform t = panelConfig.transform.Find("Titulo");
        if (t != null) t.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.ObtenerTexto("Opciones");

        Transform vm = panelConfig.transform.Find("VolMusicaLabel");
        if (vm != null) vm.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.ObtenerTexto("Volumen Música") + ":";

        Transform vs = panelConfig.transform.Find("VolSFXLabel");
        if (vs != null) vs.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.ObtenerTexto("Volumen SFX") + ":";

        Transform ml = panelConfig.transform.Find("MultiplayerLabel");
        if (ml != null) ml.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.ObtenerTexto("Multijugador:");

        Transform dl = panelConfig.transform.Find("DarkModeLabel");
        if (dl != null) dl.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.ObtenerTexto("Modo Oscuro:");

        ActualizarTextoBotonDarkMode();

        Transform lP1N = panelConfig.transform.Find("LabelP1Name");
        if (lP1N != null) lP1N.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.ObtenerTexto("Nombre P1:");

        Transform lP2N = panelConfig.transform.Find("LabelP2Name");
        if (lP2N != null) lP2N.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.ObtenerTexto("Nombre P2:");

        Transform lP1A = panelConfig.transform.Find("LabelP1Avatar");
        if (lP1A != null) lP1A.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.ObtenerTexto("Avatar P1:");

        Transform lP2A = panelConfig.transform.Find("LabelP2Avatar");
        if (lP2A != null) lP2A.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.ObtenerTexto("Avatar P2:");

        Transform bP1A = panelConfig.transform.Find("BtnP1Av");
        if (bP1A != null) bP1A.GetComponentInChildren<TextMeshProUGUI>().text = GameSettingsManager.Instance.GetAvatarName(tempAvatarP1);

        Transform bP2A = panelConfig.transform.Find("BtnP2Av");
        if (bP2A != null) bP2A.GetComponentInChildren<TextMeshProUGUI>().text = GameSettingsManager.Instance.GetAvatarName(tempAvatarP2);

        if (txtInstrucciones != null) txtInstrucciones.text = LocalizationManager.Instance.ObtenerTexto("InstruccionesContenido");

        Transform bSave = panelConfig.transform.Find("BtnSave");
        if (bSave != null) bSave.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.ObtenerTexto("Guardar y Cerrar");

        ActualizarTextoBotonMultiplayer();
    }

    private Slider CrearSlider(string nombre, Transform parent, Vector2 pos, Vector2 size, float valueVal)
    {
        GameObject sliderObj = new GameObject(nombre, typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(parent, false);
        RectTransform rt = sliderObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.zero; rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Slider slider = sliderObj.GetComponent<Slider>();

        // Fondo (Background)
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(sliderObj.transform, false);
        bgObj.GetComponent<Image>().color = new Color(0.85f, 0.75f, 0.78f);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.25f); bgRt.anchorMax = new Vector2(1f, 0.75f);
        bgRt.sizeDelta = Vector2.zero;

        // Area de llenado (Fill Area)
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform faRt = fillArea.GetComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0f, 0.25f); faRt.anchorMax = new Vector2(1f, 0.75f);
        faRt.sizeDelta = Vector2.zero;

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        fill.GetComponent<Image>().color = new Color(1.0f, 0.78f, 0.85f); // Rosa
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.zero;
        fillRt.sizeDelta = Vector2.zero;

        // Area del manejador (Handle Slide Area)
        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform haRt = handleArea.GetComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero; haRt.anchorMax = Vector2.one;
        haRt.sizeDelta = Vector2.zero;

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        handle.GetComponent<Image>().color = new Color(0.9f, 0.6f, 0.7f); // Rosa oscuro
        handle.AddComponent<Outline>().effectColor = Color.white;
        RectTransform hRt = handle.GetComponent<RectTransform>();
        hRt.sizeDelta = new Vector2(15f, 0f);

        slider.fillRect = fillRt;
        slider.handleRect = hRt;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = valueVal;

        return slider;
    }

    private TMP_InputField CrearInputField(string nombre, Transform parent, Vector2 pos, Vector2 size, string defaultText)
    {
        // Contenedor principal de Input Field
        GameObject inputObj = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        inputObj.transform.SetParent(parent, false);
        RectTransform rt = inputObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.zero; rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = inputObj.GetComponent<Image>();
        img.color = new Color(1f, 0.95f, 0.97f);
        inputObj.AddComponent<Outline>().effectColor = new Color(0.85f, 0.7f, 0.75f);

        // Area de visualización del texto (TextArea)
        GameObject textArea = new GameObject("TextArea", typeof(RectTransform));
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform taRt = textArea.GetComponent<RectTransform>();
        taRt.anchorMin = Vector2.zero; taRt.anchorMax = Vector2.one;
        taRt.sizeDelta = new Vector2(-10f, -6f); // 5px horizontal padding, 3px vertical padding

        // Texto visible
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI txt = textObj.GetComponent<TextMeshProUGUI>();
        txt.fontSize = 11;
        txt.color = new Color(0.25f, 0.15f, 0.2f);
        txt.alignment = TextAlignmentOptions.Left;

        RectTransform rTxt = textObj.GetComponent<RectTransform>();
        rTxt.anchorMin = Vector2.zero; rTxt.anchorMax = Vector2.one; rTxt.sizeDelta = Vector2.zero;

        // Configuración InputField
        TMP_InputField inputField = inputObj.GetComponent<TMP_InputField>();
        inputField.textViewport = taRt;
        inputField.textComponent = txt;
        inputField.text = defaultText;

        return inputField;
    }

    private void CrearSeccionRecords()
    {
        // Borde caja récords
        GameObject borderObj = new GameObject("RecordsFrame", typeof(RectTransform), typeof(Image));
        borderObj.transform.SetParent(panelConfig.transform, false);
        RectTransform rt = borderObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.zero; rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(30f, 65f);
        rt.sizeDelta = new Vector2(520f, 70f);
        
        Image img = borderObj.GetComponent<Image>();
        img.color = new Color(0.93f, 0.96f, 1f, 0.9f); // Blanco azulado suave
        borderObj.AddComponent<Outline>().effectColor = new Color(0.7f, 0.8f, 0.85f);

        // Texto de Récords
        GameObject textObj = new GameObject("TxtRecords", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(borderObj.transform, false);
        txtRecords = textObj.GetComponent<TextMeshProUGUI>();
        txtRecords.fontSize = 11;
        txtRecords.color = new Color(0.15f, 0.1f, 0.18f);
        txtRecords.alignment = TextAlignmentOptions.Left;

        RectTransform rTxt = textObj.GetComponent<RectTransform>();
        rTxt.anchorMin = Vector2.zero;
        rTxt.anchorMax = Vector2.one;
        rTxt.anchoredPosition = new Vector2(8f, 0f); // offset horizontal
        rTxt.sizeDelta = new Vector2(-120f, -10f); // Margen para el botón

        ActualizarTextosRécord();

        // Botón Borrar Récords
        GameObject btnReset = new GameObject("BtnResetRecords", typeof(RectTransform), typeof(Image), typeof(Button));
        btnReset.transform.SetParent(borderObj.transform, false);
        RectTransform rtReset = btnReset.GetComponent<RectTransform>();
        rtReset.anchorMin = new Vector2(1f, 0.5f);
        rtReset.anchorMax = new Vector2(1f, 0.5f);
        rtReset.pivot = new Vector2(1f, 0.5f);
        rtReset.anchoredPosition = new Vector2(-10f, 0f);
        rtReset.sizeDelta = new Vector2(90f, 30f);
        btnReset.GetComponent<Image>().color = new Color(1f, 0.82f, 0.82f); // Rojo pastel
        btnReset.AddComponent<Outline>().effectColor = new Color(0.85f, 0.7f, 0.7f);

        GameObject txtResetObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtResetObj.transform.SetParent(btnReset.transform, false);
        TextMeshProUGUI txtReset = txtResetObj.GetComponent<TextMeshProUGUI>();
        string lang = GameSettingsManager.Instance != null ? GameSettingsManager.Instance.lenguaje : "ES";
        txtReset.text = lang == "ES" ? "Borrar" : "Reset";
        txtReset.fontSize = 10;
        txtReset.color = new Color(0.25f, 0.15f, 0.2f);
        txtReset.alignment = TextAlignmentOptions.Center;
        RectTransform rtR = txtResetObj.GetComponent<RectTransform>();
        rtR.anchorMin = Vector2.zero; rtR.anchorMax = Vector2.one; rtR.sizeDelta = Vector2.zero;

        btnReset.GetComponent<Button>().onClick.AddListener(() => {
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.ReiniciarRecords();
            }
        });
    }

    public void ActualizarTextosRécord()
    {
        if (txtRecords == null) return;

        int gw = GameProgressManager.Instance != null ? GameProgressManager.Instance.ObtenerGamesWon() : 0;
        float bt = GameProgressManager.Instance != null ? GameProgressManager.Instance.ObtenerBestTime() : 0f;
        System.Collections.Generic.List<int> topScores = GameProgressManager.Instance != null ? GameProgressManager.Instance.ObtenerTopScores() : new System.Collections.Generic.List<int>();

        string lang = GameSettingsManager.Instance != null ? GameSettingsManager.Instance.lenguaje : "ES";

        // Construir la lista de las mejores puntuaciones (todas, no solo la más alta)
        string listaPuntajes;
        if (topScores.Count == 0)
        {
            listaPuntajes = lang == "ES" ? "(Sin partidas ganadas aún)" : "(No games won yet)";
        }
        else
        {
            var partes = new System.Collections.Generic.List<string>();
            int max = Mathf.Min(topScores.Count, 5); // Mostrar hasta 5 en pantalla, todas quedan guardadas
            for (int i = 0; i < max; i++)
            {
                partes.Add($"{i + 1}. {topScores[i]}");
            }
            listaPuntajes = string.Join("   ", partes);
        }

        if (lang == "ES")
        {
            txtRecords.text = $"<b>RÉCORDS LOCALES:</b>\n" +
                              $"Partidas Ganadas: {gw}  |  Mejor Tiempo: {bt:F1}s restantes\n" +
                              $"Top Puntuaciones: {listaPuntajes}";
        }
        else
        {
            txtRecords.text = $"<b>LOCAL RECORDS:</b>\n" +
                              $"Games Won: {gw}  |  Best Time: {bt:F1}s remaining\n" +
                              $"Top Scores: {listaPuntajes}";
        }
    }
}
