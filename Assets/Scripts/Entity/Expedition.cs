using System.Collections.Generic;

public class Expedition
{
    public string name;
    public float timeScheduled;
    private float timeElapsed = 0;
    private float timeElapsedSinceLastEvent = 0;
    public int distance;
    public Location location;
    public List<CrewMember> crewMembers;
    public List<Item> items;
    public List<Event> events;
    public float efficiency = 1f;

    public Expedition()
    {
        this.items = new List<Item>();
        this.crewMembers = new List<CrewMember>();
        this.timeScheduled = 10000;
    }

    public override string ToString()
    {
        return "\"" + name + "\" (" + location.name + ") : " + string.Join("\n", items); 
    }

    public void UpdateTimeElapsed(float deltaTimeElapsed)
    {
        this.timeElapsed += deltaTimeElapsed;
        this.timeElapsedSinceLastEvent += deltaTimeElapsed;
    }

    public float TimeElapsed => timeElapsed;
    public float TimeElapsedSinceLastEvent
    {
        get => timeElapsedSinceLastEvent;
        set => timeElapsedSinceLastEvent = value;
    }
}
