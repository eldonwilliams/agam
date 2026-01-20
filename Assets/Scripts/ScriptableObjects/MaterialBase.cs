using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "MaterialBase", menuName = "Scriptable Objects/MaterialBase")]
public class MaterialBase : ScriptableObject
{
    public string materialName;
    public float defaultHealth;
    public TileBase tile;
    public float value = 1f;
}
