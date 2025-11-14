using UnityEngine;

[CreateAssetMenu(fileName = "InstructionSnippet", menuName = "Scriptable Objects/InstructionSnippet")]
public class InstructionSnippet : ScriptableObject
{
    public string codeSnippet;
    public bool[] heatPattern = new bool[5]; // Array to check which phases are getting heated
}
