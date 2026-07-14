using UnityEditor;
using UnityEngine;
using UnityEngine.U2D.Animation;
using System.Collections.Generic;
using UnityEngine.U2D;
using Unity.Collections;
using UnityEngine.Rendering;
using System.Linq;
using System.Drawing.Drawing2D;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;

public class SpriteBlendShapeWindow : EditorWindow
{
    GameObject previewObject;

    SpriteRenderer spriteRenderer;

    SpriteSkin spriteSkin;

    Transform[] bones;

    string poseName = "New Pose";

    float frameWeight = 100f;

    public Sprite targetSprite;
    private Vector3[] cachedVertices;

    [MenuItem("Tools/Sprite Blend Shape Authoring")]
    static void Open()
    {
        SpriteBlendShapeWindow window = GetWindow<SpriteBlendShapeWindow>();
        GameObject activeGameObject = Selection.activeObject as GameObject;
        if (activeGameObject != null)
        {
            if (activeGameObject.TryGetComponent(out SpriteRenderer renderer))
            {
                window.targetSprite = renderer.sprite;
                window.CreatePreview();
            }
        }
    }

    void OnGUI()
    {
        Sprite prevTarget = targetSprite;

        targetSprite = (Sprite)EditorGUILayout.ObjectField("Target", targetSprite, typeof(Sprite), true);
        if (targetSprite == null)
        {
            if (previewObject != null)
            {
                DestroyImmediate(previewObject);
            }
            return;
        }
        if (prevTarget != targetSprite)
        {
            CreatePreview();
        }

        poseName = EditorGUILayout.TextField("Pose Name", poseName);
        frameWeight = EditorGUILayout.FloatField("Frame Weight", frameWeight);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Blend Shape"))
        {
            GenerateBlendShape();
        }
        if (GUILayout.Button("Reset Bind Pose") && spriteSkin != null)
        {
            spriteSkin.ResetBindPose();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        if (GUILayout.Button("Clear All Blend Shapes (Warning)") &&
        EditorUtility.DisplayDialog("ClearAall Blend Shapes?",
                "This will clear all blend shapes for the sprite!", "Yes", "No"))
        {
            targetSprite.ClearBlendShapes();
        }
    }

    void CreatePreview()
    {
        if (previewObject != null)
        {
            DestroyImmediate(previewObject);
        }
        previewObject = new GameObject("BlendShape Preview");
        spriteRenderer = previewObject.AddComponent<SpriteRenderer>();
        spriteSkin = previewObject.AddComponent<SpriteSkin>();
        spriteRenderer.sprite = targetSprite;
        SpriteBone[] spriteBones = targetSprite.GetBones();
        
        bones = new Transform[spriteBones.Length];

        for (int i = 0; i < spriteBones.Length; i++)
        {
            SpriteBone spriteBone = spriteBones[i];
            Transform boneTransform = new GameObject(spriteBone.name).transform;
            boneTransform.SetParent(previewObject.transform);
            boneTransform.localPosition = spriteBone.position;
            boneTransform.localRotation = spriteBone.rotation;

            bones[i] = boneTransform;
        }

        spriteSkin.SetRootBone(bones[0]);
        spriteSkin.SetBoneTransforms(bones);

        cachedVertices = targetSprite.GetVertexAttribute<Vector3>(VertexAttribute.Position).ToArray();

        Selection.activeObject = previewObject;
    }

    void GenerateBlendShape()
    {
        if (previewObject == null || string.IsNullOrWhiteSpace(poseName) || frameWeight <= 0)
            return;

        Vector3[] deformedVertices = spriteSkin.GetDeformedVertexPositionData().ToArray();

        int blendShapeIndex = targetSprite.GetBlendShapeIndex(poseName);

        if (blendShapeIndex < 0)
        {
            blendShapeIndex = targetSprite.AddBlendShape(poseName);
        }

        List<SpriteBlendShapeVertex> changedVertices = new List<SpriteBlendShapeVertex>();

        for (int i = 0, length = System.Math.Min(cachedVertices.Length, deformedVertices.Length); i < length; i++)
        {
            Vector3 delta = deformedVertices[i] - cachedVertices[i];

            if (delta.sqrMagnitude > 0.000001f)
            {
                changedVertices.Add(new SpriteBlendShapeVertex
                {
                    index = (uint)i,
                    vertex = delta,
                    normal = Vector3.zero,
                    tangent = Vector3.zero
                });
            }
        }
        if (changedVertices.Count == 0)
        {
            Debug.LogWarning("No vertices have changed");
            return;
        }

        NativeArray<SpriteBlendShapeVertex> frame = new NativeArray<SpriteBlendShapeVertex>(changedVertices.Count, Allocator.Temp);

        for (int i = 0; i < changedVertices.Count; i++)
        {
            frame[i] = changedVertices[i];
        }

        targetSprite.AddBlendShapeFrame(blendShapeIndex, frameWeight, frame);
        frame.Dispose();

        AssetDatabase.SaveAssets();
    }

    void OnDisable()
    {
        targetSprite = null;
        spriteSkin = null;

        if (previewObject != null)
        {
            DestroyImmediate(previewObject);
            previewObject = null;
        }
    }

}