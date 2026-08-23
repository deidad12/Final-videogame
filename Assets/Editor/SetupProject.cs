using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.Animations;
using UnityEditor.Events;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class SetupProject : EditorWindow
{
    [MenuItem("Tools/Setup Complete Game")]
    public static void SetupCompleteGame()
    {
        Debug.Log("Iniciando la configuración completa y automática de Lollipop Valley...");

        // 1. Obtener y configurar los sprites de la hoja de sprites
        string spriteSheetPath = "Assets/Sprites/Player/personaje principal1.png";
        
        // Comprobar que existe la hoja de sprites del jugador
        if (!File.Exists(spriteSheetPath))
        {
            Debug.LogError($"No se encontró la hoja de sprites del jugador en la ruta: {spriteSheetPath}");
            return;
        }

        // Cargar todos los sprites generados por el meta de personaje principal
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath);
        Sprite spriteIdle = null;
        Sprite spriteWalk1 = null;
        Sprite spriteWalk2 = null;
        Sprite spriteJump = null;
        Sprite spriteFall = null;

        foreach (Object asset in allAssets)
        {
            if (asset is Sprite sprite)
            {
                string spriteName = sprite.name.ToLower();
                if (spriteName == "idle") spriteIdle = sprite;
                else if (spriteName == "walk1") spriteWalk1 = sprite;
                else if (spriteName == "walk2") spriteWalk2 = sprite;
                else if (spriteName == "jump") spriteJump = sprite;
                else if (spriteName == "fall") spriteFall = sprite;
            }
        }

        // 2. Crear clips de animación programáticamente
        string animFolder = "Assets/Animations";
        if (!Directory.Exists(animFolder))
        {
            Directory.CreateDirectory(animFolder);
        }

        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{animFolder}/Player_Idle.anim");
        if (idleClip == null)
        {
            idleClip = CreateSpriteClip("Player_Idle", new Sprite[] { spriteIdle }, 1f, true);
            AssetDatabase.CreateAsset(idleClip, $"{animFolder}/Player_Idle.anim");
        }

        AnimationClip walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{animFolder}/Player_Walk.anim");
        if (walkClip == null)
        {
            walkClip = CreateSpriteClip("Player_Walk", new Sprite[] { spriteWalk1, spriteWalk2 }, 8f, true);
            AssetDatabase.CreateAsset(walkClip, $"{animFolder}/Player_Walk.anim");
        }

        AnimationClip jumpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{animFolder}/Player_Jump.anim");
        if (jumpClip == null)
        {
            jumpClip = CreateSpriteClip("Player_Jump", new Sprite[] { spriteJump }, 1f, false);
            AssetDatabase.CreateAsset(jumpClip, $"{animFolder}/Player_Jump.anim");
        }

        AnimationClip fallClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{animFolder}/Player_Fall.anim");
        if (fallClip == null)
        {
            fallClip = CreateSpriteClip("Player_Fall", new Sprite[] { spriteFall }, 1f, false);
            AssetDatabase.CreateAsset(fallClip, $"{animFolder}/Player_Fall.anim");
        }

        // 3. Crear Animator Controller
        string controllerPath = $"{animFolder}/PlayerAnimator.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            
            // Añadir Parámetros
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("VerticalVelocity", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);

            // Obtener máquina de estados
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            // Estados
            AnimatorState stateIdle = stateMachine.AddState("Idle");
            stateIdle.motion = idleClip;

            AnimatorState stateWalk = stateMachine.AddState("Walk");
            stateWalk.motion = walkClip;

            AnimatorState stateJump = stateMachine.AddState("Jump");
            stateJump.motion = jumpClip;

            AnimatorState stateFall = stateMachine.AddState("Fall");
            stateFall.motion = fallClip;

            // Transiciones
            var tIdleWalk = stateIdle.AddTransition(stateWalk);
            tIdleWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            tIdleWalk.hasExitTime = false;
            tIdleWalk.duration = 0f;

            var tWalkIdle = stateWalk.AddTransition(stateIdle);
            tWalkIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            tWalkIdle.hasExitTime = false;
            tWalkIdle.duration = 0f;

            var tAnyJump = stateMachine.AddAnyStateTransition(stateJump);
            tAnyJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            tAnyJump.AddCondition(AnimatorConditionMode.Greater, 0.1f, "VerticalVelocity");
            tAnyJump.duration = 0.05f;

            var tAnyFall = stateMachine.AddAnyStateTransition(stateFall);
            tAnyFall.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            tAnyFall.AddCondition(AnimatorConditionMode.Less, -0.1f, "VerticalVelocity");
            tAnyFall.duration = 0.05f;

            var tJumpIdle = stateJump.AddTransition(stateIdle);
            tJumpIdle.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            tJumpIdle.hasExitTime = false;
            tJumpIdle.duration = 0f;

            var tFallIdle = stateFall.AddTransition(stateIdle);
            tFallIdle.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            tFallIdle.hasExitTime = false;
            tFallIdle.duration = 0f;
        }

        AssetDatabase.SaveAssets();

        // 4. Crear Prefabs para proyectiles y monedas
        string prefabFolder = "Assets/Prefabs";
        if (!Directory.Exists(prefabFolder))
        {
            Directory.CreateDirectory(prefabFolder);
        }

        // Crear Prefab CaramelCoin
        string coinPrefabPath = $"{prefabFolder}/CaramelCoin.prefab";
        GameObject loadedCoinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(coinPrefabPath);
        if (loadedCoinPrefab == null)
        {
            GameObject coinGo = new GameObject("CaramelCoin");
            coinGo.AddComponent<CircleCollider2D>().isTrigger = true;
            SpriteRenderer cSr = coinGo.AddComponent<SpriteRenderer>();
            cSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), new Vector2(0.5f, 0.5f));
            cSr.color = new Color(1f, 0.84f, 0f); // Dorado/Amarillo
            coinGo.AddComponent<CollectibleItem>();
            
            loadedCoinPrefab = PrefabUtility.SaveAsPrefabAsset(coinGo, coinPrefabPath);
            DestroyImmediate(coinGo);
        }

        // Crear Prefab SweetBullet (Proyectil)
        string bulletPrefabPath = $"{prefabFolder}/SweetBullet.prefab";
        GameObject loadedBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPrefabPath);
        if (loadedBulletPrefab == null)
        {
            GameObject bulletGo = new GameObject("SweetBullet");
            bulletGo.AddComponent<CircleCollider2D>().isTrigger = true;
            SpriteRenderer bSr = bulletGo.AddComponent<SpriteRenderer>();
            bSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), new Vector2(0.5f, 0.5f));
            bSr.color = Color.magenta;
            bulletGo.AddComponent<Projectile>();
            
            loadedBulletPrefab = PrefabUtility.SaveAsPrefabAsset(bulletGo, bulletPrefabPath);
            DestroyImmediate(bulletGo);
        }

        // 5. Crear e Inicializar los 3 Niveles
        // Nivel 1: Valle de Algodón de Azúcar
        SetupBaseLevelScene("Level1", controller, spriteIdle, (player) => {
            // Añadir plataformas extra
            CreatePlatform("Plat_1", new Vector3(-8f, -2f, 0f), new Vector3(5f, 0.5f, 1f));
            CreatePlatform("Plat_2", new Vector3(0f, 0f, 0f), new Vector3(5f, 0.5f, 1f));
            CreatePlatform("Plat_3", new Vector3(8f, -2f, 0f), new Vector3(5f, 0.5f, 1f));

            // Enemigo Patrullero
            GameObject enemy = new GameObject("EnemyGoomba", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(EnemyPatrol));
            enemy.tag = "Enemy";
            enemy.transform.position = new Vector3(0f, 1f, 0f);
            var esc = enemy.GetComponent<EnemyPatrol>();
            
            GameObject wallDet = new GameObject("WallDetector");
            wallDet.transform.SetParent(enemy.transform);
            wallDet.transform.localPosition = new Vector3(0.6f, 0f, 0f);
            esc.detectorPared = wallDet.transform;
            esc.capaSuelo = LayerMask.GetMask("Default");

            SpriteRenderer eSr = enemy.GetComponent<SpriteRenderer>();
            if (eSr == null) eSr = enemy.AddComponent<SpriteRenderer>();
            eSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), new Vector2(0.5f, 0.5f));
            eSr.color = new Color(0.6f, 0.3f, 0.2f); // Chocolate

            // Pinchos
            CreateSpikes("Spikes_1", new Vector3(-4f, -3.8f, 0f), new Vector3(2f, 0.5f, 1f));
            CreateSpikes("Spikes_2", new Vector3(4f, -3.8f, 0f), new Vector3(2f, 0.5f, 1f));

            // Bloque interactivo
            GameObject block = new GameObject("QuestionBlock", typeof(BoxCollider2D), typeof(DestructibleBlock));
            block.tag = "Block";
            block.transform.position = new Vector3(0f, -1.8f, 0f);
            var db = block.GetComponent<DestructibleBlock>();
            db.prefabContenido = loadedCoinPrefab;
            SpriteRenderer bSr = block.AddComponent<SpriteRenderer>();
            bSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), new Vector2(0.5f, 0.5f));
            bSr.color = new Color(0.9f, 0.6f, 0.1f); // Naranja/Marrón

            // Coleccionables sueltos
            for(int i = 0; i < 3; i++)
            {
                GameObject coin = PrefabUtility.InstantiatePrefab(loadedCoinPrefab) as GameObject;
                coin.transform.position = new Vector3(-8f + i * 2f, 0f, 0f);
            }
        });

        // Nivel 2: Las Nubes de Fresa
        SetupBaseLevelScene("Level2", controller, spriteIdle, (player) => {
            // Añadir plataformas elevadas
            CreatePlatform("Plat_1", new Vector3(-10f, -1f, 0f), new Vector3(6f, 0.5f, 1f));
            CreatePlatform("Plat_2", new Vector3(-2f, 1.5f, 0f), new Vector3(6f, 0.5f, 1f));
            CreatePlatform("Plat_3", new Vector3(6f, -1f, 0f), new Vector3(6f, 0.5f, 1f));
            CreatePlatform("Plat_4", new Vector3(14f, 1.5f, 0f), new Vector3(6f, 0.5f, 1f));

            // Enemigo Volador
            GameObject flyer = new GameObject("EnemyFlyer", typeof(BoxCollider2D), typeof(EnemyFlyer));
            flyer.tag = "Enemy";
            flyer.transform.position = new Vector3(-2f, 3.5f, 0f);
            var eFly = flyer.GetComponent<EnemyFlyer>();
            eFly.velocidad = 2f;
            eFly.amplitud = 1.5f;
            eFly.vertical = true;
            SpriteRenderer fSr = flyer.AddComponent<SpriteRenderer>();
            fSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), new Vector2(0.5f, 0.5f));
            fSr.color = new Color(0.6f, 0.2f, 0.6f); // Púrpura

            // Torreta Disparadora
            GameObject shooter = new GameObject("TurretShooter", typeof(BoxCollider2D), typeof(ObstacleShooter));
            shooter.transform.position = new Vector3(6f, -0.3f, 0f);
            var oShoot = shooter.GetComponent<ObstacleShooter>();
            oShoot.prefabProyectil = loadedBulletPrefab;
            oShoot.direccionDisparo = Vector2.left;
            oShoot.velocidadProyectil = 4f;
            oShoot.intervaloDisparo = 2.5f;
            SpriteRenderer tSr = shooter.AddComponent<SpriteRenderer>();
            tSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), new Vector2(0.5f, 0.5f));
            tSr.color = Color.red;

            // Thwomp (Aplastador)
            GameObject thwomp = new GameObject("Thwomp", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(ObstacleThwomp));
            thwomp.tag = "Enemy";
            thwomp.transform.position = new Vector3(14f, 5f, 0f);
            thwomp.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
            var oThwomp = thwomp.GetComponent<ObstacleThwomp>();
            oThwomp.velocidadCaida = 14f;
            oThwomp.velocidadRetorno = 2f;
            oThwomp.capaJugador = LayerMask.GetMask("Default");
            SpriteRenderer thSr = thwomp.AddComponent<SpriteRenderer>();
            thSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), new Vector2(0.5f, 0.5f));
            thSr.color = new Color(0.3f, 0.2f, 0.2f); // Chocolate oscuro

            // Pinchos
            CreateSpikes("Spikes_1", new Vector3(2f, -3.8f, 0f), new Vector3(4f, 0.5f, 1f));

            // Coleccionables sueltos
            for(int i = 0; i < 4; i++)
            {
                GameObject coin = PrefabUtility.InstantiatePrefab(loadedCoinPrefab) as GameObject;
                coin.transform.position = new Vector3(-10f + i * 8f, 3f, 0f);
            }
        });

        // Nivel 3: El Castillo de Fresa (Boss)
        SetupBaseLevelScene("Level3", controller, spriteIdle, (player) => {
            // Estructura de arena de batalla
            CreatePlatform("Plat_Left", new Vector3(-8f, -1.5f, 0f), new Vector3(6f, 0.5f, 1f));
            CreatePlatform("Plat_Right", new Vector3(8f, -1.5f, 0f), new Vector3(6f, 0.5f, 1f));
            CreatePlatform("Plat_Center", new Vector3(0f, 1f, 0f), new Vector3(8f, 0.5f, 1f));

            // Jefe Final
            GameObject boss = new GameObject("BossDrCocoa", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(BossController));
            boss.tag = "Enemy";
            boss.transform.position = new Vector3(0f, 3f, 0f);
            boss.transform.localScale = new Vector3(2f, 2f, 1f); // Jefe grande
            
            var bCtrl = boss.GetComponent<BossController>();
            bCtrl.vidaMaxima = 3;
            bCtrl.velocidad = 3.5f;
            bCtrl.fuerzaSalto = 9f;
            bCtrl.intervaloSalto = 3f;

            // Crear límites de patrulla para el Boss
            GameObject limitL = new GameObject("BossLimitL");
            limitL.transform.position = new Vector3(-12f, -3f, 0f);
            GameObject limitR = new GameObject("BossLimitR");
            limitR.transform.position = new Vector3(12f, -3f, 0f);
            
            bCtrl.limiteIzquierdo = limitL.transform;
            bCtrl.limiteDerecho = limitR.transform;

            SpriteRenderer bSr = boss.GetComponent<SpriteRenderer>();
            if (bSr == null) bSr = boss.AddComponent<SpriteRenderer>();
            bSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), new Vector2(0.5f, 0.5f));
            bSr.color = new Color(0.5f, 0.1f, 0.15f); // Frambuesa oscuro

            // Pinchos en los laterales para aumentar dificultad
            CreateSpikes("Spikes_L", new Vector3(-14f, -3.8f, 0f), new Vector3(3f, 0.5f, 1f));
            CreateSpikes("Spikes_R", new Vector3(14f, -3.8f, 0f), new Vector3(3f, 0.5f, 1f));
        });

        // 6. Crear y Configurar la Escena de Menú Principal (MainMenu)
        string menuScenePath = "Assets/Scenes/MainMenu.unity";
        Scene mainMenuScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Canvas de Menú
        GameObject menuCanvasGO = new GameObject("MenuCanvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        Canvas menuCanvas = menuCanvasGO.GetComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Dynamic AestheticManager in menu
        GameObject menuAesthetic = new GameObject("AestheticManager");
        menuAesthetic.AddComponent<AestheticManager>();

        // AudioManager
        GameObject menuAudioManagerGO = new GameObject("_AudioManager");
        menuAudioManagerGO.AddComponent<AudioManager>();

        // Panel Opciones (desactivado por defecto)
        GameObject panelOpciones = new GameObject("PanelOpciones", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        panelOpciones.transform.SetParent(menuCanvasGO.transform, false);
        RectTransform opRt = panelOpciones.GetComponent<RectTransform>();
        opRt.anchorMin = new Vector2(0.15f, 0.15f);
        opRt.anchorMax = new Vector2(0.85f, 0.85f);
        opRt.offsetMin = Vector2.zero;
        opRt.offsetMax = Vector2.zero;
        panelOpciones.GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 0.92f, 0.95f, 0.95f);

        // Texto Controles
        GameObject controlsText = new GameObject("ControlsText", typeof(RectTransform), typeof(TextMeshProUGUI));
        controlsText.transform.SetParent(panelOpciones.transform, false);
        TextMeshProUGUI ctrlTmp = controlsText.GetComponent<TextMeshProUGUI>();
        ctrlTmp.text = "CONTROLES DE LOLLIPOP VALLEY\n\n\n* A / D / Flechas: Mover Izquierda / Derecha\n\n* Espacio: Saltar (pisa la cabeza de tus enemigos)\n\n* Bloques: Golpea los bloques naranjas desde abajo";
        ctrlTmp.fontSize = 20;
        ctrlTmp.alignment = TextAlignmentOptions.Center;
        ctrlTmp.color = new Color(0.25f, 0.15f, 0.2f);
        RectTransform ctrlRt = controlsText.GetComponent<RectTransform>();
        ctrlRt.anchoredPosition = new Vector2(0f, 40f);
        ctrlRt.sizeDelta = new Vector2(550f, 250f);

        // MainMenu Script Manager
        GameObject mainMenuManagerGO = new GameObject("_MainMenuManager");
        MainMenu menuComp = mainMenuManagerGO.AddComponent<MainMenu>();
        menuComp.nombreEscenaJuego = "Level1";
        menuComp.panelOpciones = panelOpciones;

        // Botón Volver de Opciones
        GameObject btnVolver = CreateUIButton(panelOpciones, "BtnVolver", "Volver", new Vector2(0f, -120f));
        AddButtonListener(btnVolver.GetComponent<UnityEngine.UI.Button>(), menuComp, "CerrarOpciones");

        panelOpciones.SetActive(false);

        // Título del Juego en Menú
        GameObject titleGO = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(menuCanvasGO.transform, false);
        TextMeshProUGUI titleTmp = titleGO.GetComponent<TextMeshProUGUI>();
        titleTmp.text = "PASTEL DREAM WORLD";
        titleTmp.fontSize = 42;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = new Color(0.6f, 0.15f, 0.25f);
        RectTransform titleRt = titleGO.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0f, 130f);
        titleRt.sizeDelta = new Vector2(500f, 80f);

        // Botones del Menú Principal
        GameObject btnJugar = CreateUIButton(menuCanvasGO, "BtnJugar", "Jugar", new Vector2(0f, 25f));
        AddButtonListener(btnJugar.GetComponent<UnityEngine.UI.Button>(), menuComp, "Jugar");

        GameObject btnOpciones = CreateUIButton(menuCanvasGO, "BtnOpciones", "Opciones", new Vector2(0f, -35f));
        AddButtonListener(btnOpciones.GetComponent<UnityEngine.UI.Button>(), menuComp, "AbrirOpciones");

        GameObject btnSalir = CreateUIButton(menuCanvasGO, "BtnSalir", "Salir", new Vector2(0f, -95f));
        AddButtonListener(btnSalir.GetComponent<UnityEngine.UI.Button>(), menuComp, "Salir");

        // Guardar escena del menú
        EditorSceneManager.MarkSceneDirty(mainMenuScene);
        EditorSceneManager.SaveScene(mainMenuScene, menuScenePath);

        // 7. Configurar Build Settings con los 4 niveles
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
        buildScenes.Add(new EditorBuildSettingsScene(menuScenePath, true));
        buildScenes.Add(new EditorBuildSettingsScene("Assets/Scenes/Level1.unity", true));
        buildScenes.Add(new EditorBuildSettingsScene("Assets/Scenes/Level2.unity", true));
        buildScenes.Add(new EditorBuildSettingsScene("Assets/Scenes/Level3.unity", true));
        EditorBuildSettings.scenes = buildScenes.ToArray();

        // Recargar la escena de gameplay por conveniencia
        EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");

        Debug.Log("¡CONFIGURACIÓN COMPLETADA CON ÉXITO! Las 4 escenas (Menú y Niveles 1, 2 y 3) han sido configuradas, vinculadas a sus scripts, tags e integradas en Build Settings.");
        EditorUtility.DisplayDialog("Juego Configurado", "El videojuego de 3 niveles con menú se ha configurado de forma automática.\n\n¡Presiona Play en Unity para probar Lollipop Valley!", "Aceptar");
    }

    private static void SetupBaseLevelScene(string sceneName, AnimatorController animController, Sprite spriteIdle, System.Action<GameObject> customizeScene)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Add Tags
        AddTag("Ground");
        AddTag("Platform");
        AddTag("Enemy");
        AddTag("Block");

        // Dynamic AestheticManager in Level
        GameObject aestheticGO = new GameObject("AestheticManager");
        aestheticGO.AddComponent<AestheticManager>();

        // Create player
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.AddComponent<Rigidbody2D>();
        player.AddComponent<CapsuleCollider2D>();
        player.AddComponent<PlayerMovement>();

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        if (spriteIdle != null) sr.sprite = spriteIdle;
        else
        {
            sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), new Vector2(0.5f, 0.5f));
            sr.color = Color.magenta;
        }

        if (animController != null)
        {
            Animator animComponent = player.AddComponent<Animator>();
            animComponent.runtimeAnimatorController = animController;
        }

        // SpawnPoint
        GameObject spawnPointGO = new GameObject("SpawnPoint");
        spawnPointGO.transform.position = new Vector3(-15f, -3f, 0f);
        player.transform.position = spawnPointGO.transform.position;

        // Ground
        GameObject groundGO = new GameObject("Ground");
        groundGO.tag = "Ground";
        groundGO.transform.position = new Vector3(0f, -5f, 0f);
        groundGO.transform.localScale = new Vector3(80f, 2f, 1f);
        groundGO.AddComponent<BoxCollider2D>();
        SpriteRenderer gSr = groundGO.AddComponent<SpriteRenderer>();
        gSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), new Vector2(0.5f, 0.5f));
        gSr.color = new Color(0.2f, 0.2f, 0.2f);

        // FallZone
        GameObject fallZoneGO = new GameObject("FallZone");
        fallZoneGO.transform.position = new Vector3(0f, -10f, 0f);
        fallZoneGO.transform.localScale = new Vector3(120f, 2f, 1f);
        BoxCollider2D fbc = fallZoneGO.AddComponent<BoxCollider2D>();
        fbc.isTrigger = true;
        fallZoneGO.AddComponent<FallZone>();

        // VictoryZone (Portal de meta)
        GameObject victoryZoneGO = new GameObject("VictoryZone");
        victoryZoneGO.transform.position = new Vector3(30f, -3f, 0f);
        victoryZoneGO.transform.localScale = new Vector3(2f, 2f, 1f);
        BoxCollider2D vbc = victoryZoneGO.AddComponent<BoxCollider2D>();
        vbc.isTrigger = true;
        victoryZoneGO.AddComponent<VictoryZone>();
        SpriteRenderer vSr = victoryZoneGO.AddComponent<SpriteRenderer>();
        vSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), new Vector2(0.5f, 0.5f));
        vSr.color = Color.yellow;

        // Camera follow
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
            if (camFollow == null) camFollow = mainCam.gameObject.AddComponent<CameraFollow>();
            camFollow.objetivo = player.transform;
            camFollow.suavidad = 5f;
            camFollow.offset = new Vector3(0f, 1.5f, -10f);
        }

        // Canvas
        GameObject canvasGO = new GameObject("HUD_Canvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Timer Text
        GameObject timerTextGO = new GameObject("TimerText", typeof(RectTransform), typeof(TextMeshProUGUI));
        timerTextGO.transform.SetParent(canvasGO.transform, false);
        RectTransform tRt = timerTextGO.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 1f);
        tRt.anchorMax = new Vector2(0f, 1f);
        tRt.pivot = new Vector2(0f, 1f);
        tRt.anchoredPosition = new Vector2(20f, -20f);
        tRt.sizeDelta = new Vector2(250f, 40f);
        TextMeshProUGUI timerTmp = timerTextGO.GetComponent<TextMeshProUGUI>();
        timerTmp.text = "Tiempo: 60s";
        timerTmp.fontSize = 24;
        timerTmp.color = Color.white;

        // Lives Text
        GameObject livesTextGO = new GameObject("LivesText", typeof(RectTransform), typeof(TextMeshProUGUI));
        livesTextGO.transform.SetParent(canvasGO.transform, false);
        RectTransform lRt = livesTextGO.GetComponent<RectTransform>();
        lRt.anchorMin = new Vector2(0f, 1f);
        lRt.anchorMax = new Vector2(0f, 1f);
        lRt.pivot = new Vector2(0f, 1f);
        lRt.anchoredPosition = new Vector2(20f, -60f);
        lRt.sizeDelta = new Vector2(250f, 40f);
        TextMeshProUGUI livesTmp = livesTextGO.GetComponent<TextMeshProUGUI>();
        livesTmp.text = "Vidas: 3";
        livesTmp.fontSize = 24;
        livesTmp.color = Color.white;

        // Objects/Collectible Counter Text
        GameObject objectsTextGO = new GameObject("ObjectsText", typeof(RectTransform), typeof(TextMeshProUGUI));
        objectsTextGO.transform.SetParent(canvasGO.transform, false);
        RectTransform oRt = objectsTextGO.GetComponent<RectTransform>();
        oRt.anchorMin = new Vector2(0f, 1f);
        oRt.anchorMax = new Vector2(0f, 1f);
        oRt.pivot = new Vector2(0f, 1f);
        oRt.anchoredPosition = new Vector2(20f, -100f);
        oRt.sizeDelta = new Vector2(250f, 40f);
        TextMeshProUGUI objectsTmp = objectsTextGO.GetComponent<TextMeshProUGUI>();
        objectsTmp.text = "Objetos: 0";
        objectsTmp.fontSize = 24;
        objectsTmp.color = Color.white;

        // Student Text
        GameObject studentTextGO = new GameObject("StudentText", typeof(RectTransform), typeof(TextMeshProUGUI));
        studentTextGO.transform.SetParent(canvasGO.transform, false);
        RectTransform sRt = studentTextGO.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(1f, 1f);
        sRt.anchorMax = new Vector2(1f, 1f);
        sRt.pivot = new Vector2(1f, 1f);
        sRt.anchoredPosition = new Vector2(-20f, -20f);
        sRt.sizeDelta = new Vector2(350f, 80f);
        TextMeshProUGUI studentTmp = studentTextGO.GetComponent<TextMeshProUGUI>();
        studentTmp.text = "Estudiante: Diseño Pastel\nMatrícula: Plataformas 2D";
        studentTmp.fontSize = 20;
        studentTmp.alignment = TextAlignmentOptions.TopRight;
        studentTmp.color = Color.white;

        // GameManager
        GameObject gameManagerGO = new GameObject("_GameManager");
        GameManager gm = gameManagerGO.AddComponent<GameManager>();

        // Panel Victoria
        GameObject panelVictoria = new GameObject("PanelVictoria", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        panelVictoria.transform.SetParent(canvasGO.transform, false);
        RectTransform pVRt = panelVictoria.GetComponent<RectTransform>();
        pVRt.anchorMin = Vector2.zero;
        pVRt.anchorMax = Vector2.one;
        pVRt.offsetMin = Vector2.zero;
        pVRt.offsetMax = Vector2.zero;
        panelVictoria.GetComponent<UnityEngine.UI.Image>().color = new Color(0.92f, 1.0f, 0.92f, 0.9f); // Verde pastel
        
        GameObject vicText = new GameObject("VicText", typeof(RectTransform), typeof(TextMeshProUGUI));
        vicText.transform.SetParent(panelVictoria.transform, false);
        TextMeshProUGUI vicTmp = vicText.GetComponent<TextMeshProUGUI>();
        vicTmp.text = "¡VICTORIA!";
        vicTmp.fontSize = 44;
        vicTmp.alignment = TextAlignmentOptions.Center;
        vicTmp.color = new Color(0.1f, 0.5f, 0.2f);
        RectTransform vTxtRt = vicText.GetComponent<RectTransform>();
        vTxtRt.anchoredPosition = new Vector2(0f, 60f);
        vTxtRt.sizeDelta = new Vector2(400f, 100f);

        GameObject btnReVic = CreateUIButton(panelVictoria, "BtnReiniciarVictoria", "Reiniciar Juego", new Vector2(0f, -50f));
        AddButtonListener(btnReVic.GetComponent<UnityEngine.UI.Button>(), gm, "ReiniciarJuego");
        GameObject btnMenuVic = CreateUIButton(panelVictoria, "BtnMenuVictoria", "Volver al Menú", new Vector2(0f, -110f));
        AddButtonListener(btnMenuVic.GetComponent<UnityEngine.UI.Button>(), gm, "IrAlMenuPrincipal");

        // Panel Derrota
        GameObject panelDerrota = new GameObject("PanelDerrota", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        panelDerrota.transform.SetParent(canvasGO.transform, false);
        RectTransform pDRt = panelDerrota.GetComponent<RectTransform>();
        pDRt.anchorMin = Vector2.zero;
        pDRt.anchorMax = Vector2.one;
        pDRt.offsetMin = Vector2.zero;
        pDRt.offsetMax = Vector2.zero;
        panelDerrota.GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.92f, 0.92f, 0.9f); // Rojo pastel

        GameObject defText = new GameObject("DefText", typeof(RectTransform), typeof(TextMeshProUGUI));
        defText.transform.SetParent(panelDerrota.transform, false);
        TextMeshProUGUI defTmp = defText.GetComponent<TextMeshProUGUI>();
        defTmp.text = "FIN DEL JUEGO";
        defTmp.fontSize = 44;
        defTmp.alignment = TextAlignmentOptions.Center;
        defTmp.color = new Color(0.6f, 0.15f, 0.25f);
        RectTransform dTxtRt = defText.GetComponent<RectTransform>();
        dTxtRt.anchoredPosition = new Vector2(0f, 80f);
        dTxtRt.sizeDelta = new Vector2(400f, 80f);

        GameObject defMsg = new GameObject("DefMsgText", typeof(RectTransform), typeof(TextMeshProUGUI));
        defMsg.transform.SetParent(panelDerrota.transform, false);
        TextMeshProUGUI defMsgTmp = defMsg.GetComponent<TextMeshProUGUI>();
        defMsgTmp.text = "¡Has muerto!";
        defMsgTmp.fontSize = 20;
        defMsgTmp.alignment = TextAlignmentOptions.Center;
        defMsgTmp.color = new Color(0.3f, 0.1f, 0.15f);
        RectTransform dMsgRt = defMsg.GetComponent<RectTransform>();
        dMsgRt.anchoredPosition = new Vector2(0f, 20f);
        dMsgRt.sizeDelta = new Vector2(400f, 60f);

        GameObject btnReDef = CreateUIButton(panelDerrota, "BtnReiniciarDerrota", "Reintentar", new Vector2(0f, -50f));
        AddButtonListener(btnReDef.GetComponent<UnityEngine.UI.Button>(), gm, "ReiniciarJuego");
        GameObject btnMenuDef = CreateUIButton(panelDerrota, "BtnMenuDerrota", "Volver al Menú", new Vector2(0f, -110f));
        AddButtonListener(btnMenuDef.GetComponent<UnityEngine.UI.Button>(), gm, "IrAlMenuPrincipal");

        // Configuración de GameManager
        gm.jugador = player;
        gm.spawnPoint = spawnPointGO.transform;
        gm.textoTemporizador = timerTmp;
        gm.textoVidas = livesTmp;
        gm.textoObjetos = objectsTmp;
        gm.textoDerrotaMensaje = defMsgTmp;
        gm.textoAlumno = studentTmp;
        gm.panelVictoria = panelVictoria;
        gm.panelDerrota = panelDerrota;
        gm.escenaNivel1 = "Level1";
        gm.escenaNivel2 = "Level2";
        gm.escenaNivel3 = "Level3";
        gm.escenaMenu = "MainMenu";

        // Instancia AudioManager en nivel
        GameObject audioManagerGO = new GameObject("_AudioManager");
        audioManagerGO.AddComponent<AudioManager>();

        // Customizar la escena concreta
        customizeScene?.Invoke(player);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, $"Assets/Scenes/{sceneName}.unity");
    }

    private static AnimationClip CreateSpriteClip(string name, Sprite[] sprites, float frameRate, bool loop)
    {
        AnimationClip clip = new AnimationClip();
        clip.name = name;
        clip.frameRate = frameRate;

        if (loop)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        EditorCurveBinding binding = new EditorCurveBinding();
        binding.type = typeof(SpriteRenderer);
        binding.path = "";
        binding.propertyName = "m_Sprite";

        if (sprites != null && sprites.Length > 0 && sprites[0] != null)
        {
            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe();
                keyframes[i].time = i / frameRate;
                keyframes[i].value = sprites[i];
            }
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        }

        return clip;
    }

    private static GameObject CreateUIButton(GameObject parent, string name, string textStr, Vector2 localPos)
    {
        GameObject btnGO = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
        btnGO.transform.SetParent(parent.transform, false);
        
        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchoredPosition = localPos;
        rt.sizeDelta = new Vector2(180f, 40f);

        btnGO.GetComponent<UnityEngine.UI.Image>().color = new Color(0.98f, 0.88f, 0.92f); // Botón pastel

        GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(btnGO.transform, false);
        
        TextMeshProUGUI tmpText = textGO.GetComponent<TextMeshProUGUI>();
        tmpText.text = textStr;
        tmpText.fontSize = 18;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.color = new Color(0.25f, 0.15f, 0.2f);
        tmpText.alignment = TextAlignmentOptions.Center;

        RectTransform tRt = textGO.GetComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.offsetMin = Vector2.zero;
        tRt.offsetMax = Vector2.zero;

        return btnGO;
    }

    private static void AddButtonListener(UnityEngine.UI.Button button, MonoBehaviour target, string methodName)
    {
        System.Reflection.MethodInfo method = target.GetType().GetMethod(methodName);
        if (method != null)
        {
            UnityEngine.Events.UnityAction action = (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), target, method);
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(button.onClick, action);
        }
        else
        {
            Debug.LogError($"Method {methodName} not found on {target.GetType().Name}");
        }
    }

    private static void CreatePlatform(string name, Vector3 position, Vector3 scale)
    {
        GameObject plat = new GameObject(name);
        plat.tag = "Platform";
        plat.transform.position = position;
        plat.transform.localScale = scale;
        plat.AddComponent<BoxCollider2D>();

        SpriteRenderer pSr = plat.AddComponent<SpriteRenderer>();
        pSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
        pSr.color = new Color(0.96f, 0.8f, 0.98f); // Lila pastel
    }

    private static void CreateSpikes(string name, Vector3 position, Vector3 scale)
    {
        GameObject spikes = new GameObject(name);
        spikes.transform.position = position;
        spikes.transform.localScale = scale;
        
        var bc = spikes.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        spikes.AddComponent<ObstacleSpikes>();

        SpriteRenderer sSr = spikes.AddComponent<SpriteRenderer>();
        sSr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), new Vector2(0.5f, 0.5f));
        sSr.color = new Color(0.85f, 0.4f, 0.5f); // Rosa fuerte/Rojizo
    }

    private static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        bool exists = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tag))
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
            newTag.stringValue = tag;
            tagManager.ApplyModifiedProperties();
        }
    }
}
