using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public PlayerMovementData currentDataMovement;

    //tipos de movimiento
    public PlayerMovementData normalDataMovement;
    public PlayerMovementData bigSizeDataMovement;
    public PlayerMovementData littleSizeDataMovement;

    private void Awake()
    {
        currentDataMovement = normalDataMovement;
    }
}
