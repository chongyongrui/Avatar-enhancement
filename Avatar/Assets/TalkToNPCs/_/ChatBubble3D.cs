﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatBubble3D : MonoBehaviour {
    public static GameObject chatbubblePrefab; // Renamed to chatbubblePrefab for clarity

    public static void Create(Transform parent, Vector3 localPosition, IconType iconType, string text) {
        // Instantiate the chatbubblePrefab and parent it to the given parent transform
        GameObject chatBubbleGameObject = Instantiate(chatbubblePrefab, parent);
        Transform chatBubbleTransform = chatBubbleGameObject.transform;
        chatBubbleTransform.localPosition = localPosition;

        // Get the ChatBubble3D component from the instantiated game object and set it up
        ChatBubble3D chatBubble = chatBubbleGameObject.GetComponent<ChatBubble3D>();
        chatBubble.Setup(iconType, text);

        // Destroy the chat bubble game object after 6 seconds
        Destroy(chatBubbleGameObject, 6f);
    }



    public enum IconType {
        Happy,
        Neutral,
        Angry,
    }

    [SerializeField] private Sprite happyIconSprite = null;
    [SerializeField] private Sprite neutralIconSprite = null;
    [SerializeField] private Sprite angryIconSprite = null;

    private SpriteRenderer backgroundSpriteRenderer;
    private Transform backgroundCube;
    private SpriteRenderer iconSpriteRenderer;
    private TextMeshPro textMeshPro;

    private void Awake() {
        backgroundSpriteRenderer = transform.Find("Background").GetComponent<SpriteRenderer>();
        backgroundCube = transform.Find("BackgroundCube");
        iconSpriteRenderer = transform.Find("Icon").GetComponent<SpriteRenderer>();
        textMeshPro = transform.Find("Text").GetComponent<TextMeshPro>();
    }

    private void Setup(IconType iconType, string text) {
        textMeshPro.SetText(text);
        textMeshPro.ForceMeshUpdate();
        Vector2 textSize = textMeshPro.GetRenderedValues(false);

        Vector2 padding = new Vector2(7f, 3f);
        backgroundSpriteRenderer.size = textSize + padding;
        backgroundCube.localScale = textSize + padding * .5f;

        Vector3 offset = new Vector3(-3f, 0f);
        backgroundSpriteRenderer.transform.localPosition = 
            new Vector3(backgroundSpriteRenderer.size.x / 2f, 0f) + offset;
        backgroundCube.localPosition =
            new Vector3(backgroundSpriteRenderer.size.x / 2f, 0f, +.1f) + offset;

        iconSpriteRenderer.sprite = GetIconSprite(iconType);

    }

    private Sprite GetIconSprite(IconType iconType) {
        switch (iconType) {
            default:
            case IconType.Happy:    return happyIconSprite;
            case IconType.Neutral:  return neutralIconSprite;
            case IconType.Angry:    return angryIconSprite;
        }
    }

}
