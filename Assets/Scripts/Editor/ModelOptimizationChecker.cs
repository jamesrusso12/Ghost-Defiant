using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor tool to check and optimize 3D model import settings for VR performance.
/// Menu: Tools > VR Optimization > Check Model Performance
/// </summary>
public class ModelOptimizationChecker : EditorWindow
{
    private Vector2 scrollPosition;
    private List<ModelReport> reports = new List<ModelReport>();
    private bool showOnlyIssues = true;
    private int totalVertices = 0;
    private int totalTriangles = 0;
    
    private class ModelReport
    {
        public string path;
        public string name;
        public int vertices;
        public int triangles;
        public bool hasIssues;
        public List<string> issues = new List<string>();
        public List<string> recommendations = new List<string>();
        public ModelImporter importer;
        public Mesh mesh;
    }
    
    [MenuItem("Tools/VR Optimization/Check Model Performance")]
    public static void ShowWindow()
    {
        var window = GetWindow<ModelOptimizationChecker>("Model Optimizer");
        window.minSize = new Vector2(600, 400);
        window.Show();
    }
    
    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("VR Model Performance Checker", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool checks 3D models for VR performance issues and provides optimization recommendations.\n" +
            "Ideal for VR: < 10,000 vertices per model, optimized textures, proper compression.",
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Scan All Models in Project", GUILayout.Height(30)))
        {
            ScanAllModels();
        }
        
        if (GUILayout.Button("Scan Weapons Folder Only", GUILayout.Height(30)))
        {
            ScanWeaponsModels();
        }
        
        EditorGUILayout.Space();
        showOnlyIssues = EditorGUILayout.Toggle("Show Only Models with Issues", showOnlyIssues);
        EditorGUILayout.Space();
        
        if (reports.Count > 0)
        {
            EditorGUILayout.LabelField($"Total Models: {reports.Count} | Total Vertices: {totalVertices:N0} | Total Triangles: {totalTriangles:N0}", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var report in reports)
            {
                if (showOnlyIssues && !report.hasIssues)
                    continue;
                    
                DrawModelReport(report);
            }
            
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("Click 'Scan' to analyze models in your project.", MessageType.Info);
        }
    }
    
    void DrawModelReport(ModelReport report)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Header
        var headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 12;
        if (report.hasIssues)
        {
            headerStyle.normal.textColor = new Color(1f, 0.5f, 0f); // Orange
        }
        
        EditorGUILayout.LabelField($"📦 {report.name}", headerStyle);
        EditorGUILayout.LabelField($"Path: {report.path}", EditorStyles.miniLabel);
        
