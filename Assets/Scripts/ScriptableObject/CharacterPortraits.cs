using UnityEngine;

/// <summary>
/// The portraits of the characters that are not in the TeamPanel of the scene (see CharacterArt): portraits[0] is
/// the character 4, the first one after the three every user starts with. Loaded from Resources/CharacterPortraits;
/// each portrait is the front frame of the sprite sheet of the character, in import/crew.
/// </summary>
[CreateAssetMenu(fileName = "CharacterPortraits", menuName = "ScriptableObject/CharacterPortraits")]
public class CharacterPortraits : ScriptableObject
{
    public Sprite[] portraits;
}
