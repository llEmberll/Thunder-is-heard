using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Core : MonoBehaviour
{
    [SerializeField] public Save saveData;
    [SerializeField] public SelectionManager UI;

    public int gameMode;
    public int timeInGame;


    private void Start()
    {
        LoadScene(saveData.scene);
    }


    private void LoadScene(int scene)
    {
        if (scene != SceneManager.GetActiveScene().buildIndex) SceneManager.LoadScene(scene);
    }


    void FixedUpdate()
    {
        timeInGame++;
    }
}
