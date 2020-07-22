using UnityEngine;

public class CameraController : MonoBehaviour {

    public Vector3 cameraOffset;

    private void Update() {
        Rect targetFrame = Board.FramingBox;
        Camera.main.orthographicSize = targetFrame.height / 2f;
        Camera.main.transform.position = (Vector3)targetFrame.center + cameraOffset;
    }

}
