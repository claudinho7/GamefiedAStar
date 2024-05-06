using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public enum Turn {Player, AI}
    public Turn currentTurn;

    private PlayerController _playerController;
    private List<AIController> _aiControllers;

    public int wolfScore;
    public int bunnyScore;

    private void Start()
    {
        //start with Player turn
        currentTurn = Turn.Player;
        // get reference to PlayerController script
        _playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        // get references to AIController scripts
        _aiControllers = FindObjectsOfType<AIController>().ToList();
    }

    private void Update()
    {
        //check to see if game not over first
        if (wolfScore + bunnyScore >= 3) return;
        // check if current turn is over
        if (!CheckEndTurn()) return;
        // switch to the other player's turn
        currentTurn = (currentTurn == Turn.Player) ? Turn.AI : Turn.Player;
        Debug.Log("Current turn: " + currentTurn);
    }

    private bool CheckEndTurn()
    {
        switch (currentTurn)
        {
            // check if player has used up all their moves
            case Turn.Player when _playerController.movesRemaining > 0:
                return false;
            // end the player's turn
            case Turn.Player:
                _playerController.movesRemaining = _playerController.totalMoves; // reset moves
                return true;
            case Turn.AI:
            {
                var totalMovesUsedByAI = _aiControllers.Sum(aiController => aiController.totalMoves - aiController.movesRemaining);

                // check if all AI characters have used up all their moves
                if (totalMovesUsedByAI != _aiControllers.Count * _aiControllers[0].totalMoves) return false;
                {
                    // reset moves for all AI characters
                    foreach (var aiController in _aiControllers)
                    {
                        aiController.movesRemaining = aiController.totalMoves;
                    }

                    // end the AI's turn
                    return true;
                }
            }
            default:
                // turn not over yet
                return false;
        }
    }
    
    //call this when an AI gets disabled
    public void RemoveAIController(AIController aiController)
    {
        _aiControllers.Remove(aiController);
    }
}