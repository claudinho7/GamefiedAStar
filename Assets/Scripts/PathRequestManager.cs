//This script manages the queue of path requests and sends them to the AStar script for pathfinding. When a new path request is received, it is added to the queue,
//and if the queue is not empty and there is no path currently being processed, the next path request in the queue is processed.
//When a path is finished being processed, the result is sent back to the requester through a callback function.

using UnityEngine;
using System.Collections.Generic;
using System;

public class PathRequestManager : MonoBehaviour 
{
    // Queue to store path requests
    private Queue<PathRequest> _pathRequestQueue = new Queue<PathRequest>();
    // Current path request being processed
    private PathRequest _currentPathRequest;
    // Reference to AStar class for pathfinding
    private AStar _pathfinding;
    // Check to indicate if a path is currently being processed
    private bool _isProcessingPath;

    // Singleton instance
    private static PathRequestManager _instance;

    private void Awake() 
    {
        _instance = this;
        _pathfinding = GetComponent<AStar>();
    }

    // Method to request a path from the PathRequestManager
    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback) 
    {
        // Create a new path request object
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
        // Enqueue the new request to the path request queue
        _instance._pathRequestQueue.Enqueue(newRequest);
        // Try to process the next path request in the queue
        _instance.TryProcessNext();
    }

    // Method to check if there is a path request waiting to be processed, and if so, process it
    private void TryProcessNext()
    {
        // If a path is currently being processed or the path request queue is empty, do nothing
        if (_isProcessingPath || _pathRequestQueue.Count == 0) 
        {
            return;
        }
        // Dequeue the next path request from the queue
        _currentPathRequest = _pathRequestQueue.Dequeue();
        // Set the check to indicate a path is being processed
        _isProcessingPath = true;
        // Call the AStar class to start finding a path
        _pathfinding.StartFindPath(_currentPathRequest.PathStart, _currentPathRequest.PathEnd);
    }

    // Method called by AStar when a path has been found or the pathfinding failed
    public void FinishedProcessingPath(Vector3[] path, bool success) 
    {
        // Call the callback function associated with the current path request
        _currentPathRequest.Callback(path, success);
        // Set the flag to indicate the pathfinding is no longer processing a path
        _isProcessingPath = false;
        // Try to process the next path request in the queue
        TryProcessNext();
    }

    // Struct to store a single path request
    private struct PathRequest 
    {
        public readonly Vector3 PathStart;
        public readonly Vector3 PathEnd;
        public readonly Action<Vector3[], bool> Callback;

        public PathRequest(Vector3 start, Vector3 end, Action<Vector3[], bool> callback) 
        {
            PathStart = start;
            PathEnd = end;
            Callback = callback;
        }
    }
}