using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class changeScene : MonoBehaviour
{
    Scene scene;
    string scene1 = "UndergroundTemple";
    string scene2 = "Menu";
    // Start is called before the first frame update

    private void Start()
    {
       scene = SceneManager.GetActiveScene();
    }
    public void newScene()
    {
        if (scene.name == scene1)
            SceneManager.LoadScene(scene2);
        else SceneManager.LoadScene(scene1);
    }
}
