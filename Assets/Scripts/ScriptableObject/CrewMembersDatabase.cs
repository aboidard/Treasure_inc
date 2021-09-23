using UnityEngine;
using System.Linq;

public class CrewMembersDatabase : MonoBehaviour
{
    public CrewMember[] allCrewMembers;
    public static CrewMembersDatabase instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }
    public static CrewMember GetRandomCrewMember()
    {
        CrewMember crew = CrewMembersDatabase.instance.allCrewMembers.Single(x => x.id == 1);
        return crew;
    }
}
