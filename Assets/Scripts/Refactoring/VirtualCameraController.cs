using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualCameraController : MonoBehaviour
{
    private Vector2 mouseLook;
    private Vector2 smoothV;
    public float sensitivity = 5.0f;
    public float smoothing = 2.0f;
    private GameObject character;

    public float speed = 10.0F;
    private float normalSpeed;
    private float dashSpeed;

    [SerializeField]
    private Transform m_moveTarget;
    [SerializeField]
    private Transform m_rotTarget;
    public Transform secondaryTarget;

    public bool enableMouseControl;
    public bool enableRightJoystick;
    public bool inVR;

    public Transform MoveTarget
    {
        get
        {
            if (m_moveTarget == null)
                m_moveTarget = transform.parent;
            return m_moveTarget;
        }
    }

    public Transform RotTarget
    {
        get
        {
            if (m_rotTarget == null)
                m_rotTarget = transform;
            return m_rotTarget;
        }
    }

    // Use this for initialization
    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        character = MoveTarget.gameObject;
        normalSpeed = speed;
        dashSpeed = speed * 2;
    }

    // Update is called once per frame
    private void Update()
    {
        if (enableMouseControl)
            UpdateRotationByMouse();
        if (enableRightJoystick)
            UpdateRotationByJoystick();

        UpdateMovement();

        RaiseOrLower();

        Speedup();
    }

    private void UpdateMovement()
    {
        float translation = Input.GetAxis("Vertical") * speed;
        float straffe = Input.GetAxis("Horizontal") * speed;
        translation *= Time.deltaTime;
        straffe *= Time.deltaTime;

        Vector3 moveDir = new Vector3(straffe, 0, translation);

        MoveTarget.Translate((RotTarget.rotation * moveDir).SetY(0), Space.World);

        if (Input.GetKeyDown("escape"))
            Cursor.lockState = CursorLockMode.None;
    }

    private void UpdateRotationByMouse()
    {
        Vector2 moveDir = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        moveDir = Vector2.Scale(moveDir, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
        smoothV.x = Mathf.Lerp(smoothV.x, moveDir.x, 1f / smoothing);
        smoothV.y = Mathf.Lerp(smoothV.y, moveDir.y, 1f / smoothing);
        mouseLook += smoothV;

        if (inVR)
        {
            if (secondaryTarget != null)
                secondaryTarget.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
        }
        else
        {
            RotTarget.localRotation = Quaternion.AngleAxis(Mathf.Clamp(-mouseLook.y, -30, 30), Vector3.right);
        }
        MoveTarget.localRotation = Quaternion.AngleAxis(mouseLook.x, MoveTarget.up);
    }

    private void UpdateRotationByJoystick()
    {
        Vector2 moveDir = new Vector2(Input.GetAxis("Oculus_GearVR_RThumbstickX"), Input.GetAxis("Oculus_GearVR_RThumbstickY"));
        if (Mathf.Abs(moveDir.x) < 0.1f)
            moveDir = new Vector2(0, moveDir.y);
        if (Mathf.Abs(moveDir.y) < 0.1f)
            moveDir = new Vector2(moveDir.x, 0);

        moveDir = Vector2.Scale(moveDir, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
        smoothV.x = Mathf.Lerp(smoothV.x, moveDir.x, 1f / smoothing);
        smoothV.y = Mathf.Lerp(smoothV.y, moveDir.y, 1f / smoothing);
        mouseLook += smoothV;

        if (inVR)
        {
            if (secondaryTarget != null)
                secondaryTarget.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
        }
        else
        {
            RotTarget.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
        }
        MoveTarget.localRotation = Quaternion.AngleAxis(mouseLook.x, MoveTarget.up);
    }

    private void RaiseOrLower()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            MoveTarget.Translate(Vector3.down * speed * Time.deltaTime, Space.World);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            MoveTarget.Translate(Vector3.up * speed * Time.deltaTime, Space.World);
        }
    }

    private void Speedup()
    {
        if (Input.GetAxis("Jump") > 0)
        {
            //print("Speed up");
            speed = dashSpeed;
        }
        else
        {
            speed = normalSpeed;
        }
    }
}
