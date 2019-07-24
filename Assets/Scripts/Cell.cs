
using UnityEngine;

public class Cell
{
    #region Variables
    private bool m_isVisited = false;
    private GameObject m_upWall, m_downWall, m_leftWall, m_rightWall, m_floor;
    #endregion Variables

    #region Unity's functions
    #endregion Unity's functions

    #region Functions
    #endregion Functions

    #region Accessors
    /// <summary>
    /// Return true if the cell was previously visited
    /// </summary>
    /// <returns><c>true</c>, if the cell was previously visited, <c>false</c> otherwise.</returns>
    public bool GetIsVisited()
    {
        return m_isVisited;
    }
    public void SetIsVisited(bool isVisited)
    {
        m_isVisited = isVisited;
    }

    public void SetFloor(GameObject floor)
    {
        m_floor = floor;
    }
    public GameObject GetFloor()
    {
        return m_floor;
    }

    public GameObject GetUpWall()
    {
        return m_upWall;
    }
    public void SetUpWall(GameObject upWall)
    {
        m_upWall = upWall;
    }

    public GameObject GetDownWall()
    {
        return m_downWall;
    }
    public void SetDownWall(GameObject downWall)
    {
        m_downWall = downWall;
    }

    public GameObject GetLeftWall()
    {
        return m_leftWall;
    }
    public void SetLeftWall(GameObject leftWall)
    {
        m_leftWall = leftWall;
    }

    public GameObject GetRightWall()
    {
        return m_rightWall;
    }
    public void SetRightWall(GameObject rightWall)
    {
        m_rightWall = rightWall;
    }
    #endregion Accessors
}