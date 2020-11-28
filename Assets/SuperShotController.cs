using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperShotController : ProjectileController
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Enemy" && startLife == true)
        {
            gameObject.SendMessage("DealDamageToEnemy", (Damage, transform.position, new Vector2(2000, 100)));
        }
    }
}
