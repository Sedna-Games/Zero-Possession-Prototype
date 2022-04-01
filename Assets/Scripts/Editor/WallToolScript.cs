using UnityEditor;
using UnityEngine;

public class WallToolScript : EditorWindow
{
    [SerializeField] private GameObject wallPrefab;


    public GameObject straightWallA;
    public GameObject straightWallB;
    public GameObject convexWallA;
    public GameObject convexWallB;
    public GameObject concaveWallA;
    public GameObject concaveWallB;

    //make this an enum to control the offset and then make the TileVersion a dropdown menu
    public enum TileVersion { VariationA, VariationB };
    public TileVersion tv;
    private float height = 5.0f;
    [SerializeField] private float groundHeight = 0.0f;


    [MenuItem("Tools/Marcus's Wall Tool")]
    static void CreateWallTool()
    {
        EditorWindow.GetWindow<WallToolScript>();
    }

    private async void OnGUI()
    {

        //GUI options
        tv = (TileVersion)EditorGUILayout.EnumPopup("Tile Variation", tv);
        groundHeight = EditorGUILayout.FloatField("Ground Height", groundHeight);
        wallPrefab = (GameObject)EditorGUILayout.ObjectField("Wall Prefab", wallPrefab, typeof(GameObject), false);

        var selection = Selection.gameObjects;

        GameObject wallToSpawn = null;

        if (GUILayout.Button("Spawn"))
        {

            if (wallPrefab != null)
            {
                wallToSpawn = wallPrefab;
            }

            switch (tv)
            {
                case TileVersion.VariationA:
                    height = 5.0f;
                    break;
                case TileVersion.VariationB:
                    height = 10.0f;
                    break;
            }


            for (int i = 0; i < selection.Length; i++)
            {
                if (wallToSpawn == null)
                {
                    //this is here to stop it from throwing an undeclared exception
                    wallPrefab = null;

                    //get the name of the mesh to figure out what kind of piece to spawn
                    string meshName = selection[i].GetComponent<MeshFilter>().sharedMesh.name;


                    //ignore the floor tiles
                    if(meshName.Contains("floortile", System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }

                    else if (meshName.Contains("convex", System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (tv == TileVersion.VariationA)
                        {
                            wallToSpawn = convexWallA;
                        }
                        else if (tv == TileVersion.VariationB)
                        {
                            wallToSpawn = convexWallB;
                        }
                    }

                    else if (meshName.Contains("concave", System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (tv == TileVersion.VariationA)
                        {
                            wallToSpawn = concaveWallA;
                        }
                        else if (tv == TileVersion.VariationB)
                        {
                            wallToSpawn = concaveWallB;
                        }

                    }

                    else if (meshName.Contains("straight", System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (tv == TileVersion.VariationA)
                        {
                            wallToSpawn = straightWallA;
                        }
                        else if (tv == TileVersion.VariationB)
                        {
                            wallToSpawn = straightWallB;
                        }

                    }

                }

                Transform currentSpawnTransform = selection[i].transform;

                Vector3 originalPosition = selection[i].transform.position;
                Quaternion originalRotation = selection[i].transform.rotation;



                while (currentSpawnTransform.position.y > groundHeight)
                {

                    //Spawn in new wall
                    if (currentSpawnTransform.position == originalPosition)
                    {
                        currentSpawnTransform.position = new Vector3(currentSpawnTransform.position.x, currentSpawnTransform.position.y - 5, currentSpawnTransform.position.z);
                    }
                    else
                    {
                        currentSpawnTransform.position = new Vector3(currentSpawnTransform.position.x, currentSpawnTransform.position.y - height, currentSpawnTransform.position.z);
                    }

                    GameObject newObject = Instantiate(wallToSpawn);

                    //Move new wall to proper position
                    newObject.name = selection[i].name;

                    //Position and Rotation
                    newObject.transform.SetPositionAndRotation(currentSpawnTransform.position, currentSpawnTransform.rotation);
                    Vector3 diffVector = new Vector3(currentSpawnTransform.position.x - originalPosition.x, currentSpawnTransform.position.y - originalPosition.y, currentSpawnTransform.position.z - originalPosition.z);

                    //Transform
                    newObject.transform.SetParent(selection[i].transform);
                    newObject.transform.localPosition = diffVector;

                    Undo.RegisterCreatedObjectUndo(newObject, "SpawnWall");

                }

                selection[i].transform.SetPositionAndRotation(originalPosition, originalRotation);
                wallToSpawn = null;

            }

        }

        GUI.enabled = false;
        EditorGUILayout.LabelField("Selection count: " + Selection.objects.Length);
    }

}
