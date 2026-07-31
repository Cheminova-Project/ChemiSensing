using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;

using UnityEngine.XR.OpenXR;

#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
using UnityEditor.Build.Profile;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
#endif
using Debug = UnityEngine.Debug;
using Object = System.Object;

public enum Plugin
{
    OpenXR,
    Oculus,
    ARCore,
    ARKit
}
public enum BuildType
{
    Desktop = 0,
    VR_META = 1,
    MobileiOS = 2,
    MobileAndroid = 3,
    ARiOS = 4,
    ARAndroid = 5,
    WindowsServer = 6,
    LinuxServer = 7,
    LinuxClient = 8,
    MacOS = 9,
    UWP = 10,
    VR_PICO = 11
}

#if UNITY_EDITOR

/// <summary>
/// Permite gestionar y lanzar builds para diferentes perfiles de ChemiSensing.
/// Proporciona métodos para seleccionar y compilar perfiles específicos.
/// </summary>
public class ChemiSensingProfileBuilds : EditorWindow
{
    // Crear estilos personalizados
    private GUIStyle headerStyle, boxStyle, buttonStyle, labelStyle, toggleStyle, separatorStyle, sectionBoxStyle;

    // Definir colores personalizados
    private static Color primaryColor = new Color(18f / 255f, 171f / 255f, 195f / 255f);
    private static Color secondaryColor = new Color(158f / 255f, 200f / 255f, 104f / 255f);
    private static Color darkGray = new Color(60f / 255f, 60f / 255f, 59f / 255f);
    private static Color lightGray = new Color(237f / 255f, 237f / 255f, 237f / 255f);
    private static Color accentColor = new Color(241f / 255f, 136f / 255f, 49f / 255f);

    // Hover: se hace más claro que el accentColor
    private static Color hoverColor = new Color(accentColor.r * 1.1f, accentColor.g * 1.1f, accentColor.b * 1.1f);

    // Active: se hace más oscuro que el accentColor
    private static Color activeColor = new Color(accentColor.r * 0.8f, accentColor.g * 0.8f, accentColor.b * 0.8f);

    private BuildProfile _activeBuildProfile;
    private BuildTargetProfile _activeBuildTargetProfile;
    private List<BuildTargetProfile> _buildTargetProfiles;

    private const string picoFeatureId = "com.picoxr.openxr.features";
    private const string metaFeatureId = "com.unity.openxr.featureset.meta";

    [MenuItem("Builds/ChemiSensing Profile Builds")]
    public static void ShowWindow()
    {
        GetWindow<ChemiSensingProfileBuilds>("ChemiSensing Builds");
    }
    
    
    private void OnEnable()
    {
        _buildTargetProfiles = GetAllBuildTargetProfiles();
        string buildTargetProfilesNames = string.Join(", ", _buildTargetProfiles.Select(p => p.name));
        Debug.Log("BuildTargetProfiles found: " + buildTargetProfilesNames);
        _activeBuildTargetProfile = GetActiveBuildTargetProfile();
        Debug.Log("Active BuildTargetProfile: " + (_activeBuildTargetProfile != null ? _activeBuildTargetProfile.name : "None"));
        _activeBuildProfile = _activeBuildTargetProfile.buildProfile;
        
        SetBuildSettingsSO(_activeBuildTargetProfile.BuildType);
    }

    private void SetBuildSettingsSO(BuildType buildType)
    {
        BuildConfig config = Resources.Load<BuildConfig>("BuildSettings");
        if (config == null)
        {
            Debug.LogError("BuildConfig not found in Resources. Please create a BuildConfig asset.");
            return;
        }
        config.CurrentBuildType = buildType;
        
        // AÑADIR ESTAS LÍNEAS:
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
    }
    
    private BuildTargetProfile GetActiveBuildTargetProfile()
    {
        foreach (var buildTargetProfile in _buildTargetProfiles)
        {
            if (buildTargetProfile.isEnabled)
            {
                Debug.Log("Active BuildTargetProfile found: " + buildTargetProfile.name + " with BuildType: " + buildTargetProfile.BuildType + " and BuildTarget: " + buildTargetProfile.buildTarget);
                return buildTargetProfile;
            }
        }
        Debug.LogError("No active BuildTargetProfile found. Returning the first one as default.");
        // Si no se encuentra un perfil activo, devolvemos el primero de la lista
        return _buildTargetProfiles.FirstOrDefault() ?? null;
    }
    
