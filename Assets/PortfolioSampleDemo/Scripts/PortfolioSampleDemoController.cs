using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yakchonara.PortfolioSample.Common;
using Yakchonara.PortfolioSample.Elemental;
using Yakchonara.PortfolioSample.Facility;
using Yakchonara.PortfolioSample.SceneFlow;
using Yakchonara.PortfolioSample.UI;

namespace Yakchonara.PortfolioSample.Demo.Unity;

public sealed class PortfolioSampleDemoController : MonoBehaviour
{
    private const string SceneName = "Bath_House";
    private const string BathHerbId = "Herb-001";
    private const string SaunaHerbId = "Herb-002";
    private const float PlayerMoveSpeed = 3.6f;
    private const float InteractionRange = 1.45f;

    [Header("Facility Renderers")]
    [SerializeField] private Renderer? bathRenderer;
    [SerializeField] private Renderer? saunaRenderer;

    [Header("Herb Markers")]
    [SerializeField] private Transform? bathHerbMarker;
    [SerializeField] private Transform? saunaHerbMarker;
    [SerializeField] private Transform? bathWaitingPoint;
    [SerializeField] private Transform? bathSlotPoint;
    [SerializeField] private Transform? saunaWaitingPoint;
    [SerializeField] private Transform? saunaSlotPoint;

    [Header("Elemental Markers")]
    [SerializeField] private Transform? bathElementalMarker;
    [SerializeField] private Transform? saunaElementalMarker;
    [SerializeField] private Transform? bathElementalWorldPoint;
    [SerializeField] private Transform? bathElementalDockPoint;
    [SerializeField] private Transform? saunaElementalWorldPoint;
    [SerializeField] private Transform? saunaElementalDockPoint;
    [SerializeField] private Transform? playerMarker;
    [SerializeField] private Transform? exitPoint;

    private readonly List<string> logLines = new();
    private readonly Vector3 cameraOffset = new(0f, 5.7f, -6.6f);
    private SceneFlowCoordinator sceneFlow = new();
    private AdditiveSceneContext bathContext = new(SceneName);
    private BathFacility bath = CreateBath();
    private SaunaFacility sauna = new(id: 201, name: "SaunaFacility-A", capacity: 1, lineupCapacity: 2);
    private FacilityStatusBinder bathBinder = new();
    private FacilityStatusBinder saunaBinder = new();
    private ElementalActionController actionController = new();
    private ElementalAgent bathElemental = new("Elemental-Fire-01", ElementalType.Fire, defaultScale: 1.0);
    private ElementalAgent saunaElemental = new("Elemental-Fire-03", ElementalType.Fire, defaultScale: 1.0);
    private GUIStyle? titleStyle;
    private GUIStyle? labelStyle;
    private GUIStyle? logStyle;
    private bool bathHerbQueued;
    private bool bathHerbAssigned;
    private bool saunaHerbQueued;
    private bool saunaHerbAssigned;

    public void Configure(
        Renderer bathRenderer,
        Renderer saunaRenderer,
        Transform bathHerbMarker,
        Transform saunaHerbMarker,
        Transform bathWaitingPoint,
        Transform bathSlotPoint,
        Transform saunaWaitingPoint,
        Transform saunaSlotPoint,
        Transform bathElementalMarker,
        Transform saunaElementalMarker,
        Transform bathElementalWorldPoint,
        Transform bathElementalDockPoint,
        Transform saunaElementalWorldPoint,
        Transform saunaElementalDockPoint,
        Transform playerMarker,
        Transform exitPoint)
    {
        this.bathRenderer = bathRenderer;
        this.saunaRenderer = saunaRenderer;
        this.bathHerbMarker = bathHerbMarker;
        this.saunaHerbMarker = saunaHerbMarker;
        this.bathWaitingPoint = bathWaitingPoint;
        this.bathSlotPoint = bathSlotPoint;
        this.saunaWaitingPoint = saunaWaitingPoint;
        this.saunaSlotPoint = saunaSlotPoint;
        this.bathElementalMarker = bathElementalMarker;
        this.saunaElementalMarker = saunaElementalMarker;
        this.bathElementalWorldPoint = bathElementalWorldPoint;
        this.bathElementalDockPoint = bathElementalDockPoint;
        this.saunaElementalWorldPoint = saunaElementalWorldPoint;
        this.saunaElementalDockPoint = saunaElementalDockPoint;
        this.playerMarker = playerMarker;
        this.exitPoint = exitPoint;
    }

