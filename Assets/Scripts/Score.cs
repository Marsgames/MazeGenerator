using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    #region Variables
    private int m_score = 0;
    private Text m_text;
    #endregion Variables

    #region Unity's functions
    // Start is called before the first frame update
    void Start()
    {
        m_text = GetComponent<Text>();
    }
    #endregion Unity's functions

    #region Functions
    public void IncrementScore()
    {
        m_text.text = "Score : " + ++m_score;
    }
    #endregion Functions

    #region Accessors
    #endregion Accessors
}
