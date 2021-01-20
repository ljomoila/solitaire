using UnityEngine;

public class SelectionPile : CardPile
{
	public GameObject mover;
	
	// Use this for initialization
	void Start () 
	{
		Type = CardPileType.SelectionPile;		
		cardZ = .75f;
	}
	// Update is called once per frame
	void Update () 
	{

	}
}
	
	