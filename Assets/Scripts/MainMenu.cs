using UnityEngine;
        using UnityEngine.SceneManagement;
        using UnityEngine.UI;
        using TMPro;
        
        public class MainMenu : MonoBehaviour
        {
            [Header("Configuración de Escena")]
            [Tooltip("Nombre de la escena de juego que se cargará al pulsar jugar.")]
            public string nombreEscenaJuego = "Level1";
        
            [Header("Paneles de Interfaz")]
            [Tooltip("El panel que muestra las opciones/controles del juego.")]
            public GameObject panelOpciones;
        
            private static readonly Color colorBoton = new Color(0.98f, 0.88f, 0.92f);
            private static readonly Color colorTextoBoton = new Color(0.25f, 0.15f, 0.2f);
            private static readonly Color colorContorno = new Color(0.85f, 0.7f, 0.75f);
        
            void Start()
            {
                if (panelOpciones != null)
                {
                    panelOpciones.SetActive(false);
                }
        
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayBGM();
                }
        
                ConfigurarBotonesMenu();
                CrearBotonesSelectorNiveles();
            }
        
            private void ConfigurarBotonesMenu()
            {
                Button[] botones = FindObjectsOfType<Button>(true);
                foreach (Button btn in botones)
                {
                    string nombre = btn.gameObject.name.ToLower();
        
                    if (nombre.Contains("jugar") || nombre.Contains("play") || nombre.Contains("start") || nombre.Contains("inicio"))
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(Jugar);
                    }
                    else if (nombre.Contains("salir") || nombre.Contains("quit") || nombre.Contains("exit"))
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(Salir);
                    }
                    else if (nombre.Contains("opciones") || nombre.Contains("config") || nombre.Contains("settings"))
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(AbrirOpciones);
                    }
                    else if (nombre.Contains("volver") || nombre.Contains("back") || nombre.Contains("cerrar") || nombre.Contains("atras") || nombre.Contains("regresar") || nombre.Contains("close") || nombre.Contains("home"))
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(CerrarOpciones);
                    }
                    else if (nombre.Contains("nivel1") || nombre.Contains("level1"))
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => IrANivel(1));
                    }
                    else if (nombre.Contains("nivel2") || nombre.Contains("level2"))
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => IrANivel(2));
                    }
                    else if (nombre.Contains("nivel3") || nombre.Contains("level3"))
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => IrANivel(3));
                    }
                }
            }
        
            // --- SELECTOR DE NIVELES (creado por código, no requiere setup manual) ---
            private void CrearBotonesSelectorNiveles()
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas == null) return;
        
                if (canvas.transform.Find("SelectorNivelesPanel") != null) return; // Evitar duplicados
        
                GameObject contenedor = new GameObject("SelectorNivelesPanel", typeof(RectTransform));
                contenedor.transform.SetParent(canvas.transform, false);
                RectTransform rtCont = contenedor.GetComponent<RectTransform>();
                rtCont.anchorMin = new Vector2(0.5f, 0.5f);
                rtCont.anchorMax = new Vector2(0.5f, 0.5f);
                rtCont.pivot = new Vector2(0.5f, 0.5f);
                rtCont.anchoredPosition = new Vector2(0f, -190f);
                rtCont.sizeDelta = new Vector2(420f, 70f);
        
                // Título
                GameObject tituloObj = new GameObject("TxtSelectorTitulo", typeof(RectTransform), typeof(TextMeshProUGUI));
                tituloObj.transform.SetParent(contenedor.transform, false);
                RectTransform rtTit = tituloObj.GetComponent<RectTransform>();
                rtTit.anchorMin = new Vector2(0.5f, 1f);
                rtTit.anchorMax = new Vector2(0.5f, 1f);
                rtTit.pivot = new Vector2(0.5f, 1f);
                rtTit.anchoredPosition = new Vector2(0f, 0f);
                rtTit.sizeDelta = new Vector2(400f, 25f);
                TextMeshProUGUI tituloTxt = tituloObj.GetComponent<TextMeshProUGUI>();
                tituloTxt.text = LocalizationManager.Instance != null ? (GameSettingsManager.Instance != null && GameSettingsManager.Instance.lenguaje == "EN" ? "Select Level" : "Seleccionar Nivel") : "Seleccionar Nivel";
                tituloTxt.fontSize = 14;
                tituloTxt.alignment = TextAlignmentOptions.Center;
                tituloTxt.color = colorTextoBoton;
                tituloTxt.fontWeight = FontWeight.Bold;
        
                float[] xPositions = new float[] { -140f, 0f, 140f };
                for (int i = 0; i < 3; i++)
                {
                    int numeroNivel = i + 1;
                    CrearBotonNivel("BtnNivel" + numeroNivel, contenedor.transform, new Vector2(xPositions[i], -20f), numeroNivel);
                }
            }
        
            private void CrearBotonNivel(string nombre, Transform parent, Vector2 pos, int numeroNivel)
            {
                GameObject btnObj = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(parent, false);
                RectTransform rt = btnObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = pos;
                rt.sizeDelta = new Vector2(110f, 40f);
        
                Image img = btnObj.GetComponent<Image>();
                img.color = colorBoton;
                Outline outline = btnObj.AddComponent<Outline>();
                outline.effectColor = colorContorno;
                outline.effectDistance = new Vector2(1.5f, -1.5f);
        
                GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtObj.transform.SetParent(btnObj.transform, false);
                TextMeshProUGUI txt = txtObj.GetComponent<TextMeshProUGUI>();
                txt.text = "Nivel " + numeroNivel;
                txt.fontSize = 15;
                txt.alignment = TextAlignmentOptions.Center;
                txt.color = colorTextoBoton;
                txt.fontWeight = FontWeight.Bold;
                RectTransform rTxt = txtObj.GetComponent<RectTransform>();
                rTxt.anchorMin = Vector2.zero;
                rTxt.anchorMax = Vector2.one;
                rTxt.sizeDelta = Vector2.zero;
        
                Button btn = btnObj.GetComponent<Button>();
                btn.onClick.AddListener(() => IrANivel(numeroNivel));
            }
        
            // Carga directamente el nivel indicado (Selector de Niveles)
            public void IrANivel(int numero)
            {
                SceneManager.LoadScene("Level" + numero);
            }
        
            public void Jugar()
            {
                string escenaCargar = nombreEscenaJuego;
                if (escenaCargar == "SampleScene" || string.IsNullOrEmpty(escenaCargar))
                {
                    escenaCargar = "Level1";
                }
                SceneManager.LoadScene(escenaCargar);
            }
        
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

    public void CerrarOpciones()
    {
        Debug.Log("BOTON VOLVER PRESIONADO");

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }
    }

    public void Salir()
        {
            Debug.Log("Saliendo del juego...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        }
        