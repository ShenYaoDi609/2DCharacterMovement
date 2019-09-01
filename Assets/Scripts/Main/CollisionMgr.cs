using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionMgr : MonoBehaviour
{
    [Header("Layers")]
    public LayerMask groundLayer;

    [Space]
    [Header("Boolean")]
    public bool onGround;
    public bool onRightWall;
    public bool onLeftWall;
    public bool onWall;
    //-1:右侧碰到墙壁
    //1：左侧碰到墙壁
    public int colSide;
    [Space]
    [Header("Collsion")]
    public float collisionRadius = 0.25f;
    public Vector2 bottomOffset;
    public Vector2 rightOffset;
    public Vector2 leftOffset;



    void Start()
    {
        
    }

    
    void Update()
    {
        onGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, collisionRadius, groundLayer);
        onWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer)
            || Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);
        onRightWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer);
        onLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);

        colSide = onRightWall ? -1 : 1;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset, collisionRadius);
    }
}
