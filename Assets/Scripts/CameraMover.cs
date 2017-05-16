using UnityEngine;
using System.Collections;

public class CameraMover : MonoBehaviour {

	public bool mouseMoveActive;
	public bool keyboardMoveActive;

	float speed = 0.1f;
	Vector3 fixedPosition = Vector3.zero;

	public void SetFixedPosition(Vector3 position)
	{
		fixedPosition = new Vector3(position.x, position.y, transform.position.z);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		// return to fixedPosition.
		if (Input.GetKeyDown(KeyCode.Space))
		{
			MoveCameraToPosition(fixedPosition);
			return;
		}

		// move by mouse.
		if (mouseMoveActive)
		{
			Vector3 mousePosition = Input.mousePosition;

			if (mousePosition.x < Screen.width*0.01f)
				MoveCameraToDirection(Vector3.left);
			else if (mousePosition.x > Screen.width*0.99f)
				MoveCameraToDirection(Vector3.right);

			if (mousePosition.y < Screen.height*0.01f)
				MoveCameraToDirection(Vector3.down);
			else if (mousePosition.y > Screen.height*0.99f)
				MoveCameraToDirection(Vector3.up);
		}

		// move by keyboard.
		if (keyboardMoveActive)
		{
			if (Input.GetKey(KeyCode.LeftArrow))
				MoveCameraToDirection(Vector3.left);
			else if (Input.GetKey(KeyCode.RightArrow))
				MoveCameraToDirection(Vector3.right);

			if (Input.GetKey(KeyCode.DownArrow))
				MoveCameraToDirection(Vector3.down);
			else if (Input.GetKey(KeyCode.UpArrow))
				MoveCameraToDirection(Vector3.up);
		}
	}

	void MoveCameraToDirection(Vector3 direction)
	{
		Camera.main.transform.position += direction*speed;
	}

	void MoveCameraToPosition(Vector3 position)
	{
		Camera.main.transform.position = position;
	}
}
