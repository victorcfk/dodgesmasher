using UnityEngine;

public static class LayerMaskExtensions
{
    /// <summary>
    /// Check if this layer mask contains the layer.
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool Contains(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
}