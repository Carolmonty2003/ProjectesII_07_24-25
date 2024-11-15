using UnityEngine;
using Cinemachine;
using System.Collections;

public class DoorCameraController : MonoBehaviour
{
    public CinemachineVirtualCamera transitionCamera;
    public Transform door1Position;
    public Transform door2Position;
    public CinemachineVirtualCamera playerCamera;
    public float transitionDuration = 1.5f;

    public void ShowDoors()
    {
        StartCoroutine(ShowDoorsSequence());
    }

    private IEnumerator ShowDoorsSequence()
    {
        transitionCamera.Priority = 11;

        transitionCamera.transform.position = new Vector3(door1Position.position.x, door1Position.position.y, transitionCamera.transform.position.z);
        yield return new WaitForSeconds(transitionDuration);

        transitionCamera.transform.position = new Vector3(door2Position.position.x, door2Position.position.y, transitionCamera.transform.position.z);
        yield return new WaitForSeconds(transitionDuration);

        transitionCamera.Priority = 5;
        playerCamera.Priority = 10;
    }
}
