using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;


public class PlayerInputs : MonoBehaviour
{
    public static PlayerInputs _instance;
    [HideInInspector] public MyInputs myInputs;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    #region Player map
    public void OnMove(InputAction.CallbackContext callbackContext)
    {

        myInputs.move = callbackContext.ReadValue<Vector2>();



    }
    public void OnJump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            myInputs.jump = true;
        }
        if (callbackContext.performed)
        {
            //se hace o mantine la pulsación como el getkeydown
            //MovementController._instance.Jump();
        }

        if (callbackContext.canceled)
        {
            myInputs.jump = false;
        }
        Debug.Log(callbackContext.phase);



    }
    public void OnInteraction(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            myInputs.interaction = true;
            //temp
            myInputs.shrink = true;


        }
        if (callbackContext.canceled)
        {
            myInputs.shrink = false;

        }
    }

    public void OnGrow(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            myInputs.grow = !myInputs.grow;
        }
    }

    #endregion
    #region Ui map
    #endregion

    public void ChangeMap(string map)
    {
        //Pasale un string con el nombre del map a activar, UI o Player

        PlayerInput.all[0].SwitchCurrentActionMap(map);
    }
}

public struct MyInputs
{
    public Vector2 move;
    public bool jump;
    public bool grow;
    public bool interaction;
    internal bool shrink;
}