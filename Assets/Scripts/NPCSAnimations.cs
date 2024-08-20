using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSAnimations : MonoBehaviour
{
    [SerializeField] Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        int newAnimation = Random.Range(0,3);
        switch(newAnimation)
        {
            case 0:
                anim.SetTrigger("Punch");
                break;
            case 1:
                anim.SetTrigger("Dead");
                break;
            case 2:
                anim.SetTrigger("Dead2");
                break;
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
