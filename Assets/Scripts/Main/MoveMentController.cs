using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveMentController : MonoBehaviour
{
    private Rigidbody2D rigid;
    private AnimationMgr anim;
    private CollisionMgr collision;

    [Space]
    [Header("Status")]
    public float speed = 10;
    public float jumpForce = 10;
    public float slideSpeed = 5;
    public float dashSpeed = 20;
    public float wallJumpLerp = 10;
    //主角朝向 1：右侧 \ -1：左侧
    public int side = 1;

    [Space]
    [Header("Boolean")]
    public bool canMove;
    //是否在 冲刺过程中
    public bool isDashing;
    public bool wallSlide;
    public bool wallGrab;
    public bool wallJump;
    //是否能进行冲刺
    private bool canDash = true;
    //保证在Update中GroundTouch只触发一次
    private bool groundTouch;

    [Space]
    [Header("Particle")]
    public ParticleSystem jumpParticle;
    public ParticleSystem slideParticle;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationMgr>();
        collision = GetComponent<CollisionMgr>();
    }


    void Update()
    {

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        Walk(dir);
        anim.SetMoveMent(Mathf.Abs(dir.x), dir.y, rigid.velocity.y);

        #region WallGrab
        if (collision.onWall && Input.GetButton("Fire3"))
        {
            if (side != collision.colSide)
            {
                anim.SetFilp(-side);
            }
            wallGrab = true;
            wallSlide = false;
        }

        if (Input.GetButtonUp("Fire3") || !collision.onWall || !canMove)
        {
            wallGrab = false;
            wallSlide = false;
            rigid.gravityScale = 3;
        }

        if (wallGrab && !isDashing)
        {
            rigid.gravityScale = 0;

            //设置向上爬和向下爬不同速率
            float speedModifier = y > 0 ? .5f : 1;
            rigid.velocity = new Vector2(rigid.velocity.x, y * speed * speedModifier);
        }
        #endregion

        #region WallSlide
        if (collision.onWall && !collision.onGround)
        {
            canDash = true;

            if (x != 0 && !wallGrab)
            {
                wallSlide = true;
                WallSlide();
            }

        }

        if (!collision.onWall || collision.onGround )
        {
            wallSlide = false;
        }
        #endregion

        #region Input Operation
        if (Input.GetButtonDown("Jump"))
        {
            anim.SetTrigger("Jump");
            if(collision.onGround)
            {
                Jump(Vector2.up);
            }
            if(collision.onWall && !collision.onGround)
            {
                WallJump();
            }
        }

        if(Input.GetButtonDown("Fire1") && canDash)
        {
            if(xRaw!=0 || yRaw != 0)
            {
                Dash(xRaw, yRaw);
            }
        }
        #endregion

        if(collision.onGround && !groundTouch)
        {
            GroundTouch();
        }

        if(!collision.onGround && groundTouch)
        {
            groundTouch = false;

        }

        WallParticle(y);
        #region 调整主角朝向
        //当处于下滑状态时，取消横轴输入对人物朝向的控制
        if (wallSlide || wallGrab)
            return;

        if (x > 0)
        {
            side = 1;
            anim.SetFilp(side);
        }
        if(x < 0)
        {
            side = -1;
            anim.SetFilp(side);
        }
        #endregion
    }

    private void Walk(Vector2 dir)
    {
        if (wallGrab)
            return;

        if (!canMove)
            return;

        if(!wallJump)
        {
            rigid.velocity = new Vector2(dir.x * speed, rigid.velocity.y);
        }
        else
        {
            rigid.velocity = Vector2.Lerp(rigid.velocity, (new Vector2(dir.x * speed, rigid.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
        
    }

    private void Jump(Vector2 dir)
    {
        rigid.velocity = new Vector2(rigid.velocity.x, 0);
        rigid.velocity += dir * jumpForce;

        jumpParticle.Play();
    }

    private void WallJump()
    {
        if((collision.onRightWall && side == 1)||(collision.onLeftWall && side == -1))
        {
            side *= -1;
            anim.SetFilp(side);

        }
        StopCoroutine(DisableMoveMent(0));
        StartCoroutine(DisableMoveMent(.1f));

        Vector2 jumpDir = collision.onRightWall ? Vector2.left : Vector2.right;

        Jump(Vector2.up/1.5f + jumpDir/1.5f);

        wallJump = true;
    }

    IEnumerator DisableMoveMent(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    private void WallSlide()
    {
        if (!canMove)
            return;

        if(side != collision.colSide)
        {
            anim.SetFilp(-side);
        }

        bool pushingWall = false;
        if(rigid.velocity.x > 0 && collision.onRightWall || rigid.velocity.x < 0 && collision.onLeftWall)
        {
            pushingWall = true;
        }
        rigid.velocity = new Vector2(pushingWall ? 0 : rigid.velocity.x, -slideSpeed);
    }

    private void Dash(float x,float y)
    {
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);

        canDash = false;

        anim.SetTrigger("Dash");
        rigid.velocity = Vector2.zero;
        rigid.velocity += new Vector2(x, y).normalized * dashSpeed;
        StartCoroutine(DashEffect());
    }

    IEnumerator DashEffect()
    {
        StartCoroutine(DashOnGround());
        DOVirtual.Float(14, 0, .8f, SetRigidBodyDrag);
        FindObjectOfType<GhostTrailEffect>().ShowGhost();

        rigid.gravityScale = 0;
        GetComponent<BetterJump>().enabled = false;
        wallJump = true;
        isDashing = true;


        yield return new WaitForSeconds(.3f);

        rigid.gravityScale = 3;
        GetComponent<BetterJump>().enabled = true;
        isDashing = false;
        wallJump = false;
    }

    /// <summary>
    /// 冲刺未离开地面处理
    /// </summary>
    /// <returns></returns>
    IEnumerator DashOnGround()
    {
        yield return new WaitForSeconds(.15f);
        if (collision.onGround)
        {
            canDash = true;
        }
    }

    /// <summary>
    /// 设置拖拽力
    /// </summary>
    /// <param name="drag"></param>
    private void SetRigidBodyDrag(float drag)
    {
        rigid.drag = drag;
    }

    private void GroundTouch()
    {
        canDash = true;
        isDashing = false;
        groundTouch = true;
        wallJump = false;

        jumpParticle.Play();
    }

    private void WallParticle(float vertical)
    {
        var main = slideParticle.main;
        if(wallSlide || (wallGrab && vertical < 0))
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = Color.clear;
        }
    }

    private int ParticleSide()
    {
        return collision.onRightWall ? 1 : -1;
    }
}
