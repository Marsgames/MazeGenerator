using UnityEngine;

public class Player : MonoBehaviour
{
    #region Variables
    Rigidbody m_rigidbody;
    #endregion Variables

    #region Unity's functions
    // Start is called before the first frame update
    void Start()
    {
        m_rigidbody = gameObject.AddComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float hMove = Input.GetAxis("Horizontal");
        float vMove = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(hMove, 0, vMove);
        m_rigidbody.AddForce(move * 20);
    }
    #endregion Unity's functions

    #region Functions
    #endregion Functions

    #region Accessors
    #endregion Accessors
}
