using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Surfaces;

[InitializeOnLoad]
public static class Lab4SceneBuilder
{
    private const string PokePrefabGuid = "5fca3702cdf1a214a94e69dcfa704e17";
    private const string HandGrabPrefabGuid = "6ee61821e0d5b094a8d732834b365b21";
    private const string MaterialsFolder = "Assets/Materials/Lab4";

    static Lab4SceneBuilder()
    {
        EditorApplication.delayCall += TryAutoCreateOnce;
    }

    private static void TryAutoCreateOnce()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (SceneManager.GetActiveScene().name != "SampleScene")
        {
            return;
        }

        GameObject existing = GameObject.Find("Lab4_Workbench");
        if (existing != null)
        {
            Transform cp = existing.transform.Find("ControlPanel3D");
            if (cp != null && cp.eulerAngles.x > 180f)
            {
                BuildLab4Workbench();
            }
            return;
        }

        BuildLab4Workbench();
    }

    [MenuItem("Tools/Lab 4/Create 3D Control Panel & Assembly Stand")]
    public static void BuildLab4Workbench()
    {
        GameObject existing = GameObject.Find("Lab4_Workbench");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
        }

        EnsureFolder("Assets/Materials");
        EnsureFolder(MaterialsFolder);

        Material panelMat = CreateUrpMaterial("M_Lab4_Panel", new Color(0.18f, 0.20f, 0.24f, 1f));
        Material darkMetalMat = CreateUrpMaterial("M_Lab4_Metal", new Color(0.32f, 0.35f, 0.40f, 1f));
        Material buttonMat = CreateUrpMaterial("M_Lab4_Button", new Color(0.20f, 0.55f, 0.90f, 1f));
        Material leverMat = CreateUrpMaterial("M_Lab4_Lever", new Color(0.85f, 0.85f, 0.88f, 1f));
        Material indicatorMat = CreateUrpMaterial("M_Lab4_Indicator", new Color(0.85f, 0.20f, 0.20f, 1f));
        Material lampMat = CreateUrpMaterial("M_Lab4_Lamp", new Color(0.90f, 0.55f, 0.15f, 1f));
        Material handleMat = CreateUrpMaterial("M_Lab4_SliderHandle", new Color(0.25f, 0.70f, 0.95f, 1f));
        Material socketAMat = CreateUrpMaterial("M_Lab4_SocketA", new Color(0.25f, 0.45f, 0.75f, 0.75f));
        Material socketBMat = CreateUrpMaterial("M_Lab4_SocketB", new Color(0.75f, 0.48f, 0.22f, 0.75f));
        Material moduleAMat = CreateUrpMaterial("M_Lab4_ModuleA", new Color(0.18f, 0.52f, 0.95f, 1f));
        Material moduleBMat = CreateUrpMaterial("M_Lab4_ModuleB", new Color(0.95f, 0.52f, 0.15f, 1f));

        GameObject root = new GameObject("Lab4_Workbench");
        Undo.RegisterCreatedObjectUndo(root, "Create Lab4 Workbench");

        GameObject controlPanel = new GameObject("ControlPanel3D");
        controlPanel.transform.SetParent(root.transform, false);
        controlPanel.transform.position = new Vector3(-0.36f, 0.91f, 1.48f);
        controlPanel.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

        CreatePrimitive(PrimitiveType.Cube, "PanelBoard", controlPanel.transform,
            new Vector3(0f, 0f, 0f), new Vector3(0.50f, 0.024f, 0.26f), panelMat);

        PhysicalToggleSwitch toggleSwitch = CreateToggleSwitch(controlPanel.transform, darkMetalMat, leverMat, indicatorMat);
        PhysicalSlider3D slider3D = CreateSlider3D(controlPanel.transform, darkMetalMat, handleMat);
        PhysicalPokeButton pokeButton = CreatePokeButton(controlPanel.transform, darkMetalMat, buttonMat);

        GameObject statusLamp = CreatePrimitive(PrimitiveType.Sphere, "StatusLamp", controlPanel.transform,
            new Vector3(0.19f, 0.026f, 0.085f), new Vector3(0.028f, 0.028f, 0.028f), lampMat);

        TextMeshProUGUI statusText = CreatePanelLabel(controlPanel.transform, "StatusDisplay",
            new Vector3(-0.02f, 0.07f, 0.115f), new Vector2(360f, 95f), 0.001f, 22f,
            "Зберіть вузол: 0/2 деталей");

        GameObject standRoot = new GameObject("AssemblyStand3D");
        standRoot.transform.SetParent(root.transform, false);
        standRoot.transform.position = new Vector3(0.32f, 0.855f, 1.56f);

        CreatePrimitive(PrimitiveType.Cube, "StandBase", standRoot.transform,
            new Vector3(0f, 0.008f, 0f), new Vector3(0.42f, 0.016f, 0.22f), panelMat);

        GameObject moduleA = CreateGrabbableModule("Module_A", root.transform,
            new Vector3(0.20f, 0.905f, 1.33f), PrimitiveType.Cylinder,
            new Vector3(0.065f, 0.045f, 0.065f), moduleAMat);

        GameObject moduleB = CreateGrabbableModule("Module_B", root.transform,
            new Vector3(0.44f, 0.905f, 1.33f), PrimitiveType.Cube,
            new Vector3(0.07f, 0.07f, 0.07f), moduleBMat);

        SnapZone socketA = CreateAssemblySocket("Socket_ModuleA", standRoot.transform,
            new Vector3(-0.10f, 0.024f, 0f), new Vector3(0.12f, 0.018f, 0.12f),
            new Vector3(0f, 0.045f, 0f), socketAMat, moduleA);

        SnapZone socketB = CreateAssemblySocket("Socket_ModuleB", standRoot.transform,
            new Vector3(0.10f, 0.024f, 0f), new Vector3(0.12f, 0.018f, 0.12f),
            new Vector3(0f, 0.040f, 0f), socketBMat, moduleB);

        AssemblyStandController controller = root.AddComponent<AssemblyStandController>();
        SerializedObject ctrlSo = new SerializedObject(controller);
        SerializedProperty socketsProp = ctrlSo.FindProperty("sockets");
        socketsProp.arraySize = 2;
        socketsProp.GetArrayElementAtIndex(0).objectReferenceValue = socketA;
        socketsProp.GetArrayElementAtIndex(1).objectReferenceValue = socketB;
        ctrlSo.FindProperty("powerToggle").objectReferenceValue = toggleSwitch;
        ctrlSo.FindProperty("voltageSlider").objectReferenceValue = slider3D;
        ctrlSo.FindProperty("statusLampRenderer").objectReferenceValue = statusLamp.GetComponent<Renderer>();
        ctrlSo.FindProperty("statusText").objectReferenceValue = statusText;
        ctrlSo.ApplyModifiedPropertiesWithoutUndo();

        BindVoidEvent(pokeButton, "onPressed", controller.RunDiagnosticTest);
        BindVoidEvent(socketA, "onSnapped", controller.OnAssemblyChanged);
        BindVoidEvent(socketA, "onUnsnapped", controller.OnAssemblyChanged);
        BindVoidEvent(socketB, "onSnapped", controller.OnAssemblyChanged);
        BindVoidEvent(socketB, "onUnsnapped", controller.OnAssemblyChanged);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = root;
    }

    private static PhysicalPokeButton CreatePokeButton(Transform parent, Material baseMat, Material capMat)
    {
        GameObject buttonRoot = new GameObject("PokeButton3D");
        buttonRoot.transform.SetParent(parent, false);
        buttonRoot.transform.localPosition = new Vector3(0.16f, 0.012f, -0.02f);

        CreatePrimitive(PrimitiveType.Cylinder, "Base", buttonRoot.transform,
            new Vector3(0f, 0.008f, 0f), new Vector3(0.075f, 0.008f, 0.075f), baseMat);

        GameObject cap = CreatePrimitive(PrimitiveType.Cylinder, "Cap", buttonRoot.transform,
            new Vector3(0f, 0.022f, 0f), new Vector3(0.058f, 0.012f, 0.058f), capMat);

        PokeInteractable poke = InstantiatePokeZone(buttonRoot.transform,
            new Vector3(0f, 0.036f, 0f), new Vector3(0.075f, 0.075f, 0.05f), false);

        CreatePanelLabel(buttonRoot.transform, "Label",
            new Vector3(0f, 0.004f, -0.07f), new Vector2(150f, 45f), 0.0009f, 26f, "ТЕСТ", true);

        PhysicalPokeButton script = buttonRoot.AddComponent<PhysicalPokeButton>();
        SerializedObject so = new SerializedObject(script);
        so.FindProperty("pokeInteractable").objectReferenceValue = poke;
        so.FindProperty("buttonCap").objectReferenceValue = cap.transform;
        so.FindProperty("capRenderer").objectReferenceValue = cap.GetComponent<Renderer>();
        so.ApplyModifiedPropertiesWithoutUndo();
        return script;
    }

    private static PhysicalToggleSwitch CreateToggleSwitch(Transform parent, Material baseMat, Material leverMat, Material indicatorMat)
    {
        GameObject toggleRoot = new GameObject("ToggleSwitch3D");
        toggleRoot.transform.SetParent(parent, false);
        toggleRoot.transform.localPosition = new Vector3(-0.16f, 0.012f, -0.02f);

        CreatePrimitive(PrimitiveType.Cube, "Base", toggleRoot.transform,
            new Vector3(0f, 0.008f, 0f), new Vector3(0.065f, 0.016f, 0.065f), baseMat);

        GameObject pivot = new GameObject("Pivot");
        pivot.transform.SetParent(toggleRoot.transform, false);
        pivot.transform.localPosition = new Vector3(0f, 0.014f, 0f);

        CreatePrimitive(PrimitiveType.Cylinder, "Lever", pivot.transform,
            new Vector3(0f, 0.022f, 0f), new Vector3(0.014f, 0.022f, 0.014f), leverMat);

        GameObject indicator = CreatePrimitive(PrimitiveType.Sphere, "Indicator", toggleRoot.transform,
            new Vector3(0f, 0.018f, 0.052f), new Vector3(0.022f, 0.022f, 0.022f), indicatorMat);

        PokeInteractable poke = InstantiatePokeZone(toggleRoot.transform,
            new Vector3(0f, 0.048f, 0f), new Vector3(0.075f, 0.075f, 0.05f), true);

        CreatePanelLabel(toggleRoot.transform, "Label",
            new Vector3(0f, 0.004f, -0.07f), new Vector2(160f, 45f), 0.0009f, 24f, "ЖИВЛЕННЯ", true);

        PhysicalToggleSwitch script = toggleRoot.AddComponent<PhysicalToggleSwitch>();
        SerializedObject so = new SerializedObject(script);
        so.FindProperty("pokeInteractable").objectReferenceValue = poke;
        so.FindProperty("leverTransform").objectReferenceValue = pivot.transform;
        so.FindProperty("indicatorRenderer").objectReferenceValue = indicator.GetComponent<Renderer>();
        so.ApplyModifiedPropertiesWithoutUndo();
        return script;
    }

    private static PhysicalSlider3D CreateSlider3D(Transform parent, Material railMat, Material handleMat)
    {
        GameObject sliderRoot = new GameObject("Slider3D");
        sliderRoot.transform.SetParent(parent, false);
        sliderRoot.transform.localPosition = new Vector3(0f, 0.012f, -0.02f);

        CreatePrimitive(PrimitiveType.Cube, "Track", sliderRoot.transform,
            new Vector3(0f, 0.006f, 0f), new Vector3(0.18f, 0.012f, 0.022f), railMat);

        GameObject handle = CreatePrimitive(PrimitiveType.Cube, "Handle", sliderRoot.transform,
            new Vector3(0f, 0.022f, 0f), new Vector3(0.028f, 0.032f, 0.036f), handleMat);

        PokeInteractable poke = InstantiatePokeZone(sliderRoot.transform,
            new Vector3(0f, 0.040f, 0f), new Vector3(0.19f, 0.065f, 0.05f), false);

        CreatePanelLabel(sliderRoot.transform, "TitleLabel",
            new Vector3(0f, 0.004f, 0.042f), new Vector2(160f, 40f), 0.0009f, 22f, "НАПРУГА", true);

        TextMeshProUGUI valueLabel = CreatePanelLabel(sliderRoot.transform, "ValueLabel",
            new Vector3(0f, 0.004f, -0.07f), new Vector2(160f, 45f), 0.0009f, 26f, "12 V", true);

        PhysicalSlider3D script = sliderRoot.AddComponent<PhysicalSlider3D>();
        SerializedObject so = new SerializedObject(script);
        so.FindProperty("trackPokeInteractable").objectReferenceValue = poke;
        so.FindProperty("handleTransform").objectReferenceValue = handle.transform;
        so.FindProperty("minLocalX").floatValue = -0.075f;
        so.FindProperty("maxLocalX").floatValue = 0.075f;
        so.FindProperty("minValue").floatValue = 0f;
        so.FindProperty("maxValue").floatValue = 24f;
        so.FindProperty("initialNormalizedValue").floatValue = 0.5f;
        so.FindProperty("valueLabel").objectReferenceValue = valueLabel;
        so.ApplyModifiedPropertiesWithoutUndo();
        return script;
    }

    private static GameObject CreateGrabbableModule(string name, Transform parent, Vector3 worldPos,
        PrimitiveType type, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.position = worldPos;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.mass = 0.3f;

        Grabbable grabbable = obj.AddComponent<Grabbable>();
        SerializedObject grabSo = new SerializedObject(grabbable);
        grabSo.FindProperty("_rigidbody").objectReferenceValue = rb;
        grabSo.FindProperty("_targetTransform").objectReferenceValue = obj.transform;
        grabSo.ApplyModifiedPropertiesWithoutUndo();

        string prefabPath = AssetDatabase.GUIDToAssetPath(HandGrabPrefabGuid);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            GameObject interactableObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, obj.transform);
            interactableObj.name = "ISDK_HandGrabInteraction";

            HandGrabInteractable handGrab = interactableObj.GetComponent<HandGrabInteractable>();
            if (handGrab != null)
            {
                SerializedObject hgSo = new SerializedObject(handGrab);
                hgSo.FindProperty("_pointableElement").objectReferenceValue = grabbable;
                hgSo.FindProperty("_rigidbody").objectReferenceValue = rb;
                hgSo.ApplyModifiedPropertiesWithoutUndo();
            }

            GrabInteractable grab = interactableObj.GetComponent<GrabInteractable>();
            if (grab != null)
            {
                SerializedObject gSo = new SerializedObject(grab);
                gSo.FindProperty("_pointableElement").objectReferenceValue = grabbable;
                gSo.FindProperty("_rigidbody").objectReferenceValue = rb;
                gSo.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        return obj;
    }

    private static SnapZone CreateAssemblySocket(string name, Transform parent, Vector3 localPos,
        Vector3 scale, Vector3 snapPointLocalOffset, Material mat, GameObject requiredPart)
    {
        GameObject socketObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        socketObj.name = name;
        socketObj.transform.SetParent(parent, false);
        socketObj.transform.localPosition = localPos;
        socketObj.transform.localScale = scale;
        socketObj.GetComponent<Renderer>().sharedMaterial = mat;

        BoxCollider col = socketObj.GetComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(1.1f, 3.5f, 1.1f);
        col.center = new Vector3(0f, 1.0f, 0f);

        GameObject snapPoint = new GameObject("SnapPoint");
        snapPoint.transform.SetParent(socketObj.transform, false);
        snapPoint.transform.position = socketObj.transform.position + snapPointLocalOffset;

        SnapZone zone = socketObj.AddComponent<SnapZone>();
        SerializedObject so = new SerializedObject(zone);
        so.FindProperty("snapPoint").objectReferenceValue = snapPoint.transform;
        so.FindProperty("requiredPart").objectReferenceValue = requiredPart;
        so.FindProperty("hoverColor").colorValue = Color.yellow;
        so.FindProperty("snappedColor").colorValue = new Color(0.15f, 0.95f, 0.35f, 1f);
        so.FindProperty("wrongPartColor").colorValue = Color.red;
        so.ApplyModifiedPropertiesWithoutUndo();

        return zone;
    }

    private static PokeInteractable InstantiatePokeZone(Transform parent, Vector3 localPos, Vector3 localScale, bool doubleSided)
    {
        string prefabPath = AssetDatabase.GUIDToAssetPath(PokePrefabGuid);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject pokeObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        pokeObj.name = "ISDK_PokeInteraction";
        pokeObj.transform.localPosition = localPos;
        pokeObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        pokeObj.transform.localScale = localScale;

        Transform visuals = pokeObj.transform.Find("Visuals");
        if (visuals != null)
        {
            visuals.gameObject.SetActive(false);
        }

        PlaneSurface planeSurface = pokeObj.GetComponentInChildren<PlaneSurface>();
        if (planeSurface != null && doubleSided)
        {
            SerializedObject planeSo = new SerializedObject(planeSurface);
            planeSo.FindProperty("_doubleSided").boolValue = true;
            planeSo.ApplyModifiedPropertiesWithoutUndo();
        }

        return pokeObj.GetComponentInChildren<PokeInteractable>();
    }

    private static TextMeshProUGUI CreatePanelLabel(Transform parent, string name, Vector3 localPos,
        Vector2 sizeDelta, float scale, float fontSize, string text, bool flatOnPanel = false)
    {
        GameObject canvasObj = new GameObject(name);
        canvasObj.transform.SetParent(parent, false);
        canvasObj.transform.localPosition = localPos;
        canvasObj.transform.localRotation = flatOnPanel ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one * scale;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = sizeDelta;

        if (!flatOnPanel)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(canvasObj.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.10f, 0.14f, 0.92f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
        }

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform textRect = tmp.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 4f);
        textRect.offsetMax = new Vector2(-8f, -4f);

        return tmp;
    }

    private static GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent,
        Vector3 localPos, Vector3 localScale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = localScale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }

    private static Material CreateUrpMaterial(string fileName, Color color)
    {
        string path = $"{MaterialsFolder}/{fileName}.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.color = color;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            urpLit = Shader.Find("Standard");
        }

        Material mat = new Material(urpLit);
        mat.color = color;
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static void BindVoidEvent(Object targetComponent, string eventFieldName, UnityEngine.Events.UnityAction action)
    {
        UnityEngine.Events.UnityEvent evt = GetUnityEvent(targetComponent, eventFieldName);
        if (evt != null)
        {
            UnityEventTools.AddVoidPersistentListener(evt, action);
            EditorUtility.SetDirty(targetComponent);
        }
    }

    private static UnityEngine.Events.UnityEvent GetUnityEvent(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        if (field == null) return null;
        var evt = field.GetValue(target) as UnityEngine.Events.UnityEvent;
        if (evt == null)
        {
            evt = new UnityEngine.Events.UnityEvent();
            field.SetValue(target, evt);
        }
        return evt;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string name = Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, name);
    }
}
