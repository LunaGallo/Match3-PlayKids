using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {

    public static Board instance = null;
    private void Awake() {
        instance = this;
    }
    public static IEnumerable<Vector2Int> GetAllTiles() {
        foreach (Vector2Int p in instance.dimensions.allPositionsWithin) {
            yield return p - instance.dimensions.min;
        }
    }
    public static Gem GetGem(Vector2Int tileId, bool debug = false) {
        if (debug) {
            Debug.Log("  Getting gem at " + tileId);
            Gem.AllLivingInstances.ForEach(g => Debug.Log("    Checking gem at " + g.CurTileId + " = " + (g.CurTileId == tileId)));
        }
        return Gem.AllLivingInstances.Find(g => g.CurTileId == tileId);
    }
    public static Vector3 GetTilePos(Vector2Int tileId) {
        return instance.transform.TransformPoint((Vector2)(tileId + instance.dimensions.min));
    }
    public static Vector2Int GetTileId(Vector3 pos) {
        return Vector2Int.RoundToInt((Vector2)instance.transform.InverseTransformPoint(pos) - instance.dimensions.min);
    }
    public static int Length {
        get {
            return instance.dimensions.width * instance.dimensions.height;
        }
    }

    public RectInt dimensions;
    public Vector2 framingMargin;
    public static Rect FramingBox {
        get {
            Rect result = new Rect() {
                xMin = instance.dimensions.xMin - instance.framingMargin.x - 0.5f,
                yMin = instance.dimensions.yMin - instance.framingMargin.y - 0.5f,
                xMax = instance.dimensions.xMax + instance.framingMargin.x - 0.5f,
                yMax = instance.dimensions.yMax + instance.framingMargin.y - 0.5f
            };
            return FitToAspect(result, Camera.main.aspect);
        }
    }
    private static Rect FitToAspect(Rect rect, float aspect) {
        if (aspect < rect.size.x / rect.size.y) {
            Vector2 center = rect.center;
            rect.size = new Vector2(rect.size.x, rect.size.x / aspect);
            rect.center = center;
        }
        return rect;
    }


}
