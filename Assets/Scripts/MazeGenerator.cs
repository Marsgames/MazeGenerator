using UnityEngine;
using System.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class MazeGenerator : MonoBehaviour
{
    #region Variables
    [SerializeField] private GameObject m_wallPrefab = null;
    [SerializeField] private GameObject m_floorPrefab = null;
    [SerializeField] private GameObject m_playerPrefab = null;

    private int m_height = 7;
    private int m_width = 4;
    private int m_lastGrow = 2;
    private int m_currentX = 0;
    private int m_currentY = 0;
    private float m_size = 6;
    private Cell[] m_cells = new Cell[28];
    private float m_waitTimer = 1;
    private bool m_diggingComplete;
    #endregion Variables

    #region Unity's functions
    // Start is called before the first frame update
    void Start()
    {
        CheckIfOk();

        Random.InitState(0);

        StartCoroutine(InitMaze());
        //InitMaze();
    }

    // Update is called once per frame
    void Update()
    {
        // Space to go to the next level
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GoToNextLevel();
        }
    }
    #endregion Unity's functions

    #region Functions
    /// <summary>
    /// Extend the maze for a new game
    /// </summary>
    void GrowMaze()
    {
        //m_timerDevide += .1f;
        //m_timeToWait /= m_timerDevide;
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        m_width++;
        m_height += m_lastGrow == 2 ? 1 : 2;
        m_lastGrow = m_lastGrow == 2 ? 1 : 2;

        m_cells = new Cell[m_width * m_height];
    }

    /// <summary>
    /// Create the maze
    /// This function has to be called in StartCoroutine()
    /// </summary>
    IEnumerator InitMaze()
    {
        GameObject cellGO;
        GameObject floor, leftWall, rightWall, upWall, downWall;
        Cell cell;
        Stopwatch timer = Stopwatch.StartNew();
        timer.Start();

        int x, y, indexMax = m_width * m_height;
        for (int index = 0; index < indexMax; index++)
        {
            x = index % m_width;
            y = index / m_width;
            cell = new Cell();
            cellGO = new GameObject("Cell[" + x + ", " + y + "] - index " + index);

            floor = Instantiate(m_floorPrefab, new Vector3(x * m_size, -(m_size / 2f), y * m_size), Quaternion.identity);
            floor.name = "floor[" + x + ',' + y + "]";
            floor.transform.Rotate(Vector3.right, 90);
            floor.transform.SetParent(cellGO.transform);

            cell.SetIsVisited(false);
            cell.SetFloor(floor);

            if (0 == x)
            {
                leftWall = Instantiate(m_wallPrefab, new Vector3((x * m_size) - (m_size / 2f), 0, y * m_size), Quaternion.identity);
                leftWall.name = "leftWall[" + x + "," + y + "]";
                leftWall.transform.Rotate(Vector3.up * 90f);
                leftWall.transform.SetParent(cellGO.transform);
                cell.SetLeftWall(leftWall);
            }

            rightWall = Instantiate(m_wallPrefab, new Vector3((x * m_size) + (m_size / 2f), 0, y * m_size), Quaternion.identity);
            rightWall.name = "rightWall[" + x + "," + y + "]";
            rightWall.transform.Rotate(Vector3.down * 90f);
            rightWall.transform.SetParent(cellGO.transform);
            cell.SetRightWall(rightWall);

            if (0 == y)
            {
                downWall = Instantiate(m_wallPrefab, new Vector3(x * m_size, 0, (y * m_size) - (m_size / 2f)), Quaternion.identity);
                downWall.name = "downWall[" + x + "," + y + "]";
                downWall.transform.SetParent(cellGO.transform);
                cell.SetDownWall(downWall);
            }

            upWall = Instantiate(m_wallPrefab, new Vector3(x * m_size, 0, (y * m_size) + (m_size / 2f)), Quaternion.identity);
            upWall.name = "upWall[" + x + "," + y + "]";
            upWall.transform.SetParent(cellGO.transform);
            cell.SetUpWall(upWall);

            cellGO.transform.SetParent(transform);
            m_cells[index] = cell;

            // Warning but it works correctly
            if (0 == (y * m_height + x) % m_waitTimer)
            {
                yield return new WaitForSecondsRealtime(.01f);
            }
        }

        timer.Stop();
        if (timer.Elapsed > new System.TimeSpan(0, 0, 1))
        {
            m_waitTimer += 1.5f;
        }

        StartDigging();

        yield return null;
    }

    /// <summary>
    /// Create the maze while all cells are not digged
    /// </summary>
    public void StartDigging()
    {
        PaintCell(0, 0, Color.red);
        if (m_diggingComplete)
        {
            return;
        }

        StartCoroutine(DigNewTunnel());
    }

    /// <summary>
    /// Get a random direction and remove a wall if it's possible while we're not stuck (all cells around are already visited)
    /// This function as to be called in StartCoroutine()
    /// </summary>
    IEnumerator DigNewTunnel()
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();
        while (CanDig(m_currentX, m_currentY))
        {
            // 0 gauche
            // 1 haut
            // 2 droite
            // 3 bas
            int dir = Random.Range(0, 4);
            if (4 == dir)
            {
                Debug.LogError("dir == 4, valeur impossible pour le switch");
            }

            switch (dir)
            {
                case 0: // gauche
                    if (CellIsVisited(m_currentX - 1, m_currentY))
                    {
                        continue;
                    }
                    else
                    {
                        DestroyWall(0, m_currentX, m_currentY);
                        m_currentX--;
                    }
                    break;

                case 1: // haut
                    if (CellIsVisited(m_currentX, m_currentY + 1))
                    {
                        continue;
                    }
                    else
                    {
                        DestroyWall(1, m_currentX, m_currentY);
                        m_currentY++;
                    }
                    break;

                case 2: // droite
                    if (CellIsVisited(m_currentX + 1, m_currentY))
                    {
                        continue;
                    }
                    else
                    {
                        DestroyWall(2, m_currentX, m_currentY);
                        m_currentX++;
                    }
                    break;

                case 3: // bas
                    if (CellIsVisited(m_currentX, m_currentY - 1))
                    {
                        continue;
                    }
                    else
                    {
                        DestroyWall(3, m_currentX, m_currentY);
                        m_currentY--;
                    }
                    break;
            }



            int index = m_currentY * m_width + m_currentX;
            int indexMax = m_width * m_height;

            if (index < 0 || index >= indexMax)
            {
                continue;
            }
            else
            {
                //// adapter en fonction de la taille de la grille
                if (timer.Elapsed < new System.TimeSpan(0, 0, 10))
                {
                    yield return null;
                }
                PaintCell(m_currentX, m_currentY, Color.red);
                m_cells[index].SetIsVisited(true);
            }
        }

        yield return null;
        FindNewWay();
    }

    /// <summary>
    /// Find a cell wich have an adjacent cell not visited
    /// </summary>
    void FindNewWay()
    {
        m_diggingComplete = true; // Set it to this, and see if we can prove otherwise below!

        int x, y, indexMax = m_width * m_height;
        for (int index = 0; index < indexMax; index++)
        {
            if (index >= indexMax)
            {
                print("index : " + index);
                continue;
            }

            x = index % m_width;
            y = index / m_width;

            if (!m_cells[index].GetIsVisited() && IsThereAnAdjacentVisitedCell(x, y))
            {
                m_diggingComplete = false;
                m_currentX = x;
                m_currentY = y;

                m_cells[index].SetIsVisited(true);
                PaintCell(x, y, Color.red);

                StartDigging();
                return;
            }
        }

        PaintCell(0, 0, Color.blue);
        PaintCell(m_width - 1, m_height - 1, Color.green);
        m_cells[m_width * m_height - 1].GetFloor().AddComponent<EndLevel>();
        Vector3 playerPosition = m_cells[0].GetFloor().transform.position;
        playerPosition.y += .5f;
        Instantiate(m_playerPrefab, playerPosition, Quaternion.identity);
    }

    /// <summary>
    /// Return true if there is at least one direction that has not been visited already
    /// </summary>
    /// <returns><c>true</c>, if dig is possible, <c>false</c> otherwise.</returns>
    /// <param name="positionX">Position x.</param>
    /// <param name="positionY">Position y.</param>
    private bool CanDig(int positionX, int positionY)
    {
        bool canDig = false;
        int indexMax = m_width * m_height;

        // x + 1
        int index = (positionX + 1) + positionY * m_width;
        if (positionX > 0 && positionX < m_width - 1 && positionX < m_width && !m_cells[index].GetIsVisited())
        {
            canDig = true;
        }

        // x - 1
        index = (positionX - 1) + positionY * m_width;
        if (index > 0 && index < indexMax && positionX > 0 && !m_cells[index].GetIsVisited())
        {
            canDig = true;
        }

        // y + 1
        index = positionX + (positionY + 1) * m_width;
        if (index > 0 && index < indexMax && positionY < m_height && !m_cells[index].GetIsVisited())
        {
            canDig = true;
        }

        // y - 1
        index = positionX + (positionY - 1) * m_width;
        if (index > 0 && index < indexMax && positionY > 0 && !m_cells[index].GetIsVisited())
        {
            canDig = true;
        }

        return canDig;
    }

    /// <summary>
    /// Returns false if cell is in maze and has not already been visited
    /// </summary>
    /// <returns><c>true</c>, if is visited was celled, <c>false</c> otherwise.</returns>
    /// <param name="cellX">Cell x.</param>
    /// <param name="cellY">Cell y.</param>
    private bool CellIsVisited(int cellX, int cellY)
    {
        if (cellX < 0 || cellX >= m_width || cellY < 0 || cellY >= m_height)
        {
            // Index out of range
            return true;
        }

        int index = cellY * m_width + cellX;
        int indexMax = m_width * m_height;
        if (index < 0 || index >= indexMax)
        {
            // index out of range
            return true;
        }

        return m_cells[index].GetIsVisited();
    }

    /// <summary>
    /// Destroy wall of this cell, and wall in adjacent cell
    /// </summary>
    /// <param name="direction">Direction (0 : left, 1 : up, 2 : right, 3 : down).</param>
    /// <param name="posX">Position x.</param>
    /// <param name="posY">Position y.</param>
    private void DestroyWall(int direction, int posX, int posY)
    {
        int index = posX + posY * m_width;
        int indexMax = m_width * m_height;

        switch (direction)
        {
            case 0: // gauche
                if (0 == posX)
                {
                    Destroy(m_cells[index].GetLeftWall());
                }
                else
                {
                    int localIndex = (posX - 1) + posY * m_width;
                    if (localIndex < 0)
                    {
                        return;
                    }
                    Destroy(m_cells[localIndex].GetRightWall());
                }
                break;

            case 1: // haut
                if (index < 0)
                {
                    return;
                }
                Destroy(m_cells[index].GetUpWall());
                break;

            case 2: // droite
                if (index < 0 || posX + 1 >= m_width)
                {
                    return;
                }
                Destroy(m_cells[index].GetRightWall());
                break;

            case 3: // bas
                if (0 == posY)
                {
                    Destroy(m_cells[index].GetDownWall());
                }
                else
                {
                    Destroy(m_cells[posX + (posY - 1) * m_width].GetUpWall());
                }
                break;
        }

    }

    /// <summary>
    /// Returns true if the cell[posX, posY] has an adjacent cell visited, and if so, digs the wall
    /// </summary>
    /// <returns><c>true</c>, if there an adjacent visited cell was ised, <c>false</c> otherwise.</returns>
    /// <param name="posX">Position x.</param>
    /// <param name="posY">Position y.</param>
    bool IsThereAnAdjacentVisitedCell(int posX, int posY)
    {
        // x - 1
        if (posX > 0 && m_cells[(posX - 1) + posY * m_width].GetIsVisited())
        {
            DestroyWall(0, posX, posY);
            return true;
        }

        // x + 1
        if (posX < m_width && m_cells[(posX + 1) + posY * m_width].GetIsVisited())
        {
            DestroyWall(2, posX, posY);
            return true;
        }

        // y - 1
        if (posY > 0 && m_cells[posX + (posY - 1) * m_width].GetIsVisited())
        {
            DestroyWall(3, posX, posY);
            return true;
        }

        // y + 1
        if (posY < m_height - 2 && m_cells[posX + (posY + 1) * m_width].GetIsVisited())
        {
            DestroyWall(1, posX, posY);
            return true;
        }

        return false;
    }

    public void GoToNextLevel()
    {
        StopAllCoroutines();
        GrowMaze();
        m_currentX = 0;
        m_currentY = 0;
        m_diggingComplete = false;
        StartCoroutine(InitMaze());
    }

    /// <summary>
    /// Paints a cell in the maze
    /// </summary>
    /// <param name="posX">Position x.</param>
    /// <param name="posY">Position y.</param>
    /// <param name="color">Color.</param>
    void PaintCell(int posX, int posY, Color color)
    {
        int index = posX + posY * m_width;
        int indexMax = m_height * m_width;
        if (index < 0 || index >= indexMax)
        {
            return;
        }

        // floor
        GameObject floor = m_cells[index].GetFloor();
        if (floor)
        {
            floor.GetComponent<Renderer>().material.color = color * 0.2f;
        }

        // upWall
        GameObject upWall = m_cells[index].GetUpWall();
        if (upWall)
        {
            upWall.GetComponent<Renderer>().material.color = color;
        }

        // downWall
        if (0 == posY)
        {
            GameObject downWall = m_cells[index].GetDownWall();
            if (downWall)
            {
                downWall.GetComponent<Renderer>().material.color = color;
            }
        }
        else
        {
            int localIndex = posX + (posY - 1) * m_width;
            if (localIndex < 0)
            {
                return;
            }
            GameObject downWall = m_cells[localIndex].GetUpWall();
            if (downWall)
            {
                downWall.GetComponent<Renderer>().material.color = color;
            }
        }

        // leftWall
        if (0 == posX)
        {
            GameObject leftWall = m_cells[index].GetLeftWall();
            if (leftWall)
            {
                leftWall.GetComponent<Renderer>().material.color = color;
            }
        }
        else
        {
            GameObject leftWall = m_cells[(posX - 1) + posY * m_width].GetRightWall();
            if (leftWall)
            {
                leftWall.GetComponent<Renderer>().material.color = color;
            }
        }

        // rightWall
        GameObject rightWall = m_cells[index].GetRightWall();
        if (rightWall)
        {
            rightWall.GetComponent<Renderer>().material.color = color;
        }
    }

    /// <summary>
    /// Checks if everything is set correctly at start
    /// </summary>
    void CheckIfOk()
    {
        bool quit = false;

        if (null == m_wallPrefab)
        {
            Debug.LogError("WallPrefab cannot be null in " + name, this);
            quit = true;
        }
        if (null == m_floorPrefab)
        {
            Debug.LogError("FloorPrefab cannot be null in " + name, this);
            quit = true;
        }

#if UNITY_EDITOR
        if (!quit)
        {
            return;
        }
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    new void print(object thingToPrint)
    {
        Debug.Log(thingToPrint);
    }
    #endregion Functions

    #region Accessors
    #endregion Accessors
}
