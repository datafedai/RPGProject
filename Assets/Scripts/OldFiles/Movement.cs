using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{

    InputAction moveAction;
    [SerializeField] int speed;

    void moveCharacter()

    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        transform.Translate(moveValue * speed * Time.deltaTime, 0f);        
    }



    void Start()
    {
        //Debug.Log("Movement script, Start executed.");

        moveAction = InputSystem.actions.FindAction("Move");
        speed = 10;
    }

    // Update is called once per frame
    void Update()
    {
        moveCharacter();
    }
}

