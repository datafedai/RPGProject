using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;

public class ButtonHandler : MonoBehaviour
{

    //public Button greenEyesButton;
    [SerializeField] private TurnManager turnManager;

    public void OnButtonClick()
    {
        //Debug.Log("Button Clicked!");
        // Add your desired functionality here
                // Get the name of the clicked button
        string clickedButtonName = EventSystem.current.currentSelectedGameObject.tag;
        //Debug.Log("Clicked enemy name: " + clickedButtonName);

        turnManager.handleAwaitingInputPhase(clickedButtonName);
        Debug.Log("Clicked " + clickedButtonName);
        
    }


    public void TaskOnClick()
    {
        Debug.Log("green eyes clicked");
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("ButtonHandle script, Start executed.");
        // Add a listener to the button's onClick event
        /*
        if (greenEyesButton != null)
        {
            greenEyesButton.onClick.AddListener(TaskOnClick);
        }
        */
    }

    // Update is called once per frame
    void Update()
    {

    }
}
