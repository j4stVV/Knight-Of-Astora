using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossScript : Enemy
{


    protected override void Start()
    {
        
    }
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Update()
    {
        base.Update();
        if (!isRecoiling)
        {
            transform.position = Vector2.MoveTowards
                (transform.position,
                new Vector2(PlayerController.Instance.transform.position.x, transform.position.y),
                speed * Time.deltaTime);
        }
    }
    public void CastSpell()
    {

    }

}
