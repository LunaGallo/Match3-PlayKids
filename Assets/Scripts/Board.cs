using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {

    public static Board instance = null;
    private void Awake() {
        instance = this;
    }

    public RectInt dimensions;
    public Vector3 GetTilePos(Vector2Int tileId) {
        return transform.TransformPoint((Vector2)(tileId + dimensions.min));
    }
    public Vector2Int GetTileId(Vector3 pos) {
        return Vector2Int.RoundToInt((Vector2)transform.InverseTransformPoint(pos) - dimensions.min);
    }
    public IEnumerable<Vector2Int> GetAllTiles() {
        foreach (Vector2Int p in dimensions.allPositionsWithin) {
            yield return p - dimensions.min;
        }
    }

}
