using UnityEngine;
using System.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.AI;

public class MazeGenerator : MonoBehaviour
{
    #region Variables
    [SerializeField] private GameObject m_wallPrefab = null;
    [SerializeField] private GameObject m_floorPrefab = null;
    [SerializeField] private GameObject m_playerPrefab = null;
    [SerializeField] private GameObject m_trailPrefab = null;
    [SerializeField] private GameObject m_trailContainer = null;
    [SerializeField] private bool m_randomStartAndEnd = false;

    private int m_height = 7;
    private int m_width = 4;
    private int m_lastGrow = 2;
    private int m_currentX = 0;
    private int m_currentY = 0;
    private float m_size = 6;
    private Cell[] m_cells = new Cell[28];
    private float m_waitTimer = 1;
    private bool m_diggingComplete;
    private Camera m_mainCamera;
    private NavMeshSurface m_navmesh;
    private Score m_score;
    private GameObject m_player;
    private Vector3 m_objective;
    private Vector3 dragOrigin;
    #endregion Variables

    #region Unity's functions
    // Start is called before the first frame update
    void Start()
    {
        CheckIfOk();

        m_mainCamera = Camera.main;
        m_navmesh = FindObjectOfType<NavMeshSurface>();
        m_score = FindObjectOfType<Score>();
        GameObject.Find("CubeForOcclusion").SetActive(false);

        //Random.InitState(0);

        StartCoroutine(InitMaze());
    }

    // Update is called once per frame
    void Update()
    {
        //#if UNITY_EDITOR
        // Space to go to the next level
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GoToNextLevel();
            //Debug.LogWarning("/!\\ le navmesh est cassé et l'affichage du chemin peut ne pas fonctionner suite à l'utilisation de la touche espace pour changer de niveau /!\\");
        }
        //#endif

        // B to show the path
        if (Input.GetKeyDown(KeyCode.B))
        {
            GameObject newGo = new GameObject();
            newGo.transform.position = m_player.transform.position;
            NavMeshPath newPath = new NavMeshPath();
            NavMeshAgent agent = newGo.AddComponent<NavMeshAgent>();
            agent.transform.position = m_player.transform.position;

            agent.CalculatePath(m_objective, newPath);

            Destroy(newGo);

            GameObject go = Instantiate(m_trailPrefab, m_player.transform.position, transform.rotation);
            go.name = "trail-" + name;
            Vector3[] cornersTrail = newPath.corners;
            for (int i = 0; i < cornersTrail.Length; i++)
            {
                cornersTrail[i] += Vector3.up * 2;
            }
            go.GetComponent<TrailController>().SetWaypoints(cornersTrail);

            go.transform.SetParent(m_trailContainer.transform);
        }

        // E to enable / disable random start & end
        if (Input.GetKeyDown(KeyCode.E))
        {
            m_randomStartAndEnd = !m_randomStartAndEnd;
        }



        // Those 2 functions are from https://answers.unity.com/questions/20228/mouse-wheel-zoom.html
        if (Input.GetAxis("Mouse ScrollWheel") < 0) // back
        {
            m_mainCamera.orthographicSize += 1;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
        {
            if (m_mainCamera.orthographicSize < 50)
            {
                return;
            }

            m_mainCamera.orthographicSize -= 1;
        }


        // https://forum.unity.com/threads/click-drag-to-move-camera-script-i-need-the-camera-to-move-in-reverse-directions-help.501604/
        CameraDrag();

    }
    #endregion Unity's functions

    #region Functions
    /// <summary>
    /// Extend the maze for a new game
    /// </summary>
    void GrowMaze()
    {
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
                    DestroyWall(0, m_currentX, m_currentY);
                    m_currentX--;
                    break;

                case 1: // haut
                    if (CellIsVisited(m_currentX, m_currentY + 1))
                    {
                        continue;
                    }
                    DestroyWall(1, m_currentX, m_currentY);
                    m_currentY++;
                    break;

                case 2: // droite
                    if (CellIsVisited(m_currentX + 1, m_currentY))
                    {
                        continue;
                    }
                    DestroyWall(2, m_currentX, m_currentY);
                    m_currentX++;
                    break;

                case 3: // bas
                    if (CellIsVisited(m_currentX, m_currentY - 1))
                    {
                        continue;
                    }
                    DestroyWall(3, m_currentX, m_currentY);
                    m_currentY--;
                    break;
            }



