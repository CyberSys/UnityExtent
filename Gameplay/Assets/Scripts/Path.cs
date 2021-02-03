using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path {

	public readonly List<Cell> lookPoints;
	public readonly Line[] turnBoundaries;
	public readonly int finishLineIndex;
	public readonly int slowDownIndex;

	public Path(List<Cell> waypoints, Vector3 startPos, float turnDst, float stoppingDst) {
		lookPoints = waypoints;
		turnBoundaries = new Line[lookPoints.Count];
		finishLineIndex = turnBoundaries.Length - 1;

		Vector2 previousPoint = V3ToV2 (startPos);
		for (int i = 0; i < lookPoints.Count; i++) {
			Vector2 currentPoint = V3ToV2 (lookPoints [i].centre);
			Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
			Vector2 turnBoundaryPoint = (i == finishLineIndex)?currentPoint : currentPoint - dirToCurrentPoint * turnDst;
			turnBoundaries [i] = new Line (turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);
			previousPoint = turnBoundaryPoint;
		}

		float dstFromEndPoint = 0;
		for (int i = lookPoints.Count - 1; i > 0; i--) {
			dstFromEndPoint += Vector3.Distance (lookPoints [i].centre, lookPoints [i - 1].centre);
			if (dstFromEndPoint > stoppingDst) {
				slowDownIndex = i;
				break;
			}
		}
	}

	Vector2 V3ToV2(Vector3 v3) {
		return new Vector2 (v3.x, v3.z);
	}

	public void DrawWithGizmos() {

		Gizmos.color = Color.black;
		foreach (Cell cell in lookPoints) {
			Gizmos.DrawCube (cell.centre + Vector3.up, Vector3.one);
		}

		Gizmos.color = Color.white;
		foreach (Line l in turnBoundaries) {
			l.DrawWithGizmos (10);
		}

	}

}
