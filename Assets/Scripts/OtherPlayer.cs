using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class OtherPlayer : MonoBehaviour
{
    private Transform TargetEnd;
    [SerializeField]
    private float speed;

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

        anim.SetBool("isWalking", true);
        //feetSteps.loop = true;
        //feetSteps.Play(0);
    }

    public void StopWalking()
    {
        isWalking = false;
        anim.SetBool("isWalking", false);
        //feetSteps.loop = false;
        //feetSteps.Stop();
    }

    public void SetDeadPlayer()
    {
        isDying = true;
        StartCoroutine(DeadthAnimation());
    }

    IEnumerator DeadthAnimation()
    {
        shoot.Play(0);
        die.PlayDelayed(.2f);
        yield return new WaitForSeconds(Random.Range(0f, 1f));
        anim.SetBool("isDying", true);
        feetSteps.Stop();        
    }
}
