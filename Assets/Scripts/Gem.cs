using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Gem : MonoBehaviour {

    public static List<Gem> instanceList = new List<Gem>();
    private void Awake() {
        instanceList.Add(this);
    }
    private void OnDestroy() {
        instanceList.Remove(this);
    }
    public static List<Gem> AllLivingInstances {
        get {
            return instanceList.FindAll(i => i.isAlive);
        }
    }

    public SpriteRenderer spriteRenderer;
    public int type = 0;
    public float speed = 1f;
    public Vector3 targetPoint;
    public UnityEvent onIdle;
    public UnityEvent onHover;
    public UnityEvent onSelected;

    private bool isAlive = true;

    public bool IsStopped {
        get {
            return transform.position == targetPoint;
        }
    }
    public Vector2Int CurTileId {
        get {
            return Board.GetTileId(transform.position);
        }
    }

    private void Update() {
        if (!IsStopped) {
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);
        }
        if (Board.GetTileId(Camera.main.ScreenToWorldPoint(Input.mousePosition)) == CurTileId) {
            if (Input.GetMouseButton(0)) {
                onSelected.Invoke();
            }
            else {
                onHover.Invoke();
            }
        }
        else {
            onIdle.Invoke();
        }
    }

    public void SetSprite(Sprite sprite) {
        spriteRenderer.sprite = sprite;
    }
    public void Clear() {
        isAlive = false;
        Destroy(gameObject);
    }

}
