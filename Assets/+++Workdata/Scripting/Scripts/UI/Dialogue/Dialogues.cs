using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Dialogues
{
    [Header("Situation")]
    [SerializeField] private string situation;
    
    [TextArea(3, 10)] public List<string> dialogues = new();
}