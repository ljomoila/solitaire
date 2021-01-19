using UnityEngine;

public class SelectionPile : CardPile
{
	public GameObject mover;
	
	// Use this for initialization
	void Start () 
	{
		Type = CardPileType.SelectionPile;		
		cardZ = GameManager.Instance.deckMaker.cardZ*10;
		//yStep = .75f;
	}
	
	public override void MakeDragGroup()
	{
        if (mover == null)
        {
            mover = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mover.gameObject.name = "mover";
            mover.GetComponent<Renderer>().enabled = false;
            mover.GetComponent<Collider>().enabled = false;
            mover.transform.parent = transform;
            Rigidbody rb = mover.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        Vector3 pos = cards[0].transform.localPosition;
		pos.y += 1.5f;
		
		mover.transform.localPosition = pos;

		for (int i = 0; i < cards.Count; i++)
		{			
			HingeJoint hj = cards[i].gameObject.AddComponent<HingeJoint>();			
			hj.connectedBody = i == 0 ? mover.GetComponent<Rigidbody>() : cards[i - 1].GetComponent<Rigidbody>();

			cards[i].GetComponent<Rigidbody>().isKinematic = false;			
			cards[i].gameObject.layer = 12; // TODO name for the layer
			
			if (i > 0)
				iTween.MoveBy(cards[i].gameObject, iTween.Hash("y", .5f, "time", .25f, "isLocal", true));			
		}

	}
	
	// Update is called once per frame
	void Update () 
	{

	}
}
	
	