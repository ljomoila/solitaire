public class SelectionPile : CardPile
{
    public CardPile sourcePile;

    void Start()
    {
        Type = PileType.SelectionPile;
        CardZ = .75f;
    }
}
