﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public enum OPTIONS
{
    Hauberk,
    Cave,
    Debug
}

public class Main : EditorWindow
{
    // Generall Stuff
    private Vector2 scrollPos;
    private Vector2 mapSize;
    public OPTIONS op;
    private string seed;
    private bool useRandomSeed = false;
    private string debugPhrase;
    private Transform tilePrefab;
    
    // Hauberk
    private string objectName = "level001";
    private bool rooms = true;
	private float outlinePercent;
    private int randomFillPercent;
    private int width;
    private int height;
    private int attempts;
    
    [MenuItem("Window/LevelGen/Ocean")]
    public static void ShowWindow(){
        EditorWindow.GetWindow(typeof(Main));
    }

    void OnGUI()
    {
        GUILayout.Label("Output Settings for Level Generation", EditorStyles.boldLabel);
        objectName = EditorGUILayout.TextField("Object Name: ", objectName);
        op = (OPTIONS)EditorGUILayout.EnumPopup("Level style:", op);
        DoGUI(op);
        useRandomSeed = EditorGUILayout.Toggle("Use random seed", useRandomSeed);
        EditorGUI.BeginDisabledGroup(useRandomSeed);
        seed = EditorGUILayout.TextField("Seed: ", seed);
        EditorGUI.EndDisabledGroup();
    
        if (GUILayout.Button("Generate"))
        {
            generate(op);
        }
    }

    private void DoGUI(OPTIONS op)
    {
        if (op == OPTIONS.Hauberk)
        {
            GUILayout.Label("Hauberk Map Generation Settings", EditorStyles.boldLabel);
            mapSize = EditorGUILayout.Vector2Field("Size:", mapSize);
            tilePrefab = EditorGUILayout.ObjectField("Quad Wall Prefab", tilePrefab, typeof(Transform), true) as Transform;
            rooms = EditorGUILayout.Toggle("Add rooms", rooms);
            attempts = EditorGUILayout.IntField("Attempts", attempts);
			outlinePercent = EditorGUILayout.Slider("Outline Percent:",outlinePercent,0,1);
        }
        if (op == OPTIONS.Cave)
        {
            GUILayout.Label("Cave Generation Settings", EditorStyles.boldLabel);
            width = EditorGUILayout.IntField("Width:", width);
            height = EditorGUILayout.IntField("Height:", height);
			randomFillPercent = EditorGUILayout.IntSlider("Random Fill Percent:",randomFillPercent,0,100);
        }
        if (op == OPTIONS.TEST) {
            GUILayout.Label("Debug", EditorStyles.boldLabel);
            debugPhrase = EditorGUILayout.TextField("Debug Phrase: ", debugPhrase);
        }
    }

    void generate(OPTIONS op)
    {
        if (GameObject.Find(objectName) != null)
        {
            EditorUtility.DisplayDialog("Error", "Object "+objectName+" already exists", "Ok","");
        }
        else
        {
			Transform level = new GameObject(objectName).transform;
            if (useRandomSeed)
            {
                seed = Time.time.ToString();
            }
			System.Random rnd = new System.Random (seed.GetHashCode());
            switch (op){
				case OPTIONS.Hauberk:
					Hauberk h = GameObject.Find (objectName).AddComponent<Hauberk> ();
					h.addRooms (rooms);
					h.setSeed (rnd);
					h.setIsRandomSeed (useRandomSeed);
					h.GenerateLevel(mapSize, level, tilePrefab, outlinePercent, true, attempts);
                    break;
                case OPTIONS.Cave:
                    Debug.Log(OPTIONS.Cave +" doesn't work right now...");
					/*Cave c = GameObject.Find (objectName).AddComponent<Cave>();
                    c.setSeed(rnd);
					c.setIsRandomSeed (useRandomSeed);
                    c.GenerateLevel(level, randomFillPercent, width, height);#*/
					break;
                case OPTIONS.TEST:
                    Debug.Log("Testing...");
                    Debug.Log(System.IO.Directory.GetCurrentDirectory());
                    break;
                default:
                    Debug.LogError("Unrecognized Option");
                    break;
            }
        }
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

}