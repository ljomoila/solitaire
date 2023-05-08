public class SelectionPile : CardPile
{
    public CardPile sourcePile;

    // Use this for initialization
    void Start()
    {
        Type = PileType.SelectionPile;
        CardZ = .75f;
    }

    // Update is called once per frame
    void Update() { }
}
