﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Root object of all game-state.  Adding/removing entities, player managemment, lifecycle
/// should be done through here.  Although, perhaps we can break out lifecycle if it gets unweildy.
/// </summary>
public class GameState : MonoBehaviour 
{
    // Singleton instance.
    // Singletons suck, consider something more robust.  Can we have multiple game-states at a time?
    // Might be possible if we implement hosted multiplayer.
    public static GameState Instance;

    [SerializeField] private MessagePrinter _messagePrinter;
 
    [Range(0, 1)] 
    public int LocalPlayerNum;

    public float SecondsLeft;

    public Player LeftPlayer;
    public Player RightPlayer;

    /// <summary>
    /// Convenience array for iterating on players.
    /// </summary>
    [NonSerialized] public Player[] Players;

    /// <summary>
    /// If the game currently in progress?
    /// TODO: Add state machine for lifecycle management.
    /// </summary>
    public bool IsPlaying;

    /// <summary>
    /// The human controlled player.
    /// </summary>
    public Player MyPlayer
    {
        get
        {
            return Players[LocalPlayerNum];
        }
    }

    /// <summary>
    /// The AI.
    /// </summary>
    public Player EnemyPlayer
    {
        get
        {
            return Players[LocalPlayerNum == 0 ? 1 : 0];
        }
    }

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("More than once game state instance is running!  Destroying...");
            Destroy(this);
        }

        // EARLY OUT! //
        if (LeftPlayer == null || RightPlayer == null)
        {
            Debug.LogWarning("Need two players to have a working game state.");
            return;
        }

        var messageObj = GameObject.Find("CenterMessage");
        if(messageObj != null)
        {
            _messagePrinter = messageObj.GetComponent<MessagePrinter>();
            if(_messagePrinter != null)
            {
                _messagePrinter.PrintMessage("Clash of Clones");
            }
        }

        Players = new Player[2];
        Players[0] = LeftPlayer;
        Players[1] = RightPlayer;

        // Just use default decks for now.
        InitGame(TestFactory.CreateDeck(), TestFactory.CreateDeck());
    }

    public void InitGame(List<CardDefinition> leftDeck, List<CardDefinition> rightDeck)
    {
        LeftPlayer.Init("Computer", leftDeck);
        RightPlayer.Init("Alex", rightDeck);

        SecondsLeft = Consts.MatchStartSeconds;
        IsPlaying = true;
    }

    /// <summary>
    /// Get the player that is not the given player.
    /// </summary>
    public Player GetOppositePlayer(Player player)
    {
        Player opposite;
        if(MyPlayer == player)
        {
            opposite = EnemyPlayer;
        }
        else
        {
            opposite = MyPlayer;
        }
        return opposite;
    }

    void Update()
    {
        if(IsPlaying)
        {
            SecondsLeft -= Time.deltaTime;

            if(LeftPlayer.HQ.HP <= 0 || RightPlayer.HQ.HP <= 0 || SecondsLeft <= 0f)
            {
                IsPlaying = false;
                Player winner = determineWinner();

                // Print message.
                if(_messagePrinter != null)
                {
                    if(winner != null)
                    {
                        _messagePrinter.PrintMessage(winner.Name + " won!");
                    }
                    else
                    {
                        _messagePrinter.PrintMessage("Tie game!");
                    }

                    this.Invoke(restart, 3f);
                }
            }
        }
    }

    /// <summary>
    /// Return the winner.  If tie, returns null.
    /// </summary>
    private Player determineWinner()
    {
        int[] scores = new int[2];

        for(int i = 0; i < 2; i++)
        {
            var player = Players[i];
            if(player.HQ.HP > 0)
            {
                scores[i]++;

                if(player.TopOutpost.HP > 0)
                {
                    scores[i]++;
                }

                if(player.BottomOutpost.HP> 0)
                {
                    scores[i]++;
                }
            }
        }

        Player winner = null;

        if(scores[0] > scores[1])
        {
            winner = Players[0];
        }
        else if(scores[0] < scores[1])
        {
            winner = Players[1];
        }

        return winner;
    }

    // TODO: Play again? button
    private void restart()
    {
        SceneManager.LoadScene("ClashScene", LoadSceneMode.Single);
    }
}