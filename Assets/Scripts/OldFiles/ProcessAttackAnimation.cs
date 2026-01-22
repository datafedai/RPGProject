using UnityEngine;

public class ProcessAttackAnimation : MonoBehaviour
{


    void OnEnable()
    {
        //Debug.Log("in OnEnable in ProcessAttackAnimation");
        //TurnManager.Instance.AttackAnimation += OnAttackAnimation;
    
    }

    void OnDiable()
    {
        //Debug.Log("in OnEnable in ProcessAttackAnimation");
        //TurnManager.Instance.AttackAnimation -= OnAttackAnimation;
    
    }




    void OnAttackAnimation(GameObject p, GameObject o)
    {
        Debug.Log("player: " + p.transform.position);
        Debug.Log("oponent: " + o.transform.position);

        p.transform.position = p.transform.position + new Vector3(10f,10f,10f);
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
