using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimBehaviour_Death : StateMachineBehaviour
{
    [SerializeField] string _playerTag;
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.tag == "Player")
        {
            Destroy(animator.gameObject);
            return;
        }
        Destroy(animator.transform.parent.gameObject);
    }
}
