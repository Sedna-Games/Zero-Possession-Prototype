using UnityEditor;
using UnityEngine;

public class WallToolScript : EditorWindow
{
    [SerializeField] private GameObject wallPrefab;

    //make this an enum to control the offset and then make the tileSize a dropdown menu
    public enum TileSize { small = 5, big = 10 };
    public TileSize ts;
    [SerializeField] private float height = 5.0f;
    [SerializeField] private float groundHeight = 0.0f;


    [MenuItem("Tools/Marcus's Wall Tool")]
    static void CreateWallTool()
    {
        EditorWindow.GetWindow<WallToolScript>();
    }

    private async void OnGUI()
    {

        //GUI options
        ts = (TileSize)EditorGUILayout.EnumPopup("Tile Size:", ts);
        groundHeight = EditorGUILayout.FloatField("Ground Height", groundHeight);
        wallPrefab = (GameObject)EditorGUILayout.ObjectField("Wall Prefab", wallPrefab, typeof(GameObject), false);

        var selection = Selection.gameObjects;

        if (GUILayout.Button("Spawn"))
        {

            switch (ts)
            {
                case TileSize.small:
                    height = 5.0f;
                    break;
                case TileSize.big:
                    height = 10.0f;
                    break;
            }


            for (int i = 0; i < selection.Length; i++)
            {

                Transform currentSpawnTransform = selection[i].transform;

                Vector3 originalPosition = selection[i].transform.position;
                Quaternion originalRotation = selection[i].transform.rotation;

                while (currentSpawnTransform.position.y > groundHeight)
                {

                    //Spawn in new wall
                    if (currentSpawnTransform == selection[i].transform)
                    {
                        currentSpawnTransform.position = new Vector3(currentSpawnTransform.position.x, currentSpawnTransform.position.y - 5, currentSpawnTransform.position.z);
                    }
                    else
                    {
                        currentSpawnTransform.position = new Vector3(currentSpawnTransform.position.x, currentSpawnTransform.position.y - height, currentSpawnTransform.position.z);
                    }

                    GameObject newObject = Instantiate(wallPrefab);

                    //Move new wall to proper position
                    newObject.name = wallPrefab.name;

                    //Position and Rotation
                    newObject.transform.SetPositionAndRotation(currentSpawnTransform.position, currentSpawnTransform.rotation);
                    Vector3 diffVector = new Vector3(currentSpawnTransform.position.x - originalPosition.x, currentSpawnTransform.position.y - originalPosition.y, currentSpawnTransform.position.z - originalPosition.z);

                    //Transform
                    newObject.transform.SetParent(selection[i].transform);
                    newObject.transform.localPosition = diffVector;

                    Undo.RegisterCreatedObjectUndo(newObject, "SpawnWall");

                }

                selection[i].transform.SetPositionAndRotation(originalPosition, originalRotation);

            }

        }

        GUI.enabled = false;
        EditorGUILayout.LabelField("Selection count: " + Selection.objects.Length);
    }

}
