using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Yakchonara.PortfolioSample.Demo.Unity.EditorTools;

public static class PortfolioSampleSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/PortfolioSampleDemo.unity";

    [MenuItem("Yakchonara/Build Portfolio Sample Demo Scene")]
    public static void Build()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject root = new("Portfolio Sample Demo");
        CreateCamera();
        CreateLight();
        CreatePrimitive("Ground", PrimitiveType.Cube, new Vector3(0f, -0.08f, 0f), new Vector3(8.5f, 0.1f, 5.4f), new Color(0.16f, 0.18f, 0.20f));

        Renderer bathRenderer = CreatePrimitive("Bath Facility", PrimitiveType.Cube, new Vector3(-2.1f, 0.15f, 0f), new Vector3(2.4f, 0.28f, 2.3f), new Color(0.18f, 0.55f, 0.86f)).GetComponent<Renderer>();
        Renderer saunaRenderer = CreatePrimitive("Sauna Facility", PrimitiveType.Cube, new Vector3(2.1f, 0.15f, 0f), new Vector3(2.4f, 0.28f, 2.3f), new Color(0.86f, 0.43f, 0.18f)).GetComponent<Renderer>();
        bathRenderer.transform.SetParent(root.transform);
        saunaRenderer.transform.SetParent(root.transform);

        CreateLabel("Bath Label", "Bath Facility", new Vector3(-2.1f, 0.55f, -1.35f), root.transform);
        CreateLabel("Sauna Label", "Sauna Facility", new Vector3(2.1f, 0.55f, -1.35f), root.transform);
        CreateLabel("World Label", "Elementals", new Vector3(0f, 0.55f, 1.95f), root.transform);

        Transform bathWaitingPoint = CreatePoint("Bath Herb Waiting Point", new Vector3(-3.45f, 0.55f, 1.35f), root.transform);
        Transform bathSlotPoint = CreatePoint("Bath Herb Slot Point", new Vector3(-2.1f, 0.62f, 0f), root.transform);
        Transform saunaWaitingPoint = CreatePoint("Sauna Herb Waiting Point", new Vector3(3.45f, 0.55f, 1.35f), root.transform);
        Transform saunaSlotPoint = CreatePoint("Sauna Herb Slot Point", new Vector3(2.1f, 0.62f, 0f), root.transform);
        Transform bathElementalWorldPoint = CreatePoint("Bath Elemental World Point", new Vector3(-0.55f, 0.7f, 2f), root.transform);
        Transform bathElementalDockPoint = CreatePoint("Bath Elemental Dock Point", new Vector3(-3.15f, 0.72f, -0.85f), root.transform);
        Transform saunaElementalWorldPoint = CreatePoint("Sauna Elemental World Point", new Vector3(0.55f, 0.7f, 2f), root.transform);
        Transform saunaElementalDockPoint = CreatePoint("Sauna Elemental Dock Point", new Vector3(3.15f, 0.72f, -0.85f), root.transform);
        Transform exitPoint = CreatePoint("Exit Interaction Point", new Vector3(0f, 0.05f, 2.35f), root.transform);

        Transform bathHerbMarker = CreatePrimitive("Bath Herb Marker", PrimitiveType.Capsule, bathWaitingPoint.position, Vector3.one * 0.55f, new Color(0.45f, 0.86f, 0.48f)).transform;
        Transform saunaHerbMarker = CreatePrimitive("Sauna Herb Marker", PrimitiveType.Capsule, saunaWaitingPoint.position, Vector3.one * 0.55f, new Color(0.60f, 0.90f, 0.50f)).transform;
        Transform bathElementalMarker = CreatePrimitive("Bath Fire Elemental Marker", PrimitiveType.Sphere, bathElementalWorldPoint.position, Vector3.one * 0.7f, new Color(1f, 0.35f, 0.12f)).transform;
        Transform saunaElementalMarker = CreatePrimitive("Sauna Fire Elemental Marker", PrimitiveType.Sphere, saunaElementalWorldPoint.position, Vector3.one * 0.7f, new Color(1f, 0.50f, 0.16f)).transform;
        Transform playerMarker = CreatePrimitive("Playable Herb Runner", PrimitiveType.Capsule, new Vector3(0f, 0.72f, 2.35f), new Vector3(0.65f, 0.9f, 0.65f), new Color(0.95f, 0.88f, 0.24f)).transform;
        bathHerbMarker.SetParent(root.transform);
        saunaHerbMarker.SetParent(root.transform);
        bathElementalMarker.SetParent(root.transform);
        saunaElementalMarker.SetParent(root.transform);
        playerMarker.SetParent(root.transform);

        GameObject controllerObject = new("Portfolio Sample Demo Controller");
        PortfolioSampleDemoController controller = controllerObject.AddComponent<PortfolioSampleDemoController>();
        controller.Configure(
            bathRenderer,
            saunaRenderer,
            bathHerbMarker,
            saunaHerbMarker,
            bathWaitingPoint,
            bathSlotPoint,
            saunaWaitingPoint,
            saunaSlotPoint,
            bathElementalMarker,
            saunaElementalMarker,
            bathElementalWorldPoint,
            bathElementalDockPoint,
            saunaElementalWorldPoint,
            saunaElementalDockPoint,
            playerMarker,
            exitPoint);
        controllerObject.transform.SetParent(root.transform);

        EnsureFolder("Assets/Scenes");
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        EnsureBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Built demo scene at {ScenePath}");
    }

    [MenuItem("Yakchonara/Validate Portfolio Sample Demo Scene")]
    public static void Validate()
    {
        if (!System.IO.File.Exists(ScenePath))
        {
            throw new System.IO.FileNotFoundException("Portfolio sample demo scene is missing.", ScenePath);
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (Camera.main == null)
        {
            throw new System.InvalidOperationException("Portfolio sample demo scene has no MainCamera.");
        }

        PortfolioSampleDemoController[] controllers = UnityEngine.Object.FindObjectsByType<PortfolioSampleDemoController>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        if (controllers.Length == 0)
        {
            PortfolioSampleDemoController.EnsureControllerForActiveDemoScene();
            controllers = UnityEngine.Object.FindObjectsByType<PortfolioSampleDemoController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            if (controllers.Length == 0)
            {
                throw new System.InvalidOperationException("Portfolio sample demo scene could not create a PortfolioSampleDemoController.");
            }
        }

        bool isRegistered = EditorBuildSettings.scenes.Any(scene => scene.enabled && scene.path == ScenePath);
        if (!isRegistered)
        {
            throw new System.InvalidOperationException("Portfolio sample demo scene is not registered in Build Settings.");
        }

        Debug.Log("Portfolio sample demo scene validated.");
    }

    private static void CreateCamera()
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

    private static void CreateLight()
    {
        GameObject lightObject = new("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObject.transform.rotation = Quaternion.Euler(52f, -30f, 0f);
    }

    private static GameObject CreatePrimitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
    {
        GameObject primitive = GameObject.CreatePrimitive(type);
        primitive.name = name;
        primitive.transform.position = position;
        primitive.transform.localScale = scale;
        primitive.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name, color);
        return primitive;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Lit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = new(shader)
        {
            name = $"{name} Material",
            color = color
        };
        return material;
    }

    private static Transform CreatePoint(string name, Vector3 position, Transform parent)
    {
        GameObject point = new(name);
        point.transform.SetParent(parent);
        point.transform.position = position;
        return point.transform;
    }

    private static void CreateLabel(string name, string text, Vector3 position, Transform parent)
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

    private static void EnsureFolder(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder))
        {
            string parent = System.IO.Path.GetDirectoryName(folder)?.Replace('\\', '/') ?? "Assets";
            string child = System.IO.Path.GetFileName(folder);
            if (!string.IsNullOrWhiteSpace(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static void EnsureBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
            .Where(scene => scene.path != scenePath)
            .ToList();
        scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
