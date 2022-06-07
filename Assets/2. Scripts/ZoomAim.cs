using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomAim : Character
{
	public Texture2D crosshair;                                           // �ʻ��� ������ �ý���
	public float aimTurnSmoothing = 0.15f;                                // ī�޶��� ����� ��ġ�ϱ� ���� ���� �� �� ȸ�� ���� �ӵ�
	public Vector3 aimPivotOffset = new Vector3(0.5f, 1.2f, 0f);         // ���� �� ī�޶� Pivot ����
	public Vector3 aimCamOffset = new Vector3(0f, 0.4f, -0.7f);         // ���� �� ī�޶��� Offset ����

	private int aimBool;
	private int hashSpeed;
	private bool aim;
	Transform cameraObject;

	void Start()
	{
		aimBool = Animator.StringToHash("Aim");
		hashSpeed = Animator.StringToHash("Speed");
		cameraObject = Camera.main.transform;

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update()
	{
		// ���콺 ��Ŭ���� ���ȴ°�
		if (Input.GetMouseButton(1) && !aim)
		{
			StartCoroutine(ToggleAimOn());
		}
		// ���콺 ��Ŭ�� ���°�
		else if (aim && Input.GetMouseButtonUp(1))
		{
			StartCoroutine(ToggleAimOff());
		}

		// ������Ʈ ���� ���ϰ� ����
		canSprint = !aim;

		// ī�޶� ���� ��ġ�� ���� �Ǵ� ���������� ��ȯ
		if (aim && Input.GetMouseButtonDown(2))
		{
			aimCamOffset.x = aimCamOffset.x * (-1);
			aimPivotOffset.x = aimPivotOffset.x * (-1);
		}

		ani.SetBool(aimBool, aim);
	}

	// ���� ��带 ������ ���� ó��
	private IEnumerator ToggleAimOn()
	{
		yield return new WaitForSeconds(0.05f);
		// ���� �� �� ����.
		//if (behaviourManager.GetTempLockStatus(this.behaviourCode) || behaviourManager.IsOverriding(this))
			//yield return false;

		// Start aiming.
		//else
		//{
		aim = true;
		int signal = 1;
		aimCamOffset.x = Mathf.Abs(aimCamOffset.x) * signal;
		aimPivotOffset.x = Mathf.Abs(aimPivotOffset.x) * signal;
		yield return new WaitForSeconds(0.1f);
		ani.SetFloat(hashSpeed, 0);
			//behaviourManager.GetAnim.SetFloat(speedFloat, 0);
			// This state overrides the active one.
		//behaviourManager.OverrideWithBehaviour(this);
		//}
	}

	// ���� ��带 ������ ���� ����ó��
	private IEnumerator ToggleAimOff()
	{
		aim = false;
		yield return new WaitForSeconds(0.3f);
		cameraObject.GetComponent<CameraFollow>().ResetTargetOffsets();
		cameraObject.GetComponent<CameraFollow>().ResetMaxVerticalAngle();
		yield return new WaitForSeconds(0.05f);
		//behaviourManager.RevokeOverridingBehaviour(this);
	}

	// LocalFixedUpdate overrides the virtual function of the base class.
	public void FixedUpdate()
	{
		// ī�޶� ��ġ�� ������ ���� ���� ����
		if (aim)
			cameraObject.GetComponent<CameraFollow>().SetTargetOffsets(aimPivotOffset, aimCamOffset);
	}

	// �÷��̾��� ȸ�� �缳��
	public void LateUpdate()
	{
		AimManagement();
	}

	// ������ Ȱ�� ������ �� ���� �Ű������� ó��
	void AimManagement()
	{
		// ������ �� �÷��̾� ���� ���
		Rotating();
	}

	// Rotate the player to match correct orientation, according to camera.
	void Rotating()
	{
		Vector3 forward = cameraObject.TransformDirection(Vector3.forward);
		// Player is moving on ground, Y component of camera facing is not relevant.
		forward.y = 0.0f;
		forward = forward.normalized;

		// Always rotates the player according to the camera horizontal rotation in aim mode.
		Quaternion targetRotation = Quaternion.Euler(0, cameraObject.GetComponent<CameraFollow>().GetH, 0);

		float minSpeed = Quaternion.Angle(transform.rotation, targetRotation) * aimTurnSmoothing;

		// Rotate entire player to face camera.
		SetLastDirection(forward);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, minSpeed * Time.deltaTime);

	}

	private void SetLastDirection(Vector3 direction)
	{
		lastDirection = direction;
	}

	// Draw the crosshair when aiming.
	void OnGUI()
	{
		if (crosshair)
		{
			float mag = cameraObject.GetComponent<CameraFollow>().GetCurrentPivotMagnitude(aimPivotOffset);
			if (mag < 0.05f)
				GUI.DrawTexture(new Rect(Screen.width / 2 - (crosshair.width * 0.5f),
										 Screen.height / 2 - (crosshair.height * 0.5f),
										 crosshair.width, crosshair.height), crosshair);
		}
	}
}
