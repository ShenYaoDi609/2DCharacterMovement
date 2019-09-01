using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GhostTrailEffect : MonoBehaviour
{
    private MoveMentController move;
    private AnimationMgr anim;
    public Color trailColor;
    public Color fadeColor;
    public float ghostInterval;
    public float fadeTime;

    void Start()
    {
        move = FindObjectOfType<MoveMentController>();
        anim = FindObjectOfType<AnimationMgr>();
    }

    public void ShowGhost()
    {
        Sequence s = DOTween.Sequence();

        for(int i = 0; i < transform.childCount; i++)
        {
            Transform curGhost = transform.GetChild(i);
            //设置Ghost的位置
            s.AppendCallback(() => { curGhost.position = move.transform.position; });
            //设置Ghost的朝向
            s.AppendCallback(() => { curGhost.GetComponent<SpriteRenderer>().flipX = anim.render.flipX; });
            s.AppendCallback(() => { curGhost.GetComponent<SpriteRenderer>().sprite = anim.render.sprite; });
            s.Append(curGhost.GetComponent<SpriteRenderer>().material.DOColor(trailColor, 0));
            s.AppendCallback(() => { FadeGhostColor(curGhost); });
            //间隔时间
            s.AppendInterval(ghostInterval);
        }
    }

    private void FadeGhostColor(Transform  trans)
    {
        trans.GetComponent<SpriteRenderer>().material.DOKill();
        trans.GetComponent<SpriteRenderer>().material.DOColor(fadeColor, fadeTime);
    }


}
