using System.Collections;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public GameManager gameManager;
    public Grid grid;
    
    //Turn-Base vars
    public int totalMoves= 1;
    public int movesRemaining = 1;
    
    // Variables for moving the AI towards the target
    public Transform target; // Target object for the AI to move towards
    public float speed = 20; // Speed at which the AI moves
    private Vector3[] _path; // Array of waypoints to follow to reach the target
    private int _targetIndex; // Index of the current waypoint in the path

    private bool _coroutineCheck; // Check if a coroutine is currently running


    private void Update()
    {
        if (gameObject.CompareTag("Wolf"))
        {
            CheckCollision();
            //if bunny reached target and its inactive remove this wolf
            if (!target.gameObject.activeSelf)
            {
                gameManager.RemoveAIController(this);
                gameObject.SetActive(false);
            }
        }
        
        //check if its AI turn
        if (gameManager.currentTurn != GameManager.Turn.AI) return;
        //when AI turn, recalculate path
        if (_coroutineCheck) return;
        StartCoroutine(RunAStar());
        _coroutineCheck = true;
    }

    private void CheckCollision()
    {
        var distance = Vector3.Distance(transform.position, target.position);

        if (!(distance <= 5f)) return;
        Debug.Log("collided");
        //remove that bunny
        gameManager.RemoveAIController(target.gameObject.GetComponent<AIController>());
        target.gameObject.SetActive(false);

        //add score for player
        gameManager.wolfScore+= 1;
                
        //remove this wolf
        gameManager.RemoveAIController(this);
        gameObject.SetActive(false);
    }

    // Coroutine to calculate the path using A* algorithm
    private IEnumerator RunAStar()
    {
        // Create the grid for pathfinding
        grid.CreateGrid();
        yield return new WaitForSeconds(1);
        // Request the path from the PathRequestManager
        PathRequestManager.RequestPath(transform.position,target.position, OnPathFound);
        yield return new WaitForSeconds(1);
        // subtract one from the player's remaining moves
        movesRemaining--;
        _coroutineCheck = false;
    }
    
    // Callback function when the path is found
    private void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (!pathSuccessful) return;
        _path = newPath;
        _targetIndex = 0;
        StopCoroutine(nameof(FollowPath));
        StartCoroutine(nameof(FollowPath));
    }

    // Coroutine to move the AI along the path to the target
    private IEnumerator FollowPath() 
    {
        var currentWaypoint = _path[0];
        while (true) 
        {
            //if AI reached the next checkpoint
            if (transform.position == currentWaypoint) 
            {
                //_targetIndex ++; //turn this on to make the AI fallow path without stopping for turn
                if (_targetIndex >= _path.Length - 1) 
                {
                    //if AI reached final checkpoint
                    
                    //don't want the Wolf AI to disappear unless collided with bunny
                    if (gameObject.CompareTag("Bunny"))
                    {
                        gameManager.bunnyScore += 1;
                        
                        gameManager.RemoveAIController(this);
                        gameObject.SetActive(false);
                    }
                    break;
                }
                currentWaypoint = _path[_targetIndex];
            }
            transform.position = Vector3.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);
            yield return null;
        }
    }

    // Draw the path in the scene
    public void OnDrawGizmos()
    {
        if (_path == null) return;
        for (var i = _targetIndex; i < _path.Length; i ++) 
        {
            Gizmos.color = Color.black;
            Gizmos.DrawCube(_path[i], Vector3.one);
            Gizmos.DrawLine(i == _targetIndex ? transform.position : _path[i - 1], _path[i]);
        }
    }
}
