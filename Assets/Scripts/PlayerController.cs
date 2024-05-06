using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject obstacle;
    public GameManager gameManager;
    
    //Turn Base vars
    public int totalMoves= 1;
    public int movesRemaining = 1;
    
    //Action Selector vars
    public enum PlayerAction { Unassigned, SpawnObject, MoveObstacles, MoveFinishZone}
    public PlayerAction currentAction = PlayerAction.Unassigned;
    private GameObject _selectedObject;



    private void Update()
    {
        //check if its Player turn and if he chose an action with the buttons
        if (gameManager.currentTurn != GameManager.Turn.Player && currentAction == PlayerAction.Unassigned) return;

        //switch between available Actions
        switch (currentAction)
        {
            case PlayerAction.SpawnObject:
                SpawnObject();
                break;
            case PlayerAction.MoveObstacles:
                MoveObstacle();
                break;
            case PlayerAction.MoveFinishZone:
                MoveFinishZone();
                break;
            case PlayerAction.Unassigned:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    //execute method when player clicks on spawn button
    private void SpawnObject()
    {
        if (!Input.GetMouseButtonDown(0) || movesRemaining <= 0) return;
        // detect the position of the ground object where the player clicked
        if (Camera.main == null) return;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit) || !hit.collider.CompareTag("Ground")) return;
        // spawn the object on top of the ground
        var spawnPosition = hit.point + new Vector3(0f, obstacle.transform.localScale.y / 2f, 0f);
        Instantiate(obstacle, spawnPosition, Quaternion.identity);
        
        // subtract one from the player's remaining moves
        movesRemaining--;
        //reset the selected action
        currentAction = PlayerAction.Unassigned;
    }

    //execute method when player clicks on move obstacle button
    private void MoveObstacle()
    {
        if (Camera.main == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit) && hit.collider.gameObject.CompareTag("Movable"))
        {
            // First click -> select object
            if (Input.GetMouseButtonDown(0) && _selectedObject == null)
            {
                _selectedObject = hit.transform.gameObject;
            }
            // button stays pressed > move object
            else if (_selectedObject != null && Input.GetMouseButton(0))
            {
                var newPosition = hit.point;
                newPosition.y = _selectedObject.transform.position.y; // keep the same height
                _selectedObject.transform.position = newPosition;
            }
        }
        // Release object when mouse button is released
        if (_selectedObject == null || !Input.GetMouseButtonUp(0)) return;
        _selectedObject = null;
        //reset currentAction
        currentAction = PlayerAction.Unassigned;
        // subtract one from the player's remaining moves
        movesRemaining--;
    }

    //execute method when player clicks on move finish zone button
    private void MoveFinishZone()
    {
        if (Camera.main == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit) && hit.collider.gameObject.CompareTag("FinishZone"))
        {
            // First click -> select object
            if (Input.GetMouseButtonDown(0) && _selectedObject == null)
            {
                _selectedObject = hit.transform.gameObject;
            }
            // button stays pressed > move object
            else if (_selectedObject != null && Input.GetMouseButton(0))
            {
                var newPosition = hit.point;
                newPosition.y = _selectedObject.transform.position.y; // keep the same height
                _selectedObject.transform.position = newPosition;
            }
        }
        // Release object when mouse button is released
        if (_selectedObject == null || !Input.GetMouseButtonUp(0)) return;
        _selectedObject = null;
        //reset currentAction
        currentAction = PlayerAction.Unassigned;
        // subtract one from the player's remaining moves
        movesRemaining--;
    }
}