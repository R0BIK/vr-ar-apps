using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FixMetaInteractionShaders
{
    private static readonly (string srcRel, string destRel)[] ShaderMappings = new[]
    {
        ("TransparentVertexTexture.shader", "Runtime/Shaders/Locomotion/TransparentVertexTexture.shader"),
        ("Hotspot.shader", "Runtime/Shaders/Locomotion/Hotspot.shader"),
        ("OculusControllerRayShader.shader", "Runtime/Shaders/OculusControllerRayShader.shader"),
        ("OculusRayVisual.shader", "Runtime/Shaders/OculusRayVisual.shader"),
        ("OculusHandCursor.shader", "Runtime/Shaders/OculusHandCursor.shader"),
        ("OculusRayCursor.shader", "Runtime/Shaders/OculusRayCursor.shader")
    };

    static FixMetaInteractionShaders()
    {
        EditorApplication.delayCall += CheckAndApplyFix;
    }

    [MenuItem("Tools/Fix Meta Shaders (URP)")]
    public static void ManualFix()
    {
        bool updated = ApplyFix(true);
        if (updated)
        {
            AssetDatabase.Refresh();
            Debug.Log("[FixMetaInteractionShaders] Шейдери Meta Interaction SDK успішно оновлено для URP!");
        }
        else
        {
            Debug.Log("[FixMetaInteractionShaders] Усі шейдери вже сумісні з URP, оновлення не потрібне.");
        }
    }

    private static void CheckAndApplyFix()
    {
        if (ApplyFix(false))
        {
            AssetDatabase.Refresh();
            Debug.Log("[FixMetaInteractionShaders] Автоматично відновлено URP-шейдери Meta Interaction SDK.");
        }
    }

    private static bool ApplyFix(bool force)
    {
        try
        {
            string packageCacheDir = Path.Combine(Directory.GetCurrentDirectory(), "Library", "PackageCache");
            if (!Directory.Exists(packageCacheDir)) return false;

            string[] matchDirs = Directory.GetDirectories(packageCacheDir, "com.meta.xr.sdk.interaction@*");
            if (matchDirs.Length == 0) return false;

            string targetPkgDir = matchDirs[0];
            string assetsShadersDir = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Shaders");
            if (!Directory.Exists(assetsShadersDir)) return false;

            bool anyCopied = false;

            foreach (var mapping in ShaderMappings)
            {
                string srcPath = Path.Combine(assetsShadersDir, mapping.srcRel);
                string destPath = Path.Combine(targetPkgDir, mapping.destRel);

                if (!File.Exists(srcPath)) continue;

                bool needCopy = force || !File.Exists(destPath);
                if (!needCopy && File.Exists(destPath))
                {
                    string destContent = File.ReadAllText(destPath);
                    if (!destContent.Contains("UniversalPipeline"))
                    {
                        needCopy = true;
                    }
                }

                if (needCopy)
                {
                    string dir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    File.Copy(srcPath, destPath, true);
                    anyCopied = true;
                }
            }

            return anyCopied;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[FixMetaInteractionShaders] Не вдалося автоматично оновити шейдери: {ex.Message}");
            return false;
        }
    }
}
