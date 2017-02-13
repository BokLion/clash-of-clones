﻿using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class CardChangedEvent : UnityEvent<int> {}

/// <summary>
/// Represents the state of the game with respect to the players' cards.
/// </summary>
public class PlayerCardModel : MonoBehaviour 
{
    public CardChangedEvent CardChangedEvent;
    public UnityEvent HandChangedEvent;

    public List<CardData> Deck;
    public List<CardData> Discard;

    [NonSerialized] public CardData[] Hand;

    void Awake()
    {
        Deck = new List<CardData>();
        Discard = new List<CardData>();
        Hand = new CardData[Consts.HandSize];
    }

    public void Init(List<CardData> deck)
    {
        // EARLY OUT! //
        if(deck == null || deck.Count == 0)
        {
            Debug.LogWarning("Can't do anything without a deck.");
            return;
        }
        
        // Clone the start deck so we can manipulate it.
        Discard = deck.ToList();
        Shuffle();
        initHand();
    }

    public void PlayCard(CardData card)
    {
        int index = Hand.IndexOf(card);
        if(index != -1)
        {
            replaceInHand(index, card);
        }
        else
        {
            Debug.LogWarning("Couldn't find card in hand: " + card.PrefabName);
        }
    }

    public CardData Peek()
    {
        if(Deck.Count == 0)
        {
            Shuffle();
        }

        return Deck.Last();
    }

    /// <summary>
    /// Get a random card from the hand.  Useful for basic AI card selection, not much else.
    /// </summary>
    public CardData GetRandomCardFromHand()
    {
        int rand = UnityEngine.Random.Range(0, Hand.Length);
        return Hand[rand];
    }

    /// <summary>
    /// Shuffle the discard pile and turn it into the draw pile.
    /// </summary>
    public void Shuffle()
    {
        //Debug.Log("Shuffling discard into deck, Count: " + Discard.Count);
        
        // Randomized shuffle.
        Deck = Discard.OrderBy(c => UnityEngine.Random.value).ToList();
        Discard.Clear();
    }

    private void initHand()
    {
        for(int i = 0; i < Hand.Length; i++)
        {
            if(Hand[i] != null)
            {
                Debug.LogWarning("You should only be filling an empty hand, but it had cards in it!");
                Discard.Add(Hand[i]);
            }

            Hand[i] = drawCard();
            CardChangedEvent.Invoke(i);
        }

        HandChangedEvent.Invoke();
    }

    private void replaceInHand(int handIndex, CardData newCard)
    {
        if(Hand[handIndex] != null)
        {
            Discard.Add(newCard);
        }
        Hand[handIndex] = drawCard();

        CardChangedEvent.Invoke(handIndex);
        HandChangedEvent.Invoke();
    }

    private CardData drawCard()
    {
        if(Deck.Count == 0)
        {
            Shuffle();
        }

        var top = Deck.Last();
        Deck.RemoveAt(Deck.Count - 1);
        return top;
    }
}
