using UnityEngine;
using UnityEngine.InputSystem;

// WASD와 방향키로 걷기, 클릭으로 둘러보기 시작, Esc로 마우스 해제
[RequireComponent(typeof(CharacterController))]
public class Mid_WalkCamera : MonoBehaviour
{
    public Camera walkingCamera, shotCamera;
    public float moveSpeed=2.5f, lookSensitivity=.1f;
    CharacterController controller;
    float pitch, verticalSpeed;
    Vector3 startPosition;
    Quaternion startRotation;
    void Awake()
    {
        controller=GetComponent<CharacterController>();
        startPosition=transform.position;startRotation=transform.rotation;
        SelectWalking(true);
    }
    void SelectWalking(bool walking)
    {
        walkingCamera.enabled=walking;
        if(shotCamera!=null)shotCamera.enabled=!walking;
        var listener=walkingCamera.GetComponent<AudioListener>();if(listener!=null)listener.enabled=walking;
        if(shotCamera!=null){listener=shotCamera.GetComponent<AudioListener>();if(listener!=null)listener.enabled=!walking;}
        Cursor.lockState=CursorLockMode.None;
    }
    void Update()
    {
        var kb=Keyboard.current;var mouse=Mouse.current;if(kb==null)return;
        // F1로 걷는 화면, F2로 전체 구도 전환
        if(kb.f1Key.wasPressedThisFrame)SelectWalking(true);
        if(kb.f2Key.wasPressedThisFrame)SelectWalking(false);
        if(!walkingCamera.enabled)return;
        if(kb.escapeKey.wasPressedThisFrame)Cursor.lockState=CursorLockMode.None;
        if(mouse!=null&&mouse.leftButton.wasPressedThisFrame)Cursor.lockState=CursorLockMode.Locked;
        if(mouse!=null&&(Cursor.lockState==CursorLockMode.Locked||mouse.rightButton.isPressed))
        {
            Vector2 delta=mouse.delta.ReadValue()*lookSensitivity;
            transform.Rotate(0,delta.x,0);pitch=Mathf.Clamp(pitch-delta.y,-80,80);
            walkingCamera.transform.localRotation=Quaternion.Euler(pitch,0,0);
        }
        // Home으로 처음 위치 복귀
        if(kb.homeKey.wasPressedThisFrame||transform.position.y < -5)
        {
            controller.enabled=false;transform.SetPositionAndRotation(startPosition,startRotation);controller.enabled=true;
            pitch=0;verticalSpeed=0;walkingCamera.transform.localRotation=Quaternion.identity;
        }
        float x=(kb.dKey.isPressed||kb.rightArrowKey.isPressed?1:0)-(kb.aKey.isPressed||kb.leftArrowKey.isPressed?1:0);
        float z=(kb.wKey.isPressed||kb.upArrowKey.isPressed?1:0)-(kb.sKey.isPressed||kb.downArrowKey.isPressed?1:0);
        Vector3 movement=(transform.right*x+transform.forward*z).normalized*moveSpeed*(kb.shiftKey.isPressed?1.7f:1);
        // 바닥과 다리에 맞춰 중력 및 충돌 적용
        if(controller.isGrounded&&verticalSpeed<0)verticalSpeed=-1;
        verticalSpeed-=9.8f*Time.deltaTime;movement.y=verticalSpeed;
        controller.Move(movement*Time.deltaTime);
    }
    void OnDisable(){Cursor.lockState=CursorLockMode.None;}
}
