using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;

namespace Editor
{
    [CustomEditor(typeof(AttackDataSO))]
    public class AttackDataEditor : UnityEditor.Editor
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
            if (currentFrame == null) return;

            // 1. DRAW HITBOXES (RED)
            if (currentFrame.Hitboxes != null)
            {
                for (int i = 0; i < currentFrame.Hitboxes.Count; i++)
                {
                    var hb = currentFrame.Hitboxes[i];
                    Vector3 worldCenter = pivot + (Vector3)hb.Center;

                    Handles.color = new Color(1f, 0f, 0f, 0.3f);
                    Handles.DrawSolidDisc(worldCenter, Vector3.forward, hb.Radius);
                    Handles.color = Color.white;
                    Handles.DrawWireDisc(worldCenter, Vector3.forward, hb.Radius);

                    GUIStyle labelStyle = new GUIStyle();
                    labelStyle.normal.textColor = Color.white;
                    labelStyle.alignment = TextAnchor.MiddleCenter;
                    Handles.Label(worldCenter, $"ID: {hb.Id}", labelStyle);

                    if (!Application.isPlaying)
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 newPos = Handles.FreeMoveHandle(worldCenter, 0.1f, Vector3.zero, Handles.RectangleHandleCap);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(attackData, "Move Hitbox");
                            hb.Center = newPos - pivot;
                            EditorUtility.SetDirty(attackData);
                        }

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

            // 2. DRAW HURTBOXES (CAPSULES)
            if (attackData.OverrideHurtboxes && currentFrame.Hurtboxes != null)
            {
                for (int i = 0; i < currentFrame.Hurtboxes.Count; i++)
                {
                    var hurtbox = currentFrame.Hurtboxes[i];
                    Vector3 worldCenter = pivot + (Vector3)hurtbox.Center;

                    Color hurtboxColor = Color.yellow;
                    if (hurtbox.State == HurtboxState.Invincible) hurtboxColor = Color.cyan;
                    else if (hurtbox.State == HurtboxState.Intangible) hurtboxColor = Color.blue;

                    // --- Draw Capsule Visuals ---
                    Matrix4x4 oldMatrix = Handles.matrix;
                    Handles.matrix = Matrix4x4.TRS(worldCenter, Quaternion.Euler(0, 0, hurtbox.Angle), Vector3.one);

                    // Math to construct a capsule out of circles and a rect
                    float radius = Mathf.Max(0.01f, hurtbox.Size.x / 2f);
                    float offset = Mathf.Max(0, (hurtbox.Size.y / 2f) - radius);
                    Vector3 topCenter = new Vector3(0, offset, 0);
                    Vector3 bottomCenter = new Vector3(0, -offset, 0);

                    // Solid Fill
                    Handles.color = new Color(hurtboxColor.r, hurtboxColor.g, hurtboxColor.b, 0.3f);
                    Handles.DrawSolidDisc(topCenter, Vector3.forward, radius);
                    Handles.DrawSolidDisc(bottomCenter, Vector3.forward, radius);
                    if (offset > 0)
                    {
                        Handles.DrawSolidRectangleWithOutline(new Rect(-radius, -offset, radius * 2, offset * 2), Handles.color, Color.clear);
                    }

                    // Wire Outline
                    Handles.color = hurtboxColor;
                    Handles.DrawWireArc(topCenter, Vector3.forward, Vector3.right, 180, radius);
                    Handles.DrawWireArc(bottomCenter, Vector3.forward, Vector3.left, 180, radius);
                    if (offset > 0)
                    {
                        Handles.DrawLine(topCenter + Vector3.left * radius, bottomCenter + Vector3.left * radius);
                        Handles.DrawLine(topCenter + Vector3.right * radius, bottomCenter + Vector3.right * radius);
                    }

                    Handles.matrix = oldMatrix; // Reset matrix for handles to work in world space

                    // Label
                    GUIStyle labelStyle = new GUIStyle();
                    labelStyle.normal.textColor = hurtboxColor;
                    labelStyle.alignment = TextAnchor.MiddleCenter;
                    Handles.Label(worldCenter + new Vector3(0, hurtbox.Size.y / 2 + 0.3f, 0), $"{hurtbox.Type}\n({hurtbox.State})", labelStyle);

                    // --- Interaction Handles ---
                    if (!Application.isPlaying)
                    {
                        Quaternion rotation = Quaternion.Euler(0, 0, hurtbox.Angle);

                        // 1. Move Handle (Center Square)
                        EditorGUI.BeginChangeCheck();
                        Vector3 newPos = Handles.FreeMoveHandle(worldCenter, 0.05f, Vector3.zero, Handles.RectangleHandleCap);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(attackData, "Move Hurtbox");
                            hurtbox.Center = newPos - pivot;
                            EditorUtility.SetDirty(attackData);
                        }

                        // 2. Rotation Handle (Top Lever with Sphere)
                        Vector3 rotHandlePos = worldCenter + (rotation * Vector3.up * (hurtbox.Size.y / 2f + 0.4f));
                        Handles.color = Color.white;
                        Handles.DrawLine(worldCenter, rotHandlePos); // Draw the lever arm
                        EditorGUI.BeginChangeCheck();
                        Vector3 newRotPos = Handles.FreeMoveHandle(rotHandlePos, 0.05f, Vector3.zero, Handles.SphereHandleCap);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(attackData, "Rotate Hurtbox");
                            Vector2 dir = newRotPos - worldCenter;
                            hurtbox.Angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                            EditorUtility.SetDirty(attackData);
                        }

                        // 3. Size Handles (Green Cubes)
                        Handles.color = Color.green;

                        // Width Handle (Right Side)
                        Vector3 widthHandlePos = worldCenter + (rotation * Vector3.right * (hurtbox.Size.x / 2f));
                        EditorGUI.BeginChangeCheck();
                        Vector3 newWidthPos = Handles.FreeMoveHandle(widthHandlePos, 0.05f, Vector3.zero, Handles.CubeHandleCap);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(attackData, "Resize Hurtbox Width");
                            float newWidth = Vector3.Distance(worldCenter, newWidthPos) * 2f;
                            hurtbox.Size = new Vector2(newWidth, hurtbox.Size.y);
                            EditorUtility.SetDirty(attackData);
                        }

                        // Height Handle (Top Side, below rotation lever)
                        Vector3 heightHandlePos = worldCenter + (rotation * Vector3.up * (hurtbox.Size.y / 2f));
                        EditorGUI.BeginChangeCheck();
                        Vector3 newHeightPos = Handles.FreeMoveHandle(heightHandlePos, 0.05f, Vector3.zero, Handles.CubeHandleCap);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(attackData, "Resize Hurtbox Height");
                            float newHeight = Vector3.Distance(worldCenter, newHeightPos) * 2f;
                            hurtbox.Size = new Vector2(hurtbox.Size.x, newHeight);
                            EditorUtility.SetDirty(attackData);
                        }
                    }
                }
            }

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
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AttackDataSO.Type)));

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AttackDataSO.AnimationClip)));

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AttackDataSO.FrameCount)));

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AttackDataSO.OverrideHurtboxes)));
            
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

            AttackFrame currentFrame = attackData.Frames.FirstOrDefault(f => f.frameIndex == selectedFrameIndex);


            if (currentFrame != null)
            {
                DrawHitboxEditor(currentFrame);

                if (attackData.OverrideHurtboxes)
                {
                    DrawHurtboxEditor(currentFrame);
                }
            }
            else
            {
                EditorGUILayout.HelpBox($"No Data for Frame {selectedFrameIndex}", MessageType.Info);
                return;
            }

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
            int maxFrames = Mathf.Max(attackData.TotalAnimationFrames, 1);
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

            AttackFrame data = attackData.Frames.FirstOrDefault(f => f.frameIndex == selectedFrameIndex - 1);
            
            GUI.enabled = data != null && attackData.Frames.FirstOrDefault(f => f.frameIndex == selectedFrameIndex) == null;

            if (GUILayout.Button("Copy previous frame"))
            {
                Undo.RecordObject(attackData, "Copy Frame");

                AttackFrame copyData = JsonUtility.FromJson<AttackFrame>(JsonUtility.ToJson(data));

                copyData.frameIndex = selectedFrameIndex;

                attackData.Frames.Add(copyData);
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawHitboxEditor(AttackFrame currentFrame)
        {
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
                    hb.Id = EditorGUILayout.IntField("ID", hb.Id);

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
                    //EditorGUILayout.Space(2);
                    //hb.Center = EditorGUILayout.Vector2Field("Offset", hb.Center);
                    //hb.Radius = EditorGUILayout.FloatField("Radius", hb.Radius);

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space(4); // Gap between boxes
                }
            }
        }

        private void DrawHurtboxEditor(AttackFrame currentFrame)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Hurtbox Overrides ({currentFrame.Hurtboxes.Count})", EditorStyles.boldLabel);

            if (GUILayout.Button("+ Add Hurtbox Override", GUILayout.Height(30)))
            {
                Undo.RecordObject(attackData, "Add Hurtbox Override");
                if (currentFrame.Hurtboxes == null) currentFrame.Hurtboxes = new List<HurtboxOverrideData>();
                currentFrame.Hurtboxes.Add(new HurtboxOverrideData { Type = HurtboxType.Torso, State = HurtboxState.Normal, Center = Vector2.zero, Size = new Vector2(0.5f, 1f), Angle = 0f });
            }

            EditorGUILayout.Space();

            if (currentFrame.Hurtboxes != null)
            {
                for (int i = 0; i < currentFrame.Hurtboxes.Count; i++)
                {
                    var hb = currentFrame.Hurtboxes[i];
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 80;
                    hb.Type = (HurtboxType)EditorGUILayout.EnumPopup("Type", hb.Type);

                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    {
                        Undo.RecordObject(attackData, "Delete Hurtbox Override");
                        currentFrame.Hurtboxes.RemoveAt(i);
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    }
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(2);
                    hb.State = (HurtboxState)EditorGUILayout.EnumPopup("State", hb.State);
                    hb.Center = EditorGUILayout.Vector2Field("Offset", hb.Center);
                    hb.Size = EditorGUILayout.Vector2Field("Size", hb.Size);
                    hb.Angle = EditorGUILayout.FloatField("Angle", hb.Angle);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(4);
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

}