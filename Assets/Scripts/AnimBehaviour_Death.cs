using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimBehaviour_Death : StateMachineBehaviour
{
    [SerializeField] string _playerTag = default;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.tag == _playerTag)
        {
            Destroy(animator.gameObject);
            return;
        }
        Destroy(animator.transform.parent.gameObject);
    }
}
