using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(AttackDataSO))]
public class AttackDataEditor : Editor
{
    private AttackDataSO attackData;
    private int selectedFrameIndex = 0;
    private GameObject previewCharacter;
    private bool isPreviewing = false;

    private void OnEnable()
    {
        attackData = (AttackDataSO)target;

        if (attackData.Frames == null)
            attackData.Frames = new List<AttackFrame>();

        if (previewCharacter == null)
            previewCharacter = GameObject.FindWithTag("Player");

        // Hook into Scene View for drawing handles even when not selected
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;

        if (isPreviewing)
        {
            AnimationMode.StopAnimationMode();
            isPreviewing = false;
        }
    }

    // --- SCENE VIEW LOGIC ---
    private void OnSceneGUI(SceneView sceneView)
    {
        if (attackData == null || attackData.Frames == null) return;

        Vector3 pivot = previewCharacter != null ? previewCharacter.transform.position : Vector3.zero;

        // Force visibility through objects
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

        // Draw Pivot Anchor
        Handles.color = Color.cyan;
        Handles.DrawWireDisc(pivot, Vector3.forward, 0.25f);

        // Get Current Frame Data
        var currentFrame = attackData.Frames.FirstOrDefault(f => f.frameIndex == selectedFrameIndex);

        if (currentFrame != null && currentFrame.Hitboxes != null)
        {
            for (int i = 0; i < currentFrame.Hitboxes.Count; i++)
            {
                var hb = currentFrame.Hitboxes[i];
                Vector3 worldCenter = pivot + (Vector3)hb.Center;

                // Draw Visuals
                Handles.color = new Color(1f, 0f, 0f, 0.3f); // Semi-transparent Red
                Handles.DrawSolidDisc(worldCenter, Vector3.forward, hb.Radius);
                Handles.color = Color.white;
                Handles.DrawWireDisc(worldCenter, Vector3.forward, hb.Radius);

                // Draw ID Label in Scene
                GUIStyle labelStyle = new GUIStyle();
                labelStyle.normal.textColor = Color.white;
                labelStyle.alignment = TextAnchor.MiddleCenter;
                Handles.Label(worldCenter, $"ID: {hb.Id}", labelStyle);

                // Interaction Handles (Only if not playing)
                if (!Application.isPlaying)
                {
                    // Move Handle
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPos = Handles.FreeMoveHandle(worldCenter, 0.1f, Vector3.zero, Handles.RectangleHandleCap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(attackData, "Move Hitbox");
                        hb.Center = newPos - pivot;
                        EditorUtility.SetDirty(attackData);
                    }

                    // Radius Handle
                    EditorGUI.BeginChangeCheck();
                    float newRadius = Handles.RadiusHandle(Quaternion.identity, worldCenter, hb.Radius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(attackData, "Resize Hitbox");
                        hb.Radius = newRadius;
                        EditorUtility.SetDirty(attackData);
                    }
                }
            }
        }

        // Force repaint for smooth dragging
        if (Event.current.type == EventType.Repaint)
        {
            sceneView.Repaint();
        }
    }

