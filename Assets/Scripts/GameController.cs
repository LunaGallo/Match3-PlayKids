using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public static int curGoalScore = 0;
    public static int curLevel = 0;
    public static bool lastGameWon = false;

    public int baseMatchScore = 100;
    public int bonusMatchScore = 100;
    public int comboBonusScore = 100;
    public int baseGoalScore = 1000;
    public int incGoalScore = 500;
    public float levelDuration = 120f;
    public Gem gemPrefab;
    public List<Sprite> gemSprites;
    public Text timerText;
    public UnityEvent onSelect;
    public UnityEvent onSwap;
    public UnityEvent onClear;
    public UnityEvent onLost;
    public UnityEvent onWon;

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
            new Vector2Int(1,0),
            new Vector2Int(3,0)
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
    private bool isActionHappening = false;
    private Vector2Int selectedTile;
    private Vector2Int swappedTile;
    private bool isSelected = false;
    private float curTimer = 0f;
    private int curScore = 0;
    private int comboCounter = 0;
    private bool gameEnded = false;

    private void Start() {
        FillBoardWithGems();
        curTimer = levelDuration;
        if (lastGameWon) {
            curLevel++;
        }
        curGoalScore = baseGoalScore + (curLevel * incGoalScore);
    }
    private void Update() {
        if (!gameEnded) {
            if (AreAllGemsStopped()) {
                if (IsAnyMatchMade()) {
                    ClearMadeMatches();
                    FillBoardWithGems(/*true*/);
                    isActionHappening = false;
                    onClear.Invoke();
                }
                else if (isActionHappening) {
                    SwapGems();
                    isActionHappening = false;
                }
                else if (!IsAnyMatchPossible()) { 
                    ShuffleGems();
                }
                else if (isSelected) {
                    if (Input.GetMouseButtonUp(0)) {
                        isSelected = false;
                    }
                    else {
                        Vector2Int hoveredTile = Board.GetTileId(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        Gem hoveredGem = Board.GetGem(hoveredTile);
                        if (hoveredGem != null && (hoveredTile - selectedTile).sqrMagnitude == 1f) {
                            swappedTile = hoveredTile;
                            SwapGems();
                            isActionHappening = true;
                            isSelected = false;
                            comboCounter = 0;
                            onSwap.Invoke();
                        }
                    }
                }
                else {
                    if (Input.GetMouseButtonDown(0)) {
                        Vector2Int hoveredTile = Board.GetTileId(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        Gem hoveredGem = Board.GetGem(hoveredTile);
                        if (hoveredGem != null) {
                            isSelected = true;
                            selectedTile = hoveredTile;
                            onSelect.Invoke();
                        }
                    }
                }
            }
            UpdateTimer();
        }
    }

    private void FillBoardWithGems(bool debug = false) {
        if (debug) {
            Debug.Log("Filling board with gems:");
        }
        for (int i = 0; i < Board.instance.dimensions.width; i++) {
            List<int> emptySpaces = new List<int>();
            for (int j = 0; j < Board.instance.dimensions.height; j++) {
                Gem gem = Board.GetGem(new Vector2Int(i, j), debug);
                if (gem == null) {
                    emptySpaces.Add(j);
                }
                else if (emptySpaces.Count > 0) {
                    gem.targetPoint = Board.GetTilePos(new Vector2Int(i, emptySpaces[0]));
                    emptySpaces.RemoveAt(0);
                    emptySpaces.Add(j);
                }
            }
            if (debug) {
                Debug.Log("  " + emptySpaces.Count + " empty spaces on row " + i);
            }
            for (int j = 0; j < emptySpaces.Count; j++) {
                Gem newGem = Instantiate(gemPrefab, Board.GetTilePos(new Vector2Int(i, Board.instance.dimensions.height + j)), Quaternion.identity);
                int type = Random.Range(0, gemSprites.Count);
                newGem.type = type;
                newGem.SetSprite(gemSprites[type]);
                newGem.targetPoint = Board.GetTilePos(new Vector2Int(i, emptySpaces[j]));
            }
        }
    }
    private bool AreAllGemsStopped() {
        return Gem.AllLivingInstances.TrueForAll(g => g.IsStopped);
    }
    private bool IsAnyMatchPossible() {
        foreach (Vector2Int tile in Board.GetAllTiles()) {
            Gem curGem = Board.GetGem(tile);
            int type = curGem.type;
            foreach (List<Vector2Int> matchSetup in possibleMatchSetups) {
                for (int i = 0; i < 4; i++) {
                    List<Gem> possibleMatchGems = matchSetup.ConvertAll(p => Board.GetGem(Vector2Int.RoundToInt(tile + (Vector2)(Quaternion.AngleAxis(90f * i, Vector3.forward) * (Vector2)p))));
                    if (possibleMatchGems.TrueForAll(g => g != null && g.type == type)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    private bool IsAnyMatchMade() {
        for (int i = 0; i < Board.instance.dimensions.width; i++) {
            for (int j = 0; j < Board.instance.dimensions.height; j++) {
                Gem gem = Board.GetGem(new Vector2Int(i, j));
                if ((j < Board.instance.dimensions.height-2
                    && gem.type == Board.GetGem(new Vector2Int(i, j + 1)).type
                    && gem.type == Board.GetGem(new Vector2Int(i, j + 2)).type)
                 || (i < Board.instance.dimensions.width - 2
                    && gem.type == Board.GetGem(new Vector2Int(i + 1, j)).type
                    && gem.type == Board.GetGem(new Vector2Int(i + 2, j)).type)) {
                    return true;
                }
            }
        }
        return false;
    }
    private void ShuffleGems() {
        int boardWidth = Board.instance.dimensions.width;
        List<int> newIndices = new List<int>() { };
        for (int i = 0; i < Board.Length; i++) {
            newIndices.Insert(Random.Range(0, i+1), i);
        }
        for (int i = 0; i < newIndices.Count; i++) {
            Vector2Int oldTileId = new Vector2Int(i % boardWidth, i / boardWidth);
            Vector2Int newTileId = new Vector2Int(newIndices[i] % boardWidth, newIndices[i] / boardWidth);
            Gem curGem = Board.GetGem(oldTileId);
            curGem.targetPoint = Board.GetTilePos(newTileId);
        }
    }
    private void ClearMadeMatches() {
        List<List<Gem>> matchIslands = GetMatchIslands();
        AddMatchScore(matchIslands);
        foreach (List<Gem> island in matchIslands) {
            foreach (Gem gem in island) {
                gem.Clear();
            }
        }
        comboCounter++;
    }
    private List<List<Gem>> GetMatchIslands() {
        List<List<Gem>> result = new List<List<Gem>>();
        for (int i = 0; i < Board.instance.dimensions.width; i++) {
            int lastType = -1;
            int lastTypeStart = -1;
            int lastTypeEnd = -1;
            for (int j = 0; j < Board.instance.dimensions.height; j++) {
                Gem gem = Board.GetGem(new Vector2Int(i, j));
                int curType = gem.type;
                if (lastType == -1) {
                    lastType = curType;
                    lastTypeStart = j;
                    lastTypeEnd = j+1;
                }
                else {
                    if (lastType == curType) {
                        lastTypeEnd = j+1;
                    }
                    else {
                        if (lastTypeStart + 3 <= lastTypeEnd) {
                            List<Gem> newMatch = new List<Gem>();
                            for (int k = lastTypeStart; k < lastTypeEnd; k++) {
                                newMatch.Add(Board.GetGem(new Vector2Int(i, k)));
                            }
                            result.Add(newMatch);
                        }
                        lastType = curType;
                        lastTypeStart = j;
                        lastTypeEnd = j + 1;
                    }
                }
            }
            if (lastTypeStart + 3 <= lastTypeEnd) {
                List<Gem> newMatch = new List<Gem>();
                for (int k = lastTypeStart; k < lastTypeEnd; k++) {
                    newMatch.Add(Board.GetGem(new Vector2Int(i, k)));
                }
                result.Add(newMatch);
            }
        }
        for (int j = 0; j < Board.instance.dimensions.height; j++) {
            int lastType = -1;
            int lastTypeStart = -1;
            int lastTypeEnd = -1;
            for (int i = 0; i < Board.instance.dimensions.width; i++) {
                Gem gem = Board.GetGem(new Vector2Int(i, j));
                int curType = gem.type;
                if (lastType == -1) {
                    lastType = curType;
                    lastTypeStart = i;
                    lastTypeEnd = i + 1;
                }
                else {
                    if (lastType == curType) {
                        lastTypeEnd = i + 1;
                    }
                    else {
                        if (lastTypeStart + 3 <= lastTypeEnd) {
                            List<Gem> newMatch = new List<Gem>();
                            for (int k = lastTypeStart; k < lastTypeEnd; k++) {
                                newMatch.Add(Board.GetGem(new Vector2Int(k, j)));
                            }
                            result.Add(newMatch);
                        }
                        lastType = curType;
                        lastTypeStart = i;
                        lastTypeEnd = i + 1;
                    }
                }
            }
            if (lastTypeStart + 3 <= lastTypeEnd) {
                List<Gem> newMatch = new List<Gem>();
                for (int k = lastTypeStart; k < lastTypeEnd; k++) {
                    newMatch.Add(Board.GetGem(new Vector2Int(k, j)));
                }
                result.Add(newMatch);
            }
        }
        return result;
    }
    private void AddMatchScore(List<List<Gem>> matchIslands) {
        int matchScore = comboBonusScore * comboCounter;
        foreach (List<Gem> island in matchIslands) {
            matchScore += baseMatchScore + bonusMatchScore * (island.Count-3);
        }
        curScore += matchScore;
        ScorePanel.SetNewScore(curScore);
        if (curScore >= curGoalScore) {
            gameEnded = true;
            lastGameWon = true;
            onWon.Invoke();
        }
    }
    private void SwapGems() {
        Gem swappedGem = Board.GetGem(swappedTile);
        Gem selectedGem = Board.GetGem(selectedTile);
        swappedGem.targetPoint = Board.GetTilePos(selectedTile);
        selectedGem.targetPoint = Board.GetTilePos(swappedTile);
    }
    private void UpdateTimer() {
        curTimer -= Time.deltaTime;
        if (curTimer <= 0f) {
            gameEnded = true;
            lastGameWon = false;
            onLost.Invoke();
        }
        timerText.text = Mathf.CeilToInt(curTimer).ToString();
    }

    public void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
