using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateCommands : MonoBehaviour
{
    public GameObject commandInfosPrefab;
    public CommandInfosLibrary library;
    public MementoSavedData savedData;

    const float minTextWidth = 40f;
    const float maxTextWidth = 175f;

    public void InstantiateMultiple()
    {
        Clear();

        for (int i = 1; i < savedData.commands.Length; i++)
        {
            GameObject commandInfos = Instantiate(commandInfosPrefab, transform);
            Image arrowVisual = commandInfos.transform.GetChild(0).GetChild(0).GetComponent<Image>();
            RectTransform MainText = commandInfos.transform.GetChild(1).GetComponent<RectTransform>();
            RectTransform SecondaryText = commandInfos.transform.GetChild(2).GetComponent<RectTransform>();

            arrowVisual.sprite = library.GetCommandSprite(savedData.commands[i]);

            MainText.sizeDelta = new Vector2(Random.Range(minTextWidth, maxTextWidth), MainText.sizeDelta.y);
            SecondaryText.sizeDelta = new Vector2(Random.Range(minTextWidth, maxTextWidth), SecondaryText.sizeDelta.y);
        }
    }

    void Clear()
    {
        foreach (Transform child in transform) DestroyImmediate(child.gameObject);
    }
}