        // Stats
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Vertices: {report.vertices:N0}", GUILayout.Width(150));
        EditorGUILayout.LabelField($"Triangles: {report.triangles:N0}", GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();
        
        // Issues
        if (report.issues.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("⚠️ Issues:", EditorStyles.boldLabel);
            foreach (var issue in report.issues)
            {
                EditorGUILayout.LabelField($"  • {issue}", EditorStyles.wordWrappedLabel);
            }
        }
        
        // Recommendations
        if (report.recommendations.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("💡 Recommendations:", EditorStyles.boldLabel);
            foreach (var rec in report.recommendations)
            {
                EditorGUILayout.LabelField($"  • {rec}", EditorStyles.wordWrappedLabel);
            }
        }
        
        // Actions
        if (report.hasIssues)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Auto-Optimize", GUILayout.Height(25)))
            {
                AutoOptimizeModel(report);
            }
            
            if (GUILayout.Button("Select in Project", GUILayout.Height(25)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(report.path);
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    void ScanAllModels()
    {
        reports.Clear();
        totalVertices = 0;
        totalTriangles = 0;
        
        string[] modelGuids = AssetDatabase.FindAssets("t:Model");
        
        foreach (string guid in modelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnalyzeModel(path);
        }
        
        // Sort by vertex count (highest first)
        reports = reports.OrderByDescending(r => r.vertices).ToList();
    }
    
    void ScanWeaponsModels()
    {
        reports.Clear();
        totalVertices = 0;
        totalTriangles = 0;
        
        string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { "Assets/Prefabs/Weapons" });
        
        foreach (string guid in modelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnalyzeModel(path);
        }
        
        // Sort by vertex count (highest first)
        reports = reports.OrderByDescending(r => r.vertices).ToList();
    }
    
    void AnalyzeModel(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null) return;
        
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) return;
        
        var report = new ModelReport
        {
            path = path,
            name = asset.name,
            importer = importer
        };
        
        // Get mesh stats
        var meshFilters = asset.GetComponentsInChildren<MeshFilter>(true);
        var skinnedMeshRenderers = asset.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        
        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh != null)
            {
                report.vertices += mf.sharedMesh.vertexCount;
                report.triangles += mf.sharedMesh.triangles.Length / 3;
                report.mesh = mf.sharedMesh;
            }
        }
        
        foreach (var smr in skinnedMeshRenderers)
        {
            if (smr.sharedMesh != null)
            {
                report.vertices += smr.sharedMesh.vertexCount;
                report.triangles += smr.sharedMesh.triangles.Length / 3;
                report.mesh = smr.sharedMesh;
            }
        }
        
        totalVertices += report.vertices;
        totalTriangles += report.triangles;
        
        // Analyze for issues
        AnalyzeIssues(report, importer);
        
        reports.Add(report);
    }
    
    void AnalyzeIssues(ModelReport report, ModelImporter importer)
    {
        // Check vertex count (VR-specific threshold)
        if (report.vertices > 50000)
        {
            report.hasIssues = true;
            report.issues.Add($"CRITICAL: Very high vertex count ({report.vertices:N0})! This will severely impact VR performance.");
            report.recommendations.Add("Consider using a simplified/decimated version of this model (< 10,000 vertices for VR)");
        }
        else if (report.vertices > 20000)
        {
            report.hasIssues = true;
            report.issues.Add($"HIGH: High vertex count ({report.vertices:N0}) may cause performance issues in VR.");
            report.recommendations.Add("Reduce polygon count to < 10,000 vertices if possible");
        }
        else if (report.vertices > 10000)
        {
            report.hasIssues = true;
            report.issues.Add($"MEDIUM: Vertex count ({report.vertices:N0}) is above VR recommended threshold.");
            report.recommendations.Add("Optimize mesh if possible (target: 5,000-10,000 vertices for VR weapons)");
        }
        
        // Check mesh compression
        if (importer.meshCompression == ModelImporterMeshCompression.Off)
        {
            report.hasIssues = true;
            report.issues.Add("Mesh compression is OFF");
            report.recommendations.Add("Enable mesh compression (Medium or High) to reduce memory usage");
        }
        
        // Check read/write enabled
        if (importer.isReadable)
        {
            report.hasIssues = true;
            report.issues.Add("Read/Write is ENABLED");
            report.recommendations.Add("Disable Read/Write to reduce memory usage (uses 2x memory when enabled)");
        }
        
        // Check if normals/tangents are being calculated
        if (importer.importNormals == ModelImporterNormals.Calculate)
        {
            report.recommendations.Add("Consider importing normals from model instead of calculating them");
        }
        
        // Check if blend shapes are imported (when not needed)
        if (importer.importBlendShapes)
        {
            report.recommendations.Add("Disable blend shapes if not needed (saves memory)");
        }
        
        // Check animation import (if no animations needed)
        if (importer.importAnimation)
        {
            report.recommendations.Add("Disable animation import if this model doesn't need animations");
        }
        
        // Check LOD (for VR, LODs are very important)
        if (!importer.importBlendShapes && report.vertices > 5000)
        {
            report.recommendations.Add("Consider creating LOD (Level of Detail) groups for this model");
        }
    }
    
    void AutoOptimizeModel(ModelReport report)
    {
        if (report.importer == null) return;
        
        bool madeChanges = false;
        
        // Enable mesh compression
        if (report.importer.meshCompression == ModelImporterMeshCompression.Off)
        {
            report.importer.meshCompression = ModelImporterMeshCompression.High;
            madeChanges = true;
            Debug.Log($"[Optimizer] Enabled HIGH mesh compression for {report.name}");
        }
        
        // Disable read/write
        if (report.importer.isReadable)
        {
            report.importer.isReadable = false;
            madeChanges = true;
            Debug.Log($"[Optimizer] Disabled Read/Write for {report.name}");
        }
        
        // Optimize normals
        if (report.importer.importNormals == ModelImporterNormals.Calculate)
        {
            report.importer.importNormals = ModelImporterNormals.Import;
            madeChanges = true;
            Debug.Log($"[Optimizer] Changed to import normals for {report.name}");
        }
        
        // Disable blend shapes if enabled
        if (report.importer.importBlendShapes)
        {
            report.importer.importBlendShapes = false;
            madeChanges = true;
            Debug.Log($"[Optimizer] Disabled blend shapes for {report.name}");
        }
        
        // Disable animation if enabled
        if (report.importer.importAnimation)
        {
            report.importer.importAnimation = false;
            madeChanges = true;
            Debug.Log($"[Optimizer] Disabled animation import for {report.name}");
        }
        
        // Optimize mesh settings
        report.importer.optimizeMeshPolygons = true;
        report.importer.optimizeMeshVertices = true;
        report.importer.weldVertices = true;
        madeChanges = true;
        
        if (madeChanges)
        {
            AssetDatabase.ImportAsset(report.path, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[Optimizer] ✓ Optimized {report.name}. Re-scan to see improvements.");
            
            EditorUtility.DisplayDialog(
                "Optimization Complete",
                $"Model '{report.name}' has been optimized!\n\n" +
                "Changes made:\n" +
                "• Enabled mesh compression\n" +
                "• Disabled Read/Write\n" +
                "• Optimized normals/vertices\n" +
                "• Disabled unused features\n\n" +
                "Click 'Scan' again to see the improvements.",
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Already Optimized",
                $"Model '{report.name}' is already optimized for import settings!\n\n" +
                "If performance is still an issue, you may need to:\n" +
                "• Reduce polygon count in your 3D modeling software\n" +
                "• Use a simplified/decimated mesh\n" +
                "• Create LOD (Level of Detail) groups",
                "OK"
            );
        }
    }
}
