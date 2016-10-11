﻿using UnityEngine;

/// <summary>
/// Keeps the hand display up to date.
/// </summary>
public class CardPanel : MonoBehaviour 
{
    public Card[] Hand;
    public Card PeekCard;

    void Update()
    {
        var handState = GameState.Instance.MyPlayer.CardState.Hand;

        // EARLY OUT! //
        if(handState.Length != Hand.Length)
        {
            Debug.LogWarning("Hand length and card UI are not the same length!: " + handState.Length + "-" + Hand.Length);
            return;
        }

        // Hmm maybe convert this to something more event driven?
        for(int i = 0; i < Hand.Length; i++)
        {
            var visibleCard = Hand[i].Definition;
            var handCard = handState[i];

            if(visibleCard != handCard)
            {
                //Change card.
                Hand[i].Init(handCard);
            }
        }

        var peekCard = GameState.Instance.MyPlayer.CardState.Peek();
        if(peekCard != null)
        {
            PeekCard.Init(peekCard);
        }
    }
}