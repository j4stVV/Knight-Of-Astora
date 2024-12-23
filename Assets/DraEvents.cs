using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraEvents : MonoBehaviour
{
    private void Hit(Transform attackTransform, Vector2 attackArea)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(attackTransform.position, attackArea, 0);
        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<PlayerController>() != null) 
            {
                objectsToHit[i].GetComponent<PlayerController>().TakeDamage(BossScript.instance.damage);
            }
        }
    }

    void BendDownCheck()
    {
        if (BossScript.instance.fireballAttack)
            StartCoroutine(BarrageAttackTransition());
    }
    void BarrageOrOutBreak()
    {
        if (BossScript.instance.fireballAttack)
        {
            BossScript.instance.StartCoroutine(BossScript.instance.Barrage());
        }
    }
    IEnumerator BarrageAttackTransition()
    {
        yield return new WaitForSeconds(1f);
        BossScript.instance.anim.SetBool("Cast", true);
    }
}
