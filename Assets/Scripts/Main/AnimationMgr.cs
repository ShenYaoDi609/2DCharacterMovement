using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationMgr : MonoBehaviour
{
    private Animator anim;
    private MoveMentController moveCtrl;
    private CollisionMgr collision;
    public SpriteRenderer render;

    private void Start()
    {
        anim = GetComponent<Animator>();
        moveCtrl = GetComponentInParent<MoveMentController>();
        collision = GetComponentInParent<CollisionMgr>();
        render = GetComponent<SpriteRenderer>();
    }


    private void Update()
    {
        anim.SetBool("onGround", collision.onGround);
        anim.SetBool("isDashing", moveCtrl.isDashing);
        anim.SetBool("WallSlide", moveCtrl.wallSlide);
        anim.SetBool("WallGrab", moveCtrl.wallGrab);
    }

    public void SetMoveMent(float x, float y, float vely)
    {
        anim.SetFloat("HorizontalAxis", x);
        anim.SetFloat("VerticalAxis", y);
        anim.SetFloat("VerticalVelocity", vely);
    }

    public void SetFloat(string name,float num)
    {
        anim.SetFloat(name, num);
    }

    public void SetTrigger(string name)
    {
        anim.SetTrigger(name);
    }

    public void  SetFilp(int side)
    {
        render.flipX = (side == 1) ? false : true;
    }
}