    // --- INSPECTOR GUI ---
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 1. Header & Configuration
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Attack Configuration", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Type"));

        // Animation Clip Field with Auto-Duration Logic
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimationClip"));
        if (EditorGUI.EndChangeCheck())
        {
            // Auto-set duration if animation is assigned
            if (attackData.AnimationClip != null)
            {
                float frameRate = attackData.AnimationClip.frameRate > 0 ? attackData.AnimationClip.frameRate : 60;
                attackData.TotalDurationFrames = Mathf.CeilToInt(attackData.AnimationClip.length * frameRate);
            }
        }

        // Allow manual override of duration
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TotalDurationFrames"));
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 2. Preview Settings
        EditorGUILayout.LabelField("Preview Settings", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        previewCharacter = (GameObject)EditorGUILayout.ObjectField("Character", previewCharacter, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck())
        {
            AnimationMode.StopAnimationMode();
            isPreviewing = false;
        }

        EditorGUILayout.Space();

        // 3. Timeline Controls
        DrawTimelineControls();

        // 4. Hitbox Editor (The Main Area)
        DrawHitboxEditor();

        if (GUI.changed) EditorUtility.SetDirty(attackData);
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTimelineControls()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Timeline", EditorStyles.boldLabel);

        // Step Buttons Row
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("<< Prev", GUILayout.Height(25)))
        {
            selectedFrameIndex = Mathf.Max(0, selectedFrameIndex - 1);
            UpdatePreview();
        }

        // The Slider
        int maxFrames = Mathf.Max(attackData.TotalDurationFrames, 1);
        EditorGUI.BeginChangeCheck();
        selectedFrameIndex = EditorGUILayout.IntSlider(selectedFrameIndex, 0, maxFrames);
        if (EditorGUI.EndChangeCheck())
        {
            UpdatePreview();
        }

        if (GUILayout.Button("Next >>", GUILayout.Height(25)))
        {
            selectedFrameIndex = Mathf.Min(maxFrames, selectedFrameIndex + 1);
            UpdatePreview();
        }
        EditorGUILayout.EndHorizontal();

        // Frame Data Management
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add/Edit Frame Data"))
        {
            Undo.RecordObject(attackData, "Add Frame");
            if (attackData.Frames.FirstOrDefault(f => f.frameIndex == selectedFrameIndex) == null)
                attackData.Frames.Add(new AttackFrame { frameIndex = selectedFrameIndex });
        }

        // Only show Delete if data exists
        bool hasData = attackData.Frames.Any(f => f.frameIndex == selectedFrameIndex);
        GUI.enabled = hasData;
        if (GUILayout.Button("Delete Frame Data"))
        {
            var f = attackData.Frames.FirstOrDefault(f => f.frameIndex == selectedFrameIndex);
            if (f != null)
            {
                Undo.RecordObject(attackData, "Delete Frame");
                attackData.Frames.Remove(f);
            }
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawHitboxEditor()
    {
        var currentFrame = attackData.Frames.FirstOrDefault(f => f.frameIndex == selectedFrameIndex);

        if (currentFrame == null)
        {
            EditorGUILayout.HelpBox($"No Data for Frame {selectedFrameIndex}", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Active Hitboxes ({currentFrame.Hitboxes.Count})", EditorStyles.boldLabel);

        // "Add Hitbox" Button
        if (GUILayout.Button("+ Add New Hitbox", GUILayout.Height(30)))
        {
            Undo.RecordObject(attackData, "Add Hitbox");
            
            if (currentFrame.Hitboxes == null) 
                currentFrame.Hitboxes = new List<HitboxData>();

            currentFrame.Hitboxes.Add(new HitboxData { Id = 0, Center = Vector2.zero, Radius = 0.5f });
        }

        EditorGUILayout.Space();

        // List Hitboxes using a nicer "Card" style
        if (currentFrame.Hitboxes != null)
        {
            for (int i = 0; i < currentFrame.Hitboxes.Count; i++)
            {
                var hb = currentFrame.Hitboxes[i];

                // Draw a box around each hitbox entry
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Header Row: ID + Delete Button
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 60; // Make labels smaller to fit
                hb.Id = EditorGUILayout.IntField("ID / Type", hb.Id);

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    Undo.RecordObject(attackData, "Delete Hitbox");
                    currentFrame.Hitboxes.RemoveAt(i);
                    GUI.backgroundColor = Color.white; // Reset color
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break; // Exit loop to avoid error
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                // Properties Row
                EditorGUILayout.Space(2);
                hb.Center = EditorGUILayout.Vector2Field("Offset", hb.Center);
                hb.Radius = EditorGUILayout.FloatField("Radius", hb.Radius);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4); // Gap between boxes
            }
        }
    }

    private void UpdatePreview()
    {
        if (previewCharacter == null || attackData.AnimationClip == null) return;
        if (!AnimationMode.InAnimationMode()) AnimationMode.StartAnimationMode();

        float t = attackData.AnimationClip.frameRate > 0 ? selectedFrameIndex / attackData.AnimationClip.frameRate : 0;
        AnimationMode.SampleAnimationClip(previewCharacter, attackData.AnimationClip, t);
        SceneView.RepaintAll();
    }
}