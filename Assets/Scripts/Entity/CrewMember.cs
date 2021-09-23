using UnityEngine;

[CreateAssetMenu(fileName = "CrewMember", menuName = "ScriptableObject/CrewMember")]
public class CrewMember : ScriptableObject
{
    public int id;
    public new string name;
    public Sprite graphics;

    public CrewMember(string name, Sprite graphics)
    {
        this.name = name;
        this.graphics = graphics;
    }

}