            int index = m_currentY * m_width + m_currentX;
            int indexMax = m_width * m_height;

            if (index < 0 || index >= indexMax)
            {
                continue;
            }
            //// adapter en fonction de la taille de la grille
            if (timer.Elapsed < new System.TimeSpan(0, 0, 10))
            {
                yield return null;
            }
            PaintCell(m_currentX, m_currentY, Color.red);
            m_cells[index].SetIsVisited(true);
        }

        yield return null;
        FindNewWay();
    }

    /// <summary>
    /// Find a cell wich have an adjacent cell not visited
    /// </summary>
    void FindNewWay()
    {
        m_diggingComplete = true;

        int x, y, indexMax = m_width * m_height;
        for (int index = 0; index < indexMax; index++)
        {
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

        // Paint the begining and the end of the level
        // Instantiate the player
        int endLevelIndex = m_width * m_height - 1;
        int endLevelX = m_width - 1;
        int endLevelY = m_height - 1;

        int startPlayerIndex = 0;
        int startPlayerX = 0;
        int startPlayerY = 0;
        if (m_randomStartAndEnd)
        {
            endLevelIndex = Random.Range(0, m_width * m_height + 1);
            endLevelX = endLevelIndex % m_width;
            endLevelY = endLevelIndex / m_width;

            startPlayerIndex = Random.Range(0, m_width * m_height + 1);
            startPlayerX = startPlayerIndex % m_width;
            startPlayerY = startPlayerIndex / m_width;
        }

        PaintCell(endLevelX, endLevelY, Color.green);
        m_cells[endLevelIndex].GetFloor().AddComponent<EndLevel>();

        m_navmesh.BuildNavMesh();
        m_objective = m_cells[endLevelIndex].GetFloor().transform.position;


        PaintCell(startPlayerX, startPlayerY, Color.blue);
        Vector3 playerPosition = m_cells[startPlayerIndex].GetFloor().transform.position;
        playerPosition.y += .5f;
        m_player = Instantiate(m_playerPrefab, playerPosition, Quaternion.identity);
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

    /// <summary>
    /// Reset variables and launch the next level
    /// </summary>
    public void GoToNextLevel()
    {
        foreach (Player player in FindObjectsOfType<Player>())
        {
            Destroy(player.gameObject);
        }
        //Vector3 camPosition = m_mainCamera.transform.position;
        //camPosition.x += 2.5f;
        //camPosition.y += 5.5f;
        //camPosition.z -= 0f;

        foreach (Transform child in m_trailContainer.transform)
        {
            Destroy(child.gameObject);
        }

        StopAllCoroutines();
        GrowMaze();
        m_currentX = 0;
        m_currentY = 0;
        m_diggingComplete = false;
        //m_mainCamera.transform.position = camPosition;
        SetCameraPosition();
        m_score.IncrementScore();
        StartCoroutine(InitMaze());
    }

    private void SetCameraPosition()
    {
        //float halfWidth = m_width / 2;
        //float halfHeight = m_height / 4;
        ////Vector3 position = new Vector3(halfWidth * 6, 100, halfHeight * 6);

        //m_mainCamera.transform.position = position;

        Vector3 position = m_mainCamera.transform.position;
        position.x += 3;

        m_mainCamera.transform.position = position;

        m_mainCamera.orthographicSize += 10;

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

    private void CameraDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        Vector3 pos = m_mainCamera.ScreenToViewportPoint(dragOrigin - Input.mousePosition);
        Vector3 move = new Vector3(pos.x * 2f, 0, pos.y * 2f);

        m_mainCamera.transform.Translate(move, Space.World);
    }

    /// <summary>
    /// Checks if everything is set correctly at start
    /// </summary>
    void CheckIfOk()
    {
#if UNITY_EDITOR
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

        if (null == m_playerPrefab)
        {
            Debug.LogError("PlayerPrefab cannot be null in " + name, this);
            quit = true;
        }

        if (null == m_trailPrefab)
        {
            Debug.LogError("TrailPrefab cannot be null in " + name, this);
            quit = true;
        }

        if (null == m_trailContainer)
        {
            Debug.LogError("TrailContainer cannot be null in " + name, this);
            quit = true;
        }


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
