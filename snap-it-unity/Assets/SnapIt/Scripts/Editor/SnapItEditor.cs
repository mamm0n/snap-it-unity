using SnapIt.Scripts.Enums;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SnapIt.Scripts.Editor
{
    #region CLASSES

    public class SnapItEditor : EditorWindow
    {
        #region METHODS

        [MenuItem("Tools/SnapIt")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(SnapItEditor));
            window.titleContent = new GUIContent("Snap It");
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SnapIt/Editor/SnapItEditor.uxml");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/SnapIt/Editor/SnapItEditor.uss");
            visualTree.CloneTree(root);
            root.styleSheets.Add(styleSheet);
            
            var snapButton = rootVisualElement.Q<Button>("SnapButton");
            snapButton.clicked += OnSnapButtonClicked;
        }

        private void OnSnapButtonClicked()
        {
            var snapDirection = (RayDirection) rootVisualElement.Q<EnumField>("SnapDirection").value;
            var snapLayers = (LayerMask) rootVisualElement.Q<LayerMaskField>("SnapLayers").value;
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning($"There is no object selected to snap.");
                return;
            }

            if (snapLayers == 0)
            {
                Debug.LogWarning($"There is no layer selected to snap.");
                return;
            }

            foreach (var selectedObject in selectedObjects)
            {
                var rayDirection = snapDirection switch
                {
                    RayDirection.Down => Vector3.down,
                    RayDirection.Up => Vector3.up,
                    RayDirection.Left => Vector3.left,
                    RayDirection.Right => Vector3.right,
                    RayDirection.Forward => Vector3.forward,
                    RayDirection.Backward => Vector3.back,
                    _ => Vector3.zero
                };

                var ray = new Ray(selectedObject.transform.position, rayDirection);
                if (Physics.Raycast(ray, out var hit, Mathf.Infinity, snapLayers))
                {
                    var meshOffset = selectedObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents;
                    var localScale = selectedObject.transform.localScale;
                    meshOffset.x *= rayDirection.x * localScale.x;
                    meshOffset.y *= rayDirection.y * localScale.y;
                    meshOffset.z *= rayDirection.z * localScale.z;
                    selectedObject.transform.position = hit.point - meshOffset;
                }
            }
        }

        #endregion
    }

    #endregion
}