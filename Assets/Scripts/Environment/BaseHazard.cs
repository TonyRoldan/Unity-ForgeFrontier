using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class BaseHazard : MonoBehaviour
{
    public BoxCollider2D boxCollider;
    [SerializeField] Player playerRef;
    [SerializeField] float damage;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.CompareTag("Player"))
        {       
            playerRef.TakeDamage(damage);
        }
    }
}
