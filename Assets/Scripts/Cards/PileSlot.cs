using UnityEngine;

public class PileSlot : MonoBehaviour
{
    public PileType pileType;
    public Card card;

    public SpriteRenderer border;

    private MaterialPropertyBlock propertyBlock;

    void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        Unhighlight();
    }

    public void Initialize(CardPile pile)
    {
        gameObject.SetActive(true);

        pileType = pile.Type;
        card.pile = pile;
    }

    internal void Unhighlight()
    {
        SetColor(.3f);
    }

    internal void Highlight()
    {
        SetColor(1f);
    }

    private void SetColor(float alpha)
    {
        Color c = border.color;
        c.a = alpha;

        border.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", c);
        border.SetPropertyBlock(propertyBlock);
    }
}
