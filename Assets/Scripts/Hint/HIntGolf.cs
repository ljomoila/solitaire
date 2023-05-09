using System.Collections.Generic;

public class HintGolf : HintState
{
    public override void TryShowingHint()
    {
        foreach (CardPile pile in activeGame.TableauPiles)
        {
            if (activeGame.TryMoveToPile(pile))
            {
                ShowHint(pile.GetLastCard());
                break;
            }
        }
    }
}