    private static BathFacility CreateBath()
    {
        return new BathFacility(
            id: 101,
            name: "BathFacility-A",
            capacity: 2,
            lineupCapacity: 3,
            temperature: new BathTemperatureController(currentTemperature: 36, targetTemperature: 40, tolerance: 2));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapActiveDemoScene()
    {
        EnsureControllerForActiveDemoScene();
    }

    public static void EnsureControllerForActiveDemoScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != "PortfolioSampleDemo")
        {
            return;
        }

        PortfolioSampleDemoController[] controllers = FindObjectsByType<PortfolioSampleDemoController>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        if (controllers.Length > 0)
        {
            return;
        }

        new GameObject("Portfolio Sample Demo Controller").AddComponent<PortfolioSampleDemoController>();
    }

    private void Start()
    {
        EnsureSceneObjects();
        ResetDemo();
    }

    private void EnsureSceneObjects()
    {
        Transform root = FindOrCreateRoot("Generated Demo Visuals");
        if (Camera.main is null)
        {
            CreateRuntimeCamera();
        }

        if (GameObject.Find("Directional Light") is null)
        {
            CreateRuntimeLight();
        }

        if (GameObject.Find("Ground") is null)
        {
            CreateRuntimePrimitive("Ground", PrimitiveType.Cube, new Vector3(0f, -0.08f, 0f), new Vector3(8.5f, 0.1f, 5.4f), new Color(0.16f, 0.18f, 0.20f), root);
        }

        bathRenderer ??= FindRenderer("Bath Facility") ?? CreateRuntimePrimitive("Bath Facility", PrimitiveType.Cube, new Vector3(-2.1f, 0.15f, 0f), new Vector3(2.4f, 0.28f, 2.3f), new Color(0.18f, 0.55f, 0.86f), root).GetComponent<Renderer>();
        saunaRenderer ??= FindRenderer("Sauna Facility") ?? CreateRuntimePrimitive("Sauna Facility", PrimitiveType.Cube, new Vector3(2.1f, 0.15f, 0f), new Vector3(2.4f, 0.28f, 2.3f), new Color(0.86f, 0.43f, 0.18f), root).GetComponent<Renderer>();

        EnsureRuntimeLabel("Bath Label", "Bath Facility", new Vector3(-2.1f, 0.55f, -1.35f), root);
        EnsureRuntimeLabel("Sauna Label", "Sauna Facility", new Vector3(2.1f, 0.55f, -1.35f), root);
        EnsureRuntimeLabel("World Label", "Elementals", new Vector3(0f, 0.55f, 1.95f), root);
        EnsureRuntimeLabel("Exit Label", "Exit / Town", new Vector3(0f, 0.55f, 2.72f), root);

        bathWaitingPoint ??= FindOrCreateRuntimePoint("Bath Herb Waiting Point", new Vector3(-3.45f, 0.55f, 1.35f), root);
        bathSlotPoint ??= FindOrCreateRuntimePoint("Bath Herb Slot Point", new Vector3(-2.1f, 0.62f, 0f), root);
        saunaWaitingPoint ??= FindOrCreateRuntimePoint("Sauna Herb Waiting Point", new Vector3(3.45f, 0.55f, 1.35f), root);
        saunaSlotPoint ??= FindOrCreateRuntimePoint("Sauna Herb Slot Point", new Vector3(2.1f, 0.62f, 0f), root);
        bathElementalWorldPoint ??= FindOrCreateRuntimePoint("Bath Elemental World Point", new Vector3(-0.55f, 0.7f, 2f), root);
        bathElementalDockPoint ??= FindOrCreateRuntimePoint("Bath Elemental Dock Point", new Vector3(-3.15f, 0.72f, -0.85f), root);
        saunaElementalWorldPoint ??= FindOrCreateRuntimePoint("Sauna Elemental World Point", new Vector3(0.55f, 0.7f, 2f), root);
        saunaElementalDockPoint ??= FindOrCreateRuntimePoint("Sauna Elemental Dock Point", new Vector3(3.15f, 0.72f, -0.85f), root);
        exitPoint ??= FindOrCreateRuntimePoint("Exit Interaction Point", new Vector3(0f, 0.05f, 2.35f), root);

        bathHerbMarker ??= FindOrCreateRuntimePrimitive("Bath Herb Marker", PrimitiveType.Capsule, bathWaitingPoint.position, Vector3.one * 0.55f, new Color(0.45f, 0.86f, 0.48f), root);
        saunaHerbMarker ??= FindOrCreateRuntimePrimitive("Sauna Herb Marker", PrimitiveType.Capsule, saunaWaitingPoint.position, Vector3.one * 0.55f, new Color(0.60f, 0.90f, 0.50f), root);
        bathElementalMarker ??= FindOrCreateRuntimePrimitive("Bath Fire Elemental Marker", PrimitiveType.Sphere, bathElementalWorldPoint.position, Vector3.one * 0.7f, new Color(1f, 0.35f, 0.12f), root);
        saunaElementalMarker ??= FindOrCreateRuntimePrimitive("Sauna Fire Elemental Marker", PrimitiveType.Sphere, saunaElementalWorldPoint.position, Vector3.one * 0.7f, new Color(1f, 0.50f, 0.16f), root);
        EnsurePlayablePlayer(root);
    }

    private void Update()
    {
        HandlePlayerMovement();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetDemo();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            RunFullDemo();
        }
    }

    private static void CreateRuntimeCamera()
    {
        GameObject cameraObject = new("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 4.4f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.10f, 0.12f);
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 5.7f, -6.6f), Quaternion.Euler(55f, 0f, 0f));
    }

    private static void CreateRuntimeLight()
    {
        GameObject lightObject = new("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObject.transform.rotation = Quaternion.Euler(52f, -30f, 0f);
    }

    private void EnsurePlayablePlayer(Transform parent)
    {
        if (playerMarker is not null)
        {
            return;
        }

        GameObject? existing = GameObject.Find("Playable Herb Runner");
        if (existing is not null)
        {
            playerMarker = existing.transform;
            return;
        }

        GameObject player = CreateRuntimePrimitive(
            "Playable Herb Runner",
            PrimitiveType.Capsule,
            new Vector3(0f, 0.72f, 2.35f),
            new Vector3(0.65f, 0.9f, 0.65f),
            new Color(0.95f, 0.88f, 0.24f),
            parent);
        playerMarker = player.transform;
    }

    private static GameObject CreateRuntimePrimitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color, Transform parent)
    {
        GameObject primitive = GameObject.CreatePrimitive(type);
        primitive.name = name;
        primitive.transform.SetParent(parent);
        primitive.transform.position = position;
        primitive.transform.localScale = scale;
        Renderer renderer = primitive.GetComponent<Renderer>();
        renderer.material.name = $"{name} Material";
        renderer.material.color = color;
        return primitive;
    }

    private static Renderer? FindRenderer(string name)
    {
        GameObject? existing = GameObject.Find(name);
        return existing is null ? null : existing.GetComponent<Renderer>();
    }

    private static Transform FindOrCreateRoot(string name)
    {
        GameObject? existing = GameObject.Find(name);
        if (existing is not null)
        {
            return existing.transform;
        }

        return new GameObject(name).transform;
    }

    private static Transform FindOrCreateRuntimePrimitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color, Transform parent)
    {
        GameObject? existing = GameObject.Find(name);
        return existing is null
            ? CreateRuntimePrimitive(name, type, position, scale, color, parent).transform
            : existing.transform;
    }

    private static Transform FindOrCreateRuntimePoint(string name, Vector3 position, Transform parent)
    {
        GameObject? existing = GameObject.Find(name);
        if (existing is not null)
        {
            return existing.transform;
        }

        return CreateRuntimePoint(name, position, parent);
    }

    private static Transform CreateRuntimePoint(string name, Vector3 position, Transform parent)
    {
        GameObject point = new(name);
        point.transform.SetParent(parent);
        point.transform.position = position;
        return point.transform;
    }

    private static void CreateRuntimeLabel(string name, string text, Vector3 position, Transform parent)
    {
        GameObject label = new(name);
        label.transform.SetParent(parent);
        label.transform.position = position;
        label.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
        TextMesh mesh = label.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.fontSize = 72;
        mesh.characterSize = 0.045f;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = new Color(0.92f, 0.95f, 1f);
    }

    private static void EnsureRuntimeLabel(string name, string text, Vector3 position, Transform parent)
    {
        if (GameObject.Find(name) is not null)
        {
            return;
        }

        CreateRuntimeLabel(name, text, position, parent);
    }

    private void OnGUI()
    {
        EnsureStyles();

        GUILayout.BeginArea(new Rect(16, 16, 430, Screen.height - 32), GUI.skin.box);
        GUILayout.Label("Yakchonara Portfolio Sample", titleStyle);
        GUILayout.Label("WASD/Arrows move, E interact, R reset, F full demo", labelStyle);
        GUILayout.Label(BuildInteractionPrompt(), logStyle);
        GUILayout.Space(8);

        if (GUILayout.Button("Run Full Demo", GUILayout.Height(32)))
        {
            RunFullDemo();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset", GUILayout.Height(28)))
        {
            ResetDemo();
        }

        if (GUILayout.Button("Enter Scene", GUILayout.Height(28)))
        {
            EnterScene();
        }

        if (GUILayout.Button("Exit Scene", GUILayout.Height(28)))
        {
            ExitScene();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(8);
        GUILayout.Label("Bath Flow", labelStyle);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Queue Herb", GUILayout.Height(28)))
        {
            QueueBathHerb();
        }

        if (GUILayout.Button("Assign Herb", GUILayout.Height(28)))
        {
            AssignBathHerb();
        }

        if (GUILayout.Button("Dock Fire", GUILayout.Height(28)))
        {
            DockBathElemental();
        }

        if (GUILayout.Button("Complete", GUILayout.Height(28)))
        {
            CompleteBath();
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("Sauna Flow", labelStyle);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Queue + Assign", GUILayout.Height(28)))
        {
            QueueAndAssignSaunaHerb();
        }

        if (GUILayout.Button("Dock Fire", GUILayout.Height(28)))
        {
            DockSaunaElemental();
        }

        if (GUILayout.Button("Tick Heat", GUILayout.Height(28)))
        {
            TickSaunaHeat();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(8);
        DrawFacilityState("Bath", bathBinder.CurrentView);
        DrawFacilityState("Sauna", saunaBinder.CurrentView);

        GUILayout.Space(8);
        GUILayout.Label("Event Log", labelStyle);
        foreach (string line in logLines)
        {
            GUILayout.Label(line, logStyle);
        }

        GUILayout.EndArea();
    }

    private void EnsureStyles()
    {
        titleStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        labelStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.92f, 0.95f, 1f) }
        };
        logStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            wordWrap = true,
            normal = { textColor = new Color(0.82f, 0.9f, 1f) }
        };
    }

    private void DrawFacilityState(string label, FacilityViewModel? view)
    {
        if (view is null)
        {
            GUILayout.Label($"{label}: no state", logStyle);
            return;
        }

        GUILayout.Label(
            $"{label}: {view.Status} | occ {view.Occupancy} | wait {view.Waiting} | elem {view.Elementals} | temp {view.Temperature} | gauge {view.Gauge}",
            logStyle);
    }

    private void ResetDemo()
    {
        sceneFlow = new SceneFlowCoordinator();
        sceneFlow.SceneLoaded += sceneName => AddLog($"Scene loaded: {sceneName}");
        sceneFlow.SceneUnloaded += sceneName => AddLog($"Scene unloaded: {sceneName}");

        bath = CreateBath();
        sauna = new SaunaFacility(id: 201, name: "SaunaFacility-A", capacity: 1, lineupCapacity: 2);
        bathContext = new AdditiveSceneContext(SceneName);
        bathContext.RegisterFacility(bath);
        bathContext.RegisterFacility(sauna);
        sceneFlow.RegisterContext(bathContext);

        bathBinder = new FacilityStatusBinder();
        saunaBinder = new FacilityStatusBinder();
        bathBinder.Bind(bath);
        saunaBinder.Bind(sauna);
        bath.StateChanged += snapshot => AddLog($"Bath state: {snapshot.StatusText}");
        sauna.StateChanged += snapshot => AddLog($"Sauna state: {snapshot.StatusText}");

        actionController = new ElementalActionController();
        bathElemental = new ElementalAgent("Elemental-Fire-01", ElementalType.Fire, defaultScale: 1.0);
        saunaElemental = new ElementalAgent("Elemental-Fire-03", ElementalType.Fire, defaultScale: 1.0);
        bathHerbQueued = false;
        bathHerbAssigned = false;
        saunaHerbQueued = false;
        saunaHerbAssigned = false;

        logLines.Clear();
        AddLog(bathContext.Describe());
        RefreshVisuals();
        ResetPlayerPose();
    }

    private void RunFullDemo()
    {
        ResetDemo();
        EnterScene();
        QueueBathHerb();
        AssignBathHerb();
        DockBathElemental();
        QueueAndAssignSaunaHerb();
        DockSaunaElemental();
        TickSaunaHeat();
    }

    private void TryInteract()
    {
        if (playerMarker is null)
        {
            return;
        }

        if (DistanceTo(playerMarker, bathSlotPoint) <= InteractionRange)
        {
            InteractWithBath();
        }
        else if (DistanceTo(playerMarker, saunaSlotPoint) <= InteractionRange)
        {
            InteractWithSauna();
        }
        else if (DistanceTo(playerMarker, exitPoint) <= InteractionRange)
        {
            ExitScene();
        }
        else
        {
            AddLog("Move near Bath, Sauna, or Exit before interacting.");
        }
    }

    private void InteractWithBath()
    {
        if (!sceneFlow.IsInFacility)
        {
            EnterScene();
        }
        else if (!bathHerbQueued && !bathHerbAssigned)
        {
            QueueBathHerb();
        }
        else if (bathHerbQueued)
        {
            AssignBathHerb();
        }
        else if (bathElemental.Status != ElementalStatus.Docked)
        {
            DockBathElemental();
        }
        else
        {
            CompleteBath();
        }
    }

    private void InteractWithSauna()
    {
        if (!sceneFlow.IsInFacility)
        {
            EnterScene();
        }
        else if (!saunaHerbQueued && !saunaHerbAssigned)
        {
            QueueAndAssignSaunaHerb();
        }
        else if (saunaElemental.Status != ElementalStatus.Docked)
        {
            DockSaunaElemental();
        }
        else
        {
            TickSaunaHeat();
        }
    }

    private void EnterScene()
    {
        SceneTransitionResult result = sceneFlow.EnterScene(
            new SceneTransitionRequest(SceneName, new GridPosition(4, 2), "unity-demo-enter"));
        AddLog(result.Message);
        RefreshVisuals();
    }

    private void ExitScene()
    {
        SceneTransitionResult result = sceneFlow.ExitCurrentScene("unity-demo-exit");
        if (result.Success)
        {
            bathHerbQueued = false;
            bathHerbAssigned = false;
            saunaHerbQueued = false;
            saunaHerbAssigned = false;
        }

        AddLog(result.Message);
        RefreshVisuals();
    }

    private void QueueBathHerb()
    {
        FacilityAssignmentResult result = bath.EnqueueHerb(new HerbRequest(BathHerbId, "warm-bath"));
        bathHerbQueued = result.Success || bathHerbQueued;
        AddLog(result.Message);
        RefreshVisuals();
    }

    private void AssignBathHerb()
    {
        FacilityAssignmentResult result = bath.TryAssignNextHerb();
        if (result.Success)
        {
            bathHerbQueued = false;
            bathHerbAssigned = true;
        }

        AddLog(result.Message);
        RefreshVisuals();
    }

    private void DockBathElemental()
    {
        bool captured = actionController.Capture(bathElemental);
        bool assigned = actionController.AssignToFacility(bathElemental, bath);
        AddLog($"Bath elemental captured={captured}, assigned={assigned}, scale={bathElemental.CurrentScale:0.00}");
        RefreshVisuals();
    }

    private void CompleteBath()
    {
        bool completed = bath.CompleteBath(BathHerbId);
        if (completed)
        {
            bathHerbQueued = false;
            bathHerbAssigned = false;
        }

        AddLog($"Bath complete={completed}");
        RefreshVisuals();
    }

    private void QueueAndAssignSaunaHerb()
    {
        FacilityAssignmentResult queue = sauna.EnqueueHerb(new HerbRequest(SaunaHerbId, "sauna"));
        FacilityAssignmentResult assign = sauna.TryAssignNextHerb();
        saunaHerbQueued = queue.Success && !assign.Success;
        saunaHerbAssigned = assign.Success || saunaHerbAssigned;
        AddLog($"{queue.Message} {assign.Message}");
        RefreshVisuals();
    }

    private void DockSaunaElemental()
    {
        bool captured = actionController.Capture(saunaElemental);
        bool assigned = actionController.AssignToFacility(saunaElemental, sauna);
        AddLog($"Sauna elemental captured={captured}, assigned={assigned}, scale={saunaElemental.CurrentScale:0.00}");
        RefreshVisuals();
    }

    private void TickSaunaHeat()
    {
        sauna.TickHeat(0.10);
        AddLog($"Sauna heat ticked to {sauna.HeatGauge:0.00}");
        RefreshVisuals();
    }

    private void AddLog(string message)
    {
        logLines.Insert(0, message);
        if (logLines.Count > 9)
        {
            logLines.RemoveAt(logLines.Count - 1);
        }
    }

    private void RefreshVisuals()
    {
        bool open = sceneFlow.IsInFacility;
        SetRendererColor(bathRenderer, open ? new Color(0.18f, 0.55f, 0.86f) : new Color(0.22f, 0.25f, 0.28f));
        SetRendererColor(saunaRenderer, open ? new Color(0.86f, 0.43f, 0.18f) : new Color(0.22f, 0.25f, 0.28f));

        PlaceMarker(bathHerbMarker, bathHerbQueued || bathHerbAssigned, bathHerbAssigned ? bathSlotPoint : bathWaitingPoint, 0.9f);
        PlaceMarker(saunaHerbMarker, saunaHerbQueued || saunaHerbAssigned, saunaHerbAssigned ? saunaSlotPoint : saunaWaitingPoint, 0.9f);
        PlaceElemental(bathElementalMarker, bathElemental, bathElementalWorldPoint, bathElementalDockPoint);
        PlaceElemental(saunaElementalMarker, saunaElemental, saunaElementalWorldPoint, saunaElementalDockPoint);
    }

    private void HandlePlayerMovement()
    {
        if (playerMarker is null)
        {
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontal -= 1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontal += 1f;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            vertical -= 1f;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            vertical += 1f;
        }

        Vector3 movement = new Vector3(horizontal, 0f, vertical);
        if (movement.sqrMagnitude > 1f)
        {
            movement.Normalize();
        }

        if (movement.sqrMagnitude > 0.001f)
        {
            playerMarker.position += movement * (PlayerMoveSpeed * Time.deltaTime);
            playerMarker.rotation = Quaternion.LookRotation(movement, Vector3.up);
        }

        Camera? camera = Camera.main;
        if (camera is not null)
        {
            camera.transform.position = Vector3.Lerp(camera.transform.position, playerMarker.position + cameraOffset, 8f * Time.deltaTime);
            camera.transform.LookAt(playerMarker.position + Vector3.up * 0.4f);
        }
    }

    private void ResetPlayerPose()
    {
        if (playerMarker is null)
        {
            return;
        }

        playerMarker.position = new Vector3(0f, 0.72f, 2.35f);
        playerMarker.rotation = Quaternion.identity;
        Camera? camera = Camera.main;
        if (camera is not null)
        {
            camera.transform.position = playerMarker.position + cameraOffset;
            camera.transform.LookAt(playerMarker.position + Vector3.up * 0.4f);
        }
    }

    private string BuildInteractionPrompt()
    {
        if (playerMarker is null)
        {
            return "Play mode will create the player and demo floor.";
        }

        if (DistanceTo(playerMarker, bathSlotPoint) <= InteractionRange)
        {
            return "Near Bath: press E to enter, queue, assign, dock fire, and complete.";
        }

        if (DistanceTo(playerMarker, saunaSlotPoint) <= InteractionRange)
        {
            return "Near Sauna: press E to enter, assign sauna herb, dock fire, and heat.";
        }

        if (DistanceTo(playerMarker, exitPoint) <= InteractionRange)
        {
            return "Near Exit: press E to leave the facility scene.";
        }

        return "Move near a facility, then press E.";
    }

    private static float DistanceTo(Transform source, Transform? target)
    {
        if (target is null)
        {
            return float.MaxValue;
        }

        Vector3 sourcePosition = source.position;
        Vector3 targetPosition = target.position;
        sourcePosition.y = 0f;
        targetPosition.y = 0f;
        return Vector3.Distance(sourcePosition, targetPosition);
    }

    private static void SetRendererColor(Renderer? renderer, Color color)
    {
        if (renderer is not null)
        {
            renderer.material.color = color;
        }
    }

    private static void PlaceMarker(Transform? marker, bool active, Transform? point, float scale)
    {
        if (marker is null)
        {
            return;
        }

        marker.gameObject.SetActive(active);
        if (point is not null)
        {
            marker.position = point.position;
        }

        marker.localScale = Vector3.one * scale;
    }

    private static void PlaceElemental(Transform? marker, ElementalAgent elemental, Transform? worldPoint, Transform? dockPoint)
    {
        if (marker is null)
        {
            return;
        }

        marker.gameObject.SetActive(true);
        Transform? point = elemental.Status == ElementalStatus.Docked ? dockPoint : worldPoint;
        if (point is not null)
        {
            marker.position = point.position;
        }

        marker.localScale = Vector3.one * (float)(0.7 * elemental.CurrentScale);
    }
}
