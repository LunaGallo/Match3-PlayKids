using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFraming : MonoBehaviour {

    public Vector2Int defaultSize;

    private void Update() {
        Rect targetFrame = Board.FramingBox;
        transform.localScale = new Vector3(1f/defaultSize.x, 1f/defaultSize.y, 1f) * Board.instance.dimensions.height;
        transform.position = targetFrame.center;
    }

}
