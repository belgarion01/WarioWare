using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseDifficulty : MonoBehaviour
{
    public enum Difficulty { Facile, Moyen, Difficile }

    public MementoSavedData savedData;

    public void SetDifficulty(Difficulty difficulty)
    {
        savedData.difficulty = difficulty;
        SceneManager.LoadScene("Memento_1");
    }

    public void SetDifficulty(int difficulty)
    {
        savedData.difficulty = (Difficulty)difficulty;
        SceneManager.LoadScene("Memento_1");
    }
}
