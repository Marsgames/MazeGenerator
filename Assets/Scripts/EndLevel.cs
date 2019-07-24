using UnityEngine;

public class EndLevel : MonoBehaviour
{
    #region Variables
    [SerializeField] private BoxCollider endLevelCollider;
    #endregion Variables

    #region Unity's functions
    // Start is called before the first frame update
    void Start()
    {
        endLevelCollider = gameObject.AddComponent<BoxCollider>();
        Vector3 center = endLevelCollider.center;
        center.z -= 5;
        endLevelCollider.center = center;
        Vector3 size = endLevelCollider.size;
        size.z = 10;
        endLevelCollider.size = size;
        endLevelCollider.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("on trigger enter");
    }
    #endregion Unity's functions

    #region Functions
    #endregion Functions

    #region Accessors
    #endregion Accessors
}
