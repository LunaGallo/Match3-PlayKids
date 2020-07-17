using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteColorSetter : MonoBehaviour {

    private SpriteRenderer spriteRenderer;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetBrightness(float newBrightness) {
        Color.RGBToHSV(spriteRenderer.color, out float h, out float s, out _);
        spriteRenderer.color = Color.HSVToRGB(h,s,newBrightness);
    }

}
