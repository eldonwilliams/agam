using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "MaterialRegistry", menuName = "Scriptable Objects/MaterialRegistry")]
public class MaterialRegistry : ScriptableObject
{
    [FormerlySerializedAs("tiles")] public MaterialBase[] materials;

    public MaterialBase RandomTile()
    {
        int randomIndex = Random.Range(0, materials.Length);
        return materials[randomIndex];
    }
}
