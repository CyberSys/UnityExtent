using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class PathRequestManager : MonoBehaviour {

	Queue<PathResult> results = new Queue<PathResult>();

	static PathRequestManager instance;
	Pathfinding pathfinding;

	void Awake() {
		instance = this;
		pathfinding = GetComponent<Pathfinding>();
	}

	void Update() {
		if (results.Count > 0) {
			int itemsInQueue = results.Count;
			lock (results) {
				for (int i = 0; i < itemsInQueue; i++) {
					PathResult result = results.Dequeue ();
					result.callback (result.pathCells, result.success);
				}
			}
		}
	}

	public static void RequestPath(PathRequest request) {
		ThreadStart threadStart = delegate {
			instance.pathfinding.FindPath (request, instance.FinishedProcessingPath);
		};
		threadStart.Invoke ();
	}

	public void FinishedProcessingPath(PathResult result) {
		lock (results) {
			results.Enqueue (result);
		}
	}



}

public struct PathResult {
	public List<Cell> pathCells;
	public bool success;
	public Action<List<Cell>, bool> callback;

	public PathResult (List<Cell> pathCells, bool success, Action<List<Cell>, bool> callback)
	{
		this.pathCells = pathCells;
		this.success = success;
		this.callback = callback;
	}

}

public struct PathRequest
{
	public Vector3 currentMovementDirection;
	public Vector3 pathStart;
	public Vector3 pathEnd;
	public Action<List<Cell>, bool> callback;

	public PathRequest(Vector3 _currentMovementDirection, Vector3 _start, Vector3 _end, Action<List<Cell>, bool> _callback)
	{
		currentMovementDirection = _currentMovementDirection;
		pathStart = _start;
		pathEnd = _end;
		callback = _callback;
	}

}
