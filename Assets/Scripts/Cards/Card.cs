using UnityEngine;

public class Card : MonoBehaviour 
{
	public GameObject sprite;
	public GameObject spriteSkinned;

	// Use this for initialization
	void Start () 
	{
			
	}

	// Update is called once per frame
	void Update () 
	{

	}
	
	public void Init(int number, CardSuit suit)
	{
		this.number = number;
		this.suit = suit;

		gameObject.name = suit+"_"+number;		
		transform.localEulerAngles = new Vector3(0, 0, 0);
	}
	
	bool picked = true;
	
	public void Pick(int indexInPile)
	{
		// TODO fix animation: at the moment z-index problem, if more than 3 cards
		if (indexInPile > 2)
			return;

		gameObject.layer = 12;
		PlayAnim("cardPick_01");
		picked = true;
	}
	
	public void Leave()
	{
		if (!picked)
			return;

		PlayAnim("cardLeave");
		picked = false;
		
	}
	
	private bool turned = true;

	public bool IsTurned()
	{
		return turned;
	}

	internal void Turn(bool faceDown)
    {
		this.turned = faceDown;

		int rot = faceDown ? 180 : 0;
		sprite.transform.localEulerAngles = new Vector3(0, rot, 0);	
    }

	internal void Turn(bool faceDown, float time)
	{
		this.turned = faceDown;

		if (!turned)
		{
			if (time > 0)
				PlayAnim("cardFlip_01");
			else
				sprite.transform.localEulerAngles = new Vector3(0, 0, 0);
		}

		pile.animTime = 0;

		Invoke("Align", time - .1f);
	}

	internal void Turn(bool faceDown, float time, int pos)
    {
		this.turned = faceDown;

		PlayAnim("cardFlip_01");		

		iTween.MoveBy(gameObject, iTween.Hash("x", -pos*.2, "z", -((pos*.1f) + time), "time", time, "isLocal", true));
    }
	
	void Align()
	{
		pile.AlignCards();
	}    

    public CardSuit suit;

    public CardSuit CardSuit
    {
        get { return suit; }
        set { suit = value; }
    }

    public CardPile pile = null;

    public CardPile Pile
    {
        get { return pile; }
        set { pile = value; }
    }

    public int number = 1;

    public int Number
    {
        get { return number; }
    }

    public bool IsRed
    {
        get { return (suit == CardSuit.Heart || suit == CardSuit.Diamond); }
    }

    public bool IsBlack
    {
        get { return !IsRed; }
    }
	
	public static bool IsDifferentSuit(Card card1, Card card2)
    {
        return ((card1.IsRed && card2.IsBlack) || ((card1.IsBlack && card2.IsRed)));
    }
	
	internal void Highlight()
	{
		spriteSkinned.GetComponent<Renderer>().materials[1].color = Color.yellow;
	}
	
	internal void Unhighlight()
	{
		spriteSkinned.GetComponent<Renderer>().materials[1].color = Color.white;
	}

	public void PlayAnim(string animName, float speed = 1, PlayMode playMode = PlayMode.StopAll)
    {
		Animation animation = sprite.GetComponent<Animation>();

		if (animation[animName] == null)
			return;

		animation[animName].speed = speed;
		animation.Play(animName, playMode);
	}
}


public enum CardSuit
{   
    Heart = 0,
    Spade = 1,
    Club = 2,
    Diamond = 3,
	None = 4
}