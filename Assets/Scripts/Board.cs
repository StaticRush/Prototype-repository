using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public class Board : MonoBehaviour {

    
    public GameState currentState=GameState.move;
    public int width;
    public int height;
    public int offSet;
    public GameObject tilePrefab;
    public GameObject[] dots;
    private BackgroundTile[,] allTiles;
    public GameObject[,] allDots;
    private FindMatches findMatches;
    public int basePieceValue = 20;
    private int streakValue = 1;
    private Score score;

    // Use this for initialization
    void Start() {
        findMatches = FindObjectOfType<FindMatches>();
        score = FindObjectOfType<Score>();
        allTiles = new BackgroundTile[width, height];
        allDots = new GameObject[width, height];
        SetUp();
    }

    // Update is called once per frame
    private void SetUp() {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2(i, j+offSet);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "(" + i + "," + j + ")";
                int dotToUse = Random.Range(0, dots.Length);
                int maxIterations = 0;
                while (MatchesAt(i, j, dots[dotToUse])&& maxIterations<100)
                {
                    dotToUse = Random.Range(0, dots.Length);
                    maxIterations++;
                }
                maxIterations = 0;

                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.GetComponent<Gems>().row = j;
                dot.GetComponent<Gems>().column = i;
                dot.transform.parent = this.transform;
                dot.name = "(" + i + "," + j + ")";
                allDots[i, j] = dot;
            }
        }
    }
    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if (column > 1 && row > 1)
        {
            if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
            {
                return true;
            }
            if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
            {
                return true;
            }
        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
            if (column > 1)
            {
                if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
        }
            return false;
        }
     private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<Gems>().isMatched){
            findMatches.currentMatches.Remove(allDots[column, row]);
            Destroy(allDots[column, row]);
            score.IncreaseScore(basePieceValue * streakValue);
            allDots[column, row] = null;
        }
    }
    public void DestroyMatches()
    {
        for(int i=0; i< width; i++)
        {
            for (int j=0; j<height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(RowDecreaseCo());
    }
    private IEnumerator RowDecreaseCo()
    {
        int nullcount = 0;
        for (int i=0; i < width; i++)
        {
            for (int j=0; j<height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullcount++;
                }
                else if (nullcount > 0)
                {
                    allDots[i, j].GetComponent<Gems>().row -= nullcount;
                    allDots[i, j] = null;
                }
            }
            nullcount = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard()
    {
        for (int i=0;i<width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    Vector2 tempPosition = new Vector2(i, j+offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = piece;
                    piece.GetComponent<Gems>().row = j;
                    piece.GetComponent<Gems>().column = i;
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for(int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (allDots[i, j].GetComponent<Gems>().isMatched) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
     private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(.5f);

        while (MatchesOnBoard())
        {
            streakValue++;
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        yield return new WaitForSeconds(.5f);
        currentState = GameState.move;
    }

    }