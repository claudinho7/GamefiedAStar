using System;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject turnText;
    public GameObject wolfScoreText;
    public GameObject bunnyScoreText;
    public GameObject winScore;
    public GameObject helpText;
    public PlayerController playerController;

    private TextMeshProUGUI _textMeshPro;
    private TextMeshProUGUI _wolfScore;
    private TextMeshProUGUI _bunnyScore;
    private TextMeshProUGUI _winScore;
    private TextMeshProUGUI _helpText;

    private bool _isSpawning, _isMovingObstacle, _isMovingEndZone;

    private void Start()
    {
        _textMeshPro = turnText.GetComponent<TextMeshProUGUI>();
        _wolfScore = wolfScoreText.GetComponent<TextMeshProUGUI>();
        _bunnyScore = bunnyScoreText.GetComponent<TextMeshProUGUI>();
        _winScore = winScore.GetComponent<TextMeshProUGUI>();
        _helpText = helpText.GetComponent<TextMeshProUGUI>();
        winScore.SetActive(false);
    }

    private void Update()
    {
        _textMeshPro.text = gameManager.currentTurn + "'s Turn";
        _wolfScore.text = "Wolves Score: " + gameManager.wolfScore;
        _bunnyScore.text = "Bunnies Score: " + gameManager.bunnyScore;

        //switch the helper text depending on game state
        switch (gameManager.currentTurn)
        {
            case GameManager.Turn.AI:
                _helpText.text = "AI is moving, Wait";
                _isMovingObstacle = false;
                _isMovingEndZone = false; 
                _isSpawning = false;
                break;
            case GameManager.Turn.Player when _isSpawning:
                _helpText.text = "Click everywhere on the map to spawn an Object. This will block AI's path.";
                break;
            case GameManager.Turn.Player when _isMovingObstacle:
                _helpText.text = "Use your mouse to grab and move one of the Terrain pieces on the ground and release it to a different place. This will make the rabbit change his pathing.";
                break;
            case GameManager.Turn.Player when _isMovingEndZone:
                _helpText.text = "Use your mouse to grab and move one of the Rabbit's finish zone, and release it to a different place. This will make the rabbit change his pathing.";
                break;
            case GameManager.Turn.Player:
                _helpText.text = "It's your turn, chose a action by clicking a button";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if (gameManager.wolfScore + gameManager.bunnyScore < 3) return;
        winScore.SetActive(true);
        _winScore.text = gameManager.wolfScore > gameManager.bunnyScore ? "You Won" : "You Lost";
    }
    
    public void OnSpawnObjectButtonClicked()
    {
        playerController.currentAction = PlayerController.PlayerAction.SpawnObject;
        _isSpawning = true;
    }

    public void OnActionMoveObstaclesClicked()
    {
        playerController.currentAction = PlayerController.PlayerAction.MoveObstacles;
        _isMovingObstacle = true;
    }

    public void OnActionMoveFinishZoneClicked()
    {
        playerController.currentAction = PlayerController.PlayerAction.MoveFinishZone;
        _isMovingEndZone = true;
    }

    public void OnQuitButtonClicked()
    {
        Debug.Log("Game Closed");
        Application.Quit();
    }

    public void OnResetButtonClicked()
    {
        Debug.Log("Game Restarted");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
