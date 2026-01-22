using UnityEngine;

/**
 * The container for data associated with a Tile
 */
public class TileData
{
    public float Health;
    public MaterialBase Material;

    public TileData(MaterialBase mat)
    {
        Health = mat.defaultHealth;
        Material = mat;
    }
}
