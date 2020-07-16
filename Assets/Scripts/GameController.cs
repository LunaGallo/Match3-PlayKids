using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour {

    public List<Gem> gemPrefabs;
    public UnityEvent OnSelect;
    public UnityEvent OnSwap;
    public UnityEvent OnClear;

    private List<List<Vector2Int>> possibleMatchSetups = new List<List<Vector2Int>>() {
        new List<Vector2Int>() {
            new Vector2Int(1,0),
            new Vector2Int(2,1)
        },
        new List<Vector2Int>() {
            new Vector2Int(1,0),
            new Vector2Int(2,-1)
        },
        new List<Vector2Int>() {
            new Vector2Int(1,1),
            new Vector2Int(2,0)
        },
        new List<Vector2Int>() {
            new Vector2Int(1,-1),
            new Vector2Int(2,0)
        }
    };

    public bool AreAllGemsStopped() {
        return Gem.instanceList.TrueForAll(g => g.IsStopped);
    }
    public bool IsAnyMatchPossible() {
        foreach (Vector2Int tile in Board.instance.GetAllTiles()) {
            //check every possible setup with all rotations
        }
        return false;
    }
    public void ShuffleGems() {
        //just set a new targetPoint for each
    }

}
