using UnityEngine;
//using UnityEngine.UI;

public class ClickableSprite : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;
    void OnMouseDown()
    {
        // This function is called when the mouse button is pressed over the collider
        Debug.Log("Clicked " + gameObject.name);

        // desired action: calling another function
        turnManager.handleAwaitingInputPhase(gameObject.name);
    }

    void Start()
    {
        Debug.Log("ClickableSprite script, Start executed.");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
