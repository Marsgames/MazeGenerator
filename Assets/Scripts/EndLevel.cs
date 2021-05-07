using UnityEngine;

public class EndLevel : MonoBehaviour
{
    #region Variables
    [SerializeField] private BoxCollider endLevelCollider;

    private MazeGenerator m_mg;
    #endregion Variables

    #region Unity's functions
    // Start is called before the first frame update
    void Start()
    {
        m_mg = FindObjectOfType<MazeGenerator>();

        endLevelCollider = gameObject.AddComponent<BoxCollider>();
        Vector3 center = endLevelCollider.center;
        center.z -= 5;
        endLevelCollider.center = center;
        Vector3 size = endLevelCollider.size;
        size.x = .1f;
        size.y = .1f;
        size.z = 10;
        endLevelCollider.size = size;
        endLevelCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.tag.Equals("Player"))
        {
            return;
        }

        Destroy(other.gameObject);
        m_mg.GoToNextLevel();
    }
    #endregion Unity's functions

    #region Functions
    #endregion Functions

    #region Accessors
    #endregion Accessors
}
