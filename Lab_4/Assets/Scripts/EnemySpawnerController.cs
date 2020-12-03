using System.Collections.Generic;
using UnityEngine;


public class EnemySpawnerController : MonoBehaviour
{
    #region Editable fields

    [SerializeField]
    int AmountToSpawn = 1;
    [SerializeField]
    Transform Camera;
    [SerializeField]
    GameObject EnemyToSpawn;

    #endregion

    #region Read-only fields

    #endregion

    #region Inaccessible fields

    List<GameObject> objects;

    #endregion

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, .5f);
    }

    // Use this for initialization
    void Awake()
    {
        objects = new List<GameObject>(AmountToSpawn);
        for(int i = 0; i < AmountToSpawn; ++i)
        {
            objects.Add(Instantiate(EnemyToSpawn));
            objects[i].transform.parent = gameObject.transform;
            objects[i].GetComponent<IEnemyBehavior>().SetCameraReference(Camera);
            objects[i].SetActive(false);
        }
    }

    void InitEnemy(GameObject _enemy)
    {
        IEnemyBehavior behavior = _enemy.GetComponent<IEnemyBehavior>();
        behavior.SetFaceRight(Random.Range(0, 1) >= .5f);
        float radians = Random.Range(0, Mathf.PI * 2);
        _enemy.transform.localPosition = new Vector3(Mathf.Cos(radians)*.5f, Mathf.Sin(radians)*.5f, 0);
        _enemy.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D _other)
    {
        foreach (GameObject go in objects)
        {
            if (go.activeInHierarchy == false)
            {
                InitEnemy(go);
            }
        }
    }
}
