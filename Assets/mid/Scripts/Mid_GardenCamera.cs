using UnityEngine;
using UnityEngine.InputSystem;

// 우클릭 누른 상태로 둘러보기, WASD 이동, QE 높이 조절, Home으로 시작 구도 복귀
public class Mid_GardenCamera : MonoBehaviour
{
    public float speed=5f,lookSensitivity=.12f;
    Vector3 homePosition;Quaternion homeRotation;float yaw,pitch;
    void Awake(){homePosition=transform.position;homeRotation=transform.rotation;SyncAngles();}
    void SyncAngles(){yaw=transform.eulerAngles.y;pitch=transform.eulerAngles.x;if(pitch>180)pitch-=360;}
    void Update()
    {
        var keyboard=Keyboard.current;var mouse=Mouse.current;
        if(keyboard==null||mouse==null)return;
        if(keyboard.homeKey.wasPressedThisFrame){transform.SetPositionAndRotation(homePosition,homeRotation);SyncAngles();}
        if(!mouse.rightButton.isPressed)return;
        Vector2 d=mouse.delta.ReadValue();yaw+=d.x*lookSensitivity;pitch=Mathf.Clamp(pitch-d.y*lookSensitivity,-80,80);
        transform.rotation=Quaternion.Euler(pitch,yaw,0);
        float x=(keyboard.dKey.isPressed?1:0)-(keyboard.aKey.isPressed?1:0);
        float z=(keyboard.wKey.isPressed?1:0)-(keyboard.sKey.isPressed?1:0);
        float y=(keyboard.eKey.isPressed?1:0)-(keyboard.qKey.isPressed?1:0);
        Vector3 move=transform.right*x+transform.forward*z+Vector3.up*y;
        transform.position+=move.normalized*speed*(keyboard.leftShiftKey.isPressed?2:1)*Time.deltaTime;
    }
}
