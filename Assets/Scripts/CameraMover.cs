using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Enums;
using GameData;

public class CameraMover : MonoBehaviour {

	public bool Movable;
    Dictionary<Direction, float> boundary = new Dictionary<Direction, float>();
    const float MARGIN = 1;

	float speed = 4f;
	public Vector3 fixedPosition = Vector3.zero;

	public void SetFixedPosition(Vector3 position){
		fixedPosition = new Vector3(position.x, position.y, transform.position.z);
	}

    bool isAtBoundary(Direction direction) {
        switch(direction) {
        case Direction.Left:
            return Camera.main.transform.position.x <= boundary[direction];
        case Direction.Right:
            return Camera.main.transform.position.x >= boundary[direction];
        case Direction.Up:
            return Camera.main.transform.position.y >= boundary[direction];
        case Direction.Down:
            return Camera.main.transform.position.y <= boundary[direction];
        }
        return true;
    }
	
    void Start() {
		FindObjectOfType<CameraMover>().CalculateBoundary();
		SetFixedPosition (transform.position);
    }
    public void CalculateBoundary() {

        List<Tile> tiles = new List<Tile>();
        Dictionary<Vector2, Tile> tilesDict = FindObjectOfType<TileManager>().GetAllTiles();
        foreach (var kv in tilesDict) tiles.Add(kv.Value);
        boundary[Direction.Left] = tiles.Min(tile => tile.transform.position.x) - MARGIN;
        boundary[Direction.Right] = tiles.Max(tile => tile.transform.position.x) + MARGIN;
        boundary[Direction.Up] = tiles.Max(tile => tile.transform.position.y) + MARGIN;
        boundary[Direction.Down] = tiles.Min(tile => tile.transform.position.y) - MARGIN;
    }
	void Update () {
		// return to fixedPosition.
		if (Input.GetKeyDown(KeyCode.Space)){
			MoveCameraToPosition(fixedPosition);
			return;
		}

		// move by mouse.
		if (Movable){
			Vector3 mousePosition = Input.mousePosition;

			if (mousePosition.x < Screen.width*0.01f && !isAtBoundary(Direction.Left))
				MoveCameraToDirection(Vector3.left);
			else if (mousePosition.x > Screen.width* 0.99f && !isAtBoundary(Direction.Right))
				MoveCameraToDirection(Vector3.right);

			if (mousePosition.y < Screen.height* 0.01f && !isAtBoundary(Direction.Down))
				MoveCameraToDirection(Vector3.down);
			else if (mousePosition.y > Screen.height* 0.99f && !isAtBoundary(Direction.Up))
				MoveCameraToDirection(Vector3.up);
		}

		// move by keyboard.
		if (Movable){
			if (Input.GetKey(KeyCode.LeftArrow) && !isAtBoundary(Direction.Left))
				MoveCameraToDirection(Vector3.left);
			else if (Input.GetKey(KeyCode.RightArrow) && !isAtBoundary(Direction.Right))
				MoveCameraToDirection(Vector3.right);

			if (Input.GetKey(KeyCode.DownArrow) && !isAtBoundary(Direction.Down))
				MoveCameraToDirection(Vector3.down);
			else if (Input.GetKey(KeyCode.UpArrow) && !isAtBoundary(Direction.Up))
				MoveCameraToDirection(Vector3.up);
		}
	}

	void MoveCameraToDirection(Vector3 direction){
		Camera.main.transform.position += direction * speed * Time.deltaTime;
	}

	void MoveCameraToPosition(Vector3 position){
		Camera.main.transform.position = position;
	}

	public void SetMovable(bool able){
		if(SceneData.stageNumber < Setting.mouseCameraMoveOpenStage) {
			Movable = false;
		}
		else {
			Movable = able;
		}
	}
}