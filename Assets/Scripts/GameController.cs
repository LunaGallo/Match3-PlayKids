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
    public List<Gem> gemPrefabs;
    public Text timerText;
    public Text levelText;
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
        levelText.text = "Level " + (curLevel+1);
        curGoalScore = baseGoalScore + (curLevel * incGoalScore);
    }
    private void Update() {
        if (!gameEnded) {
            if (AreAllGemsStopped()) {
                if (IsAnyMatchMade()) {
                    ClearMadeMatches();
                    FillBoardWithGems();
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

    private Gem NewRandomGem() {
        return gemPrefabs[Random.Range(0, gemPrefabs.Count)];
    }
    private void FillBoardWithGems() {
        for (int i = 0; i < Board.instance.dimensions.width; i++) {
            List<int> emptySpaces = new List<int>();
            for (int j = 0; j < Board.instance.dimensions.height; j++) {
                Gem gem = Board.GetGem(new Vector2Int(i, j));
                if (gem == null) {
                    emptySpaces.Add(j);
                }
                else if (emptySpaces.Count > 0) {
                    gem.targetPoint = Board.GetTilePos(new Vector2Int(i, emptySpaces[0]));
                    emptySpaces.RemoveAt(0);
                    emptySpaces.Add(j);
                }
            }
            for (int j = 0; j < emptySpaces.Count; j++) {
                Gem newGem = Instantiate(NewRandomGem(), Board.GetTilePos(new Vector2Int(i, Board.instance.dimensions.height + j)), Quaternion.identity);
                newGem.targetPoint = Board.GetTilePos(new Vector2Int(i, emptySpaces[j]));
            }
        }
    }
    private bool AreAllGemsStopped() {
        return Gem.instanceList.TrueForAll(g => g.IsStopped);
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
        for (int i = 0; i < Board.instance.dimensions.width-2; i++) {
            for (int j = 0; j < Board.instance.dimensions.height-2; j++) {
                Gem gem = Board.GetGem(new Vector2Int(i, j));
                if ((gem.type == Board.GetGem(new Vector2Int(i, j + 1)).type
                    && gem.type == Board.GetGem(new Vector2Int(i, j + 2)).type)
                 || (gem.type == Board.GetGem(new Vector2Int(i + 1, j)).type
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
                Destroy(gem.gameObject);
            }
        }
        comboCounter++;
    }
    private List<List<Gem>> GetMatchIslands() {
        List<List<Gem>> result = new List<List<Gem>>();
        foreach (Vector2Int curTile in Board.GetAllTiles()) {
            Gem curGem = Board.GetGem(curTile);
            List<List<Gem>> fittingIslands = result.FindAll(i => i[0].type == curGem.type && i.Exists(g => (g.CurTileId - curGem.CurTileId).sqrMagnitude == 1f));
            if (fittingIslands != null && fittingIslands.Count > 0) {
                fittingIslands[0].Add(curGem);
                for (int i = 1; i < fittingIslands.Count; i++) {
                    fittingIslands[0].AddRange(fittingIslands[i]);
                    result.Remove(fittingIslands[i]);
                }
            }
            else {
                result.Add(new List<Gem>() { curGem });
            }
        }
        result.RemoveAll(i => i.Count < 3);
        return result;
    }
    private void AddMatchScore(List<List<Gem>> matchIslands) {
        int matchScore = comboBonusScore * comboCounter;
        foreach (List<Gem> island in matchIslands) {
            matchScore += baseMatchScore + (bonusMatchScore * island.Count-3);
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
        timerText.text = Mathf.CeilToInt(curTimer).ToString() + "s";
    }

    public void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