    private List<BuildTargetProfile> GetAllBuildTargetProfiles()
    {
        var guids = AssetDatabase.FindAssets("t:BuildTargetProfile");
        var profiles = new List<BuildTargetProfile>();

        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var profile = AssetDatabase.LoadAssetAtPath<BuildTargetProfile>(assetPath);
            if (profile != null)
                profiles.Add(profile);
        }
        return profiles;
    }
    
    private void InitStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                normal = { textColor = primaryColor },
                alignment = TextAnchor.MiddleCenter
            };

            boxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = {textColor = lightGray},
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white, background = MakeTex(1, 1, accentColor) },
                hover = { textColor = Color.white, background = MakeTex(1, 1, hoverColor) },  // Cambiar color o fondo al hover
                active = { textColor = Color.white, background = MakeTex(1, 1, activeColor) }   // Cambiar color o fondo cuando está presionado
            };
            labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                normal = { textColor = lightGray},
            };
            
            // Estilo para las cajas que rodean las secciones
            sectionBoxStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                normal = {textColor = lightGray } // Fondo gris claro
            };

            toggleStyle = new GUIStyle(EditorStyles.toggle)
            {
                fontSize = 13,
                normal = { textColor = lightGray }
            };
        }
    }
    
    private static Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    private void OnGUI()
    {
        InitStyles();
        if(_activeBuildTargetProfile == null)
        {
            GUILayout.Label("No active BuildTargetProfile found. Please select one.", headerStyle);
            return;
        }
        
        GUILayout.Space(15);
        GUILayout.Label("Build Configuration", headerStyle);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
        GUILayout.Space(15);

        // Mostrar perfil activo
        GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(40));
        GUILayout.Label("Active Build Type:", labelStyle, GUILayout.Width(150));
        GUILayout.Label(_activeBuildProfile.name, new GUIStyle(labelStyle) { fontSize = 15, fontStyle = FontStyle.Bold, normal = { textColor = primaryColor } }, GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();
        
        GUILayout.Space(20);
        GUILayout.Label("Available Profiles", new GUIStyle(headerStyle) { fontSize = 13 });
        GUILayout.Space(10);

        // Mostrar todos los perfiles en una lista más compacta
        foreach (var buildProfile in _buildTargetProfiles)
        {
            GUILayout.BeginHorizontal(sectionBoxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(45));
            
            // Nombre del perfil
            GUIStyle profileNameStyle = new GUIStyle(headerStyle) { fontSize = 12, alignment = TextAnchor.MiddleLeft };
            if (buildProfile == _activeBuildTargetProfile)
            {
                profileNameStyle.normal.textColor = secondaryColor;
            }
            GUILayout.Label(buildProfile.name, profileNameStyle, GUILayout.Width(150));
            
            GUILayout.FlexibleSpace();
            
            // Botón Switch (solo si no es el activo)
            if (buildProfile != _activeBuildTargetProfile)
            {
                if (GUILayout.Button("Switch", buttonStyle, GUILayout.Width(100), GUILayout.Height(35)))
                    SetActiveBuildProfile(buildProfile);
            }
            else
            {
                GUILayout.Label("✓ Active", new GUIStyle(labelStyle) { fontSize = 12, normal = { textColor = secondaryColor }, fontStyle = FontStyle.Bold }, GUILayout.Width(100), GUILayout.Height(35));
            }
            
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
        
        GUILayout.Space(25);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
        GUILayout.Space(15);

        // Botón Build Project - más grande y prominente
        GUIStyle buildButtonStyle = new GUIStyle(buttonStyle)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white, background = MakeTex(1, 1, accentColor) }
        };

        if (GUILayout.Button("BUILD PROJECT", buildButtonStyle, GUILayout.Height(45)))
        {
            BuildProject(_activeBuildTargetProfile.BuildType);
        }
        
        GUILayout.Space(10);
    }


    private void BuildProject(BuildType buildType)
    {
        BuildTarget buildTarget = GetBuildTargetFromBuildType(buildType);
        // Obtener la carpeta donde guardar el juego.

        //Primero debemos comprobar que existen las carpetas necesarias

        //Obtenemos la carpeta base del proyecto
        string baseAssetsPath = Application.dataPath;
        Debug.Log($"Base path: {baseAssetsPath}");
        string basePath = baseAssetsPath.Substring(0, baseAssetsPath.Length - ("Assets".Length + 1));
        Debug.Log($"Base path without Assets: {basePath}");
        string buildsPath = Path.Combine(basePath, "Builds");
        if (!Directory.Exists(buildsPath))
        {
            Directory.CreateDirectory(buildsPath);
        }
        //Para cada BuildType, creamos una carpeta específica
        if (!Directory.Exists(Path.Combine(basePath, "Builds", buildType.ToString())))
        {
            Debug.Log($"Creating directory for build type: {buildType}");
            Directory.CreateDirectory(Path.Combine(basePath, "Builds", buildType.ToString()));
        }
        else
        {
            Debug.Log($"Directory for build type {buildType} already exists.");
        }

        Debug.Log(buildType.ToString() + " build selected. BuildTarget: " + buildTarget);
        string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", buildsPath, buildType.ToString());
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Path is empty. Operation cancelled");
            return;
        }
        // Definir nombre base del ejecutable
        string baseName = "ChemiSensing";
        string executablePath = path;

        switch (buildTarget)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                executablePath = Path.Combine(path, $"{baseName}.exe");
                break;
            
            case BuildTarget.WSAPlayer:
                executablePath = path;
                break;

            case BuildTarget.StandaloneOSX:
                executablePath = Path.Combine(path, $"{baseName}.app");
                break;

            case BuildTarget.iOS:
                // iOS genera una Xcode project folder, no un archivo ejecutable.
                executablePath = path;
                break;

            case BuildTarget.Android:
                executablePath = Path.Combine(path, $"{baseName}.apk");
                break;

            case BuildTarget.StandaloneLinux64:
                executablePath = Path.Combine(path, baseName); // sin extensión
                break;

            default:
                Debug.LogError($"BuildTarget {buildTarget} not handled explicitly.");
                return;
        }
        if (Directory.Exists(path))
        {
            try
            {
                Directory.Delete(path, true);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to delete build directory: {ex.Message}");
            }
        }

        Debug.Log($"Building {buildType} to: {executablePath}");
        BuildPlayerWithProfileOptions buildTargetWithProfile = new BuildPlayerWithProfileOptions
        {
            locationPathName = executablePath,
            options = BuildOptions.None, // Puedes agregar opciones como BuildOptions.Development si es necesario
            buildProfile = _activeBuildProfile // Usar el perfil activo
        };
        var report = BuildPipeline.BuildPlayer(buildTargetWithProfile);
        Debug.Log($"Build result: {report.summary.result}, total size: {report.summary.totalSize / (1024f * 1024f):0.00} MB");

        if (_activeBuildTargetProfile.BuildType == BuildType.WindowsServer)
        {
            string extraBuildFilesPath = Path.Combine(baseAssetsPath, "ExtraBuildFiles");
            if (Directory.Exists(extraBuildFilesPath))
                CopyDirectoryContents(Path.Combine(baseAssetsPath, "ExtraBuildFiles"), path);
            else
                Debug.LogError("ExtraBuildFiles directory does not exist. Please create it in the Assets folder: " + extraBuildFilesPath);
        }
    }
    
    public static void CopyDirectoryContents(string sourceDir, string destinationDir)
    {
        if (!Directory.Exists(sourceDir))
        {
            Debug.LogError($"Source directory does not exist: {sourceDir}");
            return;
        }

        Directory.CreateDirectory(destinationDir); // Crea destino si no existe

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, true); // Sobrescribe si existe
        }

        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            string destSubDir = Path.Combine(destinationDir, Path.GetFileName(subDir));
            CopyDirectoryContents(subDir, destSubDir); // Recursivo
        }
    }
    
    public void SetActiveBuildProfile(BuildTargetProfile targetProfile)
    {
        if (targetProfile == null)
        {
            Debug.LogError("BuildProfile es null.");
            return;
        }
        
        BuildTarget target = GetBuildTargetFromBuildType(targetProfile.BuildType);
        BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

        if (EditorUserBuildSettings.activeBuildTarget != target)
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
        
        // Internamente accede al asset config
        BuildProfile.SetActiveBuildProfile(targetProfile.buildProfile);

        Debug.Log($"✅ BuildProfile activo cambiado a: {targetProfile.name}");
        
        //Es importante cambiar algunos project settings según la plataforma, de momento son:
        // XR plugins según plataforma (AR foundation y ARKit en AR iOS, Open XR en VR, AR Foundarion y ARCore en AR Android)
        // Gráficos o pipelines en AR, en movil, en Windows...
        ConfigureXRLoaders(targetProfile.BuildType);
        _activeBuildProfile = targetProfile.buildProfile;
        _activeBuildTargetProfile = targetProfile;
        
        //Desactivamos ahora el campo "enabled" de todos los BuildTargetProfiles menos el elegido
        foreach (var profile in _buildTargetProfiles)
        {
            profile.isEnabled = (profile == targetProfile);
            EditorUtility.SetDirty(profile);
        }
        
        AssetDatabase.SaveAssets();
        SetBuildSettingsSO(targetProfile.BuildType);
        GUIUtility.ExitGUI();
    }

    private static void ConfigureXRLoaders(BuildType buildType)
    {
        BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(GetBuildTargetFromBuildType(buildType));
        DisableAllPlugins(buildTargetGroup);
        // Agregar el loader adecuado según el tipo de build
        if (buildType == BuildType.ARiOS)
            EnablePlugin(buildTargetGroup, Plugin.ARKit);
        else if (buildType == BuildType.ARAndroid)
            EnablePlugin(buildTargetGroup, Plugin.ARCore);
        else if (buildType == BuildType.VR_META)
        {
            EnablePlugin(buildTargetGroup, Plugin.OpenXR);
            ApplyMeta();
        }
        else if (buildType == BuildType.VR_PICO)
        {
            EnablePlugin(buildTargetGroup, Plugin.OpenXR);
            ApplyPico();
        }
    }
    
    public static void EnablePlugin(BuildTargetGroup buildTargetGroup, Plugin plugin)
    {
        var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);
        
        if (buildTargetSettings == null || buildTargetSettings.AssignedSettings == null) 
            return;
        
        var pluginsSettings = buildTargetSettings.AssignedSettings;
        var success = XRPackageMetadataStore.AssignLoader(pluginsSettings, GetLoaderName(plugin), buildTargetGroup);
        if (success)
        {
            Debug.Log($"XR Plug-in Management: Enabled {plugin} plugin on {buildTargetGroup}");
        }
    }
    
    public static void DisablePlugin(BuildTargetGroup buildTargetGroup, Plugin plugin)
    {
        var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);
        
        if (buildTargetSettings == null || buildTargetSettings.AssignedSettings == null) 
            return;
        
        var pluginsSettings = buildTargetSettings.AssignedSettings;
        var success = XRPackageMetadataStore.RemoveLoader(pluginsSettings, GetLoaderName(plugin), buildTargetGroup);
        if (success)
        {
            Debug.Log($"XR Plug-in Management: Disabled {plugin} plugin on {buildTargetGroup}");
        }
    }
    
    public static void DisableAllPlugins(BuildTargetGroup buildTargetGroup)
    {
        DisablePlugin(buildTargetGroup, Plugin.Oculus);
        DisablePlugin(buildTargetGroup, Plugin.OpenXR);
        DisablePlugin(buildTargetGroup, Plugin.ARCore);
        DisablePlugin(buildTargetGroup, Plugin.ARKit);
    }
    
    public static BuildTarget GetBuildTargetFromBuildType(BuildType buildType)
    {
        BuildTarget target = buildType switch
        {
            BuildType.Desktop => BuildTarget.StandaloneWindows64,
            BuildType.UWP => BuildTarget.WSAPlayer,
            BuildType.VR_META => BuildTarget.Android,
            BuildType.VR_PICO => BuildTarget.Android,
            BuildType.MobileiOS => BuildTarget.iOS,
            BuildType.MobileAndroid => BuildTarget.Android,
            BuildType.ARiOS => BuildTarget.iOS,
            BuildType.ARAndroid => BuildTarget.Android,
            BuildType.WindowsServer => BuildTarget.StandaloneWindows64,
            BuildType.LinuxServer => BuildTarget.StandaloneLinux64,
            BuildType.LinuxClient => BuildTarget.StandaloneLinux64,
            BuildType.MacOS => BuildTarget.StandaloneOSX,
            _ => EditorUserBuildSettings.activeBuildTarget,
        };
        return target;
    }


    private static void ApplyMeta()
    {
        var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);

        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;


        BuildTargetGroup buildTargetGroup = BuildTargetGroup.Android;

        // 1. Disable Pico XR Feature Group
        ToggleOpenXRFeature(buildTargetGroup, picoFeatureId, false);

        // 2. Enable Meta XR Feature Group
        ToggleOpenXRFeature(buildTargetGroup, metaFeatureId, true);

        // Primero desactivar todo
        foreach (var feature in settings.GetFeatures())
            feature.enabled = false;

        // META necesarios
        SetFeature(settings, "MetaQuestFeature", true);
        SetFeature(settings, "ARCameraFeature", true);
        SetFeature(settings, "ARSessionFeature", true);

        // Controllers
        SetFeature(settings, "OculusTouchControllerProfile", true);
        SetFeature(settings, "MetaQuestTouchPlusControllerProfile", true);
        SetFeature(settings, "MetaQuestTouchProControllerProfile", true);

        PlayerSettings.Android.minSdkVersion =
            AndroidSdkVersions.AndroidApiLevel32;

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        Debug.Log("META configuration applied");
    }

    private static void ApplyPico()
    {
        var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);

        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;

        BuildTargetGroup buildTargetGroup = BuildTargetGroup.Android;
        // 1. Disable Pico XR Feature Group
        ToggleOpenXRFeature(buildTargetGroup, picoFeatureId, true);

        // 2. Enable Meta XR Feature Group
        ToggleOpenXRFeature(buildTargetGroup, metaFeatureId, false);


        // Primero desactivar todo
        foreach (var feature in settings.GetFeatures())
            feature.enabled = false;

        // PICO necesarios
        SetFeature(settings, "PICOFeature", true);
        //SetFeature(settings, "PassthroughFeature", true);
        SetFeature(settings, "LayerSecureContentFeature", true);
        SetFeature(settings, "OpenXRCompositionLayersFeature", true);

        // Controllers
        SetFeature(settings, "PICO4ControllerProfile", true);
        SetFeature(settings, "PICONeo3ControllerProfile", true);
        SetFeature(settings, "PICO4UltraControllerProfile", true);
        SetFeature(settings, "PICOG3ControllerProfile", true);

        PlayerSettings.Android.minSdkVersion =
            AndroidSdkVersions.AndroidApiLevel29;

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        Debug.Log("PICO configuration applied");
    }

    private static void ToggleOpenXRFeature(BuildTargetGroup group, string featureId, bool enable)
    {
        OpenXRFeatureSetManager.FeatureSet featureSet = OpenXRFeatureSetManager.GetFeatureSetWithId(group, featureId);

        if (featureSet != null)
        {
            featureSet.isEnabled = enable;
        }
        else
        {
            Debug.LogError($"Could not find feature set with ID: {featureId}");
            return;
        }

        OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(group);
        AssetDatabase.SaveAssets();
    }

    private static void SetFeature(OpenXRSettings settings, string typeNameContains, bool enabled)
    {
        foreach (var feature in settings.GetFeatures())
        {
            string fullName = feature.GetType().FullName ?? "";

            if (fullName.Contains(typeNameContains))
            {
                feature.enabled = enabled;
                Debug.Log($"{(enabled ? "Enabled" : "Disabled")} {feature.name}");
            }
        }
    }

    static string GetLoaderName(Plugin plugin) => plugin switch
    {
        Plugin.OpenXR => "UnityEngine.XR.OpenXR.OpenXRLoader",
        Plugin.Oculus => "Unity.XR.Oculus.OculusLoader",
        Plugin.ARCore => "UnityEngine.XR.ARCore.ARCoreLoader",
        Plugin.ARKit => "UnityEngine.XR.ARKit.ARKitLoader",
        _ => throw new NotImplementedException()
    };
}

#endif
