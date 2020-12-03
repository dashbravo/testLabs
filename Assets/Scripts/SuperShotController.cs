using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperShotController : ProjectileController
{
        
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Enemy" && startLife == true)
        {
            //am i supposed to use send message here?
           collision.gameObject.GetComponent<IEnemyBehavior>().DealDamageToEnemy(Damage, transform.position, new Vector2(2000, 100));
            //SendMessage("DealDamageToEnemy", 5) this causes error, wants all 3 parameters
        }
    }

    private void OnDestroy()
    {
        EventManager.UnPause();
    }


}
