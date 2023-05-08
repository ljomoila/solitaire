using UnityEngine;

[ExecuteInEditMode]
public class DeckMaker : MonoBehaviour
{
    public bool makeDeck = false;
    public int numberOfCards = 52;
    public Card card;
    public CardPile deck;

    public Game game;

    float yStep = 0.1535f;
    float xStep = 0.06257325f;

    private Vector2[] cardTextureOffsets = new Vector2[52];

    // Use this for initialization
    void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (makeDeck)
        {
            CardPile newDeck = new GameObject(game.name + "Deck").AddComponent<CardPile>();
            newDeck.transform.parent = game.transform;
            //newDeck.transform.localPosition = game.stock.transform.localPosition;
            newDeck.Type = PileType.Stock;

            card.gameObject.SetActive(true);

            int xIndex = 0;
            int yIndex = 0;
            float zPos = 0;

            for (int i = 0; i < numberOfCards; i++)
            {
                int suitIndex = i / 13;
                int numberIndex = i % 13 + 1;

                Card newCard = Instantiate(
                    card,
                    Vector3.zero,
                    Quaternion.identity,
                    newDeck.transform
                );
                newCard.Initialize(numberIndex, (Suit)suitIndex);
                newCard.pile = newDeck;
                newCard.transform.Translate(0, 0, zPos);

                if (yIndex == 6)
                {
                    yIndex = 0;
                    xIndex++;
                }

                if (suitIndex == 2 && numberIndex == 1)
                    yIndex = 3;

                cardTextureOffsets[i] = new Vector2(xIndex * xStep, yIndex * yStep);
                // TODO fix texture instead of this ugly thing
                // TODO fix ace of clubs in texture
                Vector2 textureOffset =
                    i > 0 ? cardTextureOffsets[i - 1] : new Vector2(0.500586f, 0.614f);
                newCard.spriteSkinned.GetComponent<Renderer>().materials[1].SetTextureOffset(
                    "_MainTex",
                    textureOffset
                );

                newDeck.cards.Add(newCard);

                yIndex++;
                zPos -= .015f;
            }

            card.gameObject.SetActive(false);

            //if (game.stock != null)
            //	DestroyImmediate(game.stock.gameObject);

            game.stock = newDeck;

            makeDeck = false;
        }
    }
}
