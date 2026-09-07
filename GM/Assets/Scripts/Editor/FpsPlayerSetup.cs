using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using GhostMemory.Player;

namespace GhostMemory.Editor
{
    public static class FpsPlayerSetup
    {
        const string PrefabPath = "Assets/Prefabs/Player.prefab";
        const string ActionsPath = "Assets/InputSystem_Actions.inputactions";

        [MenuItem("GHOST-MEMORY/Setup FPS Player")]
        public static void Setup()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");

            EnsureGround();
            GameObject player = EnsurePlayer();
            PrefabUtility.SaveAsPrefabAssetAndConnect(player, PrefabPath, InteractionMode.AutomatedAction);
            EditorSceneManager.MarkSceneDirty(player.scene);
            Selection.activeGameObject = player;
            Debug.Log("FPS Player is ready. Press Play to walk around.");
        }

        static GameObject EnsurePlayer()
        {
            PlayerController existing = Object.FindFirstObjectByType<PlayerController>();
            if (existing != null)
            {
                Wire(existing.gameObject);
                return existing.gameObject;
            }

            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 0.05f, 0f);
            Undo.RegisterCreatedObjectUndo(player, "Create FPS Player");
            Wire(player);
            return player;
        }

        static void Wire(GameObject player)
        {
            CharacterController controller = GetOrAdd<CharacterController>(player);
            controller.height = 1.8f;
            controller.radius = 0.4f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.slopeLimit = 45f;
            controller.stepOffset = 0.35f;
            controller.skinWidth = 0.08f;
            controller.minMoveDistance = 0.001f;

            PlayerInputReader input = GetOrAdd<PlayerInputReader>(player);
            SerializedObject inputSo = new SerializedObject(input);
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
            inputSo.FindProperty("actions").objectReferenceValue = actions;
            inputSo.ApplyModifiedPropertiesWithoutUndo();

            GetOrAdd<PlayerMotor>(player);
            PlayerLook look = GetOrAdd<PlayerLook>(player);
            GetOrAdd<PlayerStatus>(player);
            GetOrAdd<PlayerController>(player);

            Camera cam = AttachCamera(player);
            SerializedObject lookSo = new SerializedObject(look);
            lookSo.FindProperty("cameraPivot").objectReferenceValue = cam.transform;
            lookSo.ApplyModifiedPropertiesWithoutUndo();
        }

        static Camera AttachCamera(GameObject player)
        {
            Camera main = Camera.main;
            if (main == null)
            {
                GameObject cameraObject = new GameObject("PlayerCamera");
                main = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                cameraObject.tag = "MainCamera";
            }

            Transform camTransform = main.transform;
            camTransform.SetParent(player.transform, false);
            camTransform.localPosition = new Vector3(0f, 1.6f, 0f);
            camTransform.localRotation = Quaternion.identity;
            main.nearClipPlane = 0.05f;
            main.fieldOfView = 75f;
            return main;
        }

        static void EnsureGround()
        {
            if (GameObject.Find("Ground") != null)
                return;

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(50f, 1f, 50f);
            Undo.RegisterCreatedObjectUndo(ground, "Create Ground");

            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "Box";
            box.transform.position = new Vector3(4f, 0.5f, 6f);
            Undo.RegisterCreatedObjectUndo(box, "Create Box");
        }

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            return component != null ? component : Undo.AddComponent<T>(go);
        }
    }
}
