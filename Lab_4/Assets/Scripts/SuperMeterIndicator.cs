using UnityEngine;
using UnityEngine.UI;

public class SuperMeterIndicator : MonoBehaviour
{

    [SerializeField]
    PlayerController player;
    [SerializeField]
    Color[] chargeColors;

    // Update is called once per frame
    void Update()
    {
        if (player.GetSuperMeter() == 100)
            GetComponent<Image>().color = chargeColors[Random.Range(0, chargeColors.Length)];
        else
            GetComponent<Image>().color = Color.white;
    }
}
