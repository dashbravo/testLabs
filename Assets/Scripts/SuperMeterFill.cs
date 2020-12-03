using UnityEngine;
using UnityEngine.UI;

public class SuperMeterFill : MonoBehaviour
{

    [SerializeField]
    PlayerController player;
    [SerializeField]
    Color[] chargeColors;

    // Update is called once per frame
    void Update()
    {
        float superMeter = player.GetSuperMeter();
        Vector3 scale = GetComponent<RectTransform>().localScale;
        scale.y = superMeter / 100.0f;
        GetComponent<RectTransform>().localScale = scale;
        if (superMeter == 100)
        {
            // taste the rainbow
            GetComponent<Image>().color = chargeColors[Random.Range(0, chargeColors.Length)];
        }
        else
            GetComponent<Image>().color = Color.red;
    }
}
