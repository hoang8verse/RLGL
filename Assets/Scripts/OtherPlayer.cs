using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class OtherPlayer : MonoBehaviour
{
    private Transform TargetEnd;
    [SerializeField]
    private float speed = 12;

    [SerializeField]
    private Animator anim;

    [SerializeField]
    AudioSource feetSteps;

    [SerializeField]
    AudioSource shoot;

    [SerializeField]
    AudioSource die;

    private bool isDying;
    private bool isWalking;
    private string animationPlaying;

    // Start is called before the first frame update
    void Start()
    {
        TargetEnd = GameObject.Find("TargetEnd").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDying)
            return;
        if (isWalking)
        {
            Walk();
        }
    }

    public void StartWalking()
    {
        isWalking = true;
    }
    public void Walk()
    {

        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, 0, TargetEnd.position.z), step);

        PlayAnimationSmoothly("Walk", 0.15f);
    }

    public void StopWalking()
    {
        isWalking = false;
        PlayAnimationSmoothly("Idle", 0.15f);
    }

    public void SetDeadPlayer()
    {
        isDying = true;
        shoot.Play(0);        
        die.PlayDelayed(.2f);
        PlayAnimationSmoothly("Dying", 0.25f);
    }

    private void PlayAnimationSmoothly(string animationName, float delayTime)
    {
        if (animationPlaying == animationName)
        {
            //Debug.LogWarning(animationName + " is playing");
            return;
        }
        animationPlaying = animationName;
        anim.CrossFadeInFixedTime(animationName, delayTime);
    }
}
