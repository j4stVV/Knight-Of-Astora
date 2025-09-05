using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraEvents : Enemy
{
    void BendDownCheck()
    {
        if (BossScript.instance.fireballAttack || BossScript.instance.tripleAttack)
            StartCoroutine(BarrageAttackTransition());
    }
    void BarrageOrOutBreak()
    {
        if (BossScript.instance.fireballAttack)
        {
            BossScript.instance.StartCoroutine(BossScript.instance.Barrage());
        }
        else if (BossScript.instance.tripleAttack)
        {
            BossScript.instance.StartCoroutine(BossScript.instance.TripleBarrage());
        }
        
    }
    IEnumerator BarrageAttackTransition()
    {
        yield return new WaitForSeconds(1f);
        anim.SetBool("Cast", true);
    }
    void DestroyAfterDeath()
    {
        BossScript.instance.DestroyAfterDeath();
    }
}
