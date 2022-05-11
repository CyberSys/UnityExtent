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
	public int agentID;
	public Vector3 startingMovementDirection;
	public List<Vector3> keyPatrolTargets;
	public Action<List<Cell>, bool> callback;

	public PathRequest(int _agentID, Vector3 _startingMovementDirection, List<Vector3> _keyPatrolTargets, Action<List<Cell>, bool> _callback)
	{
		agentID = _agentID;
		startingMovementDirection = _startingMovementDirection;
		keyPatrolTargets = _keyPatrolTargets;
		callback = _callback;
	}
}


