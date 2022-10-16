/* Scripted by Omabu - omabuarts@gmail.com */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using TPSBR;

public class FrogControllerAnim : MonoBehaviour
{
    private Animator[] animator;

    private Transform animal;
    
    Vector2 _prevLocation = Vector2.zero;
    float _prevTimeStamp = 0;
    
    float _speed = 0;
    float _averageSpeed = 0;

    int _queueCapacity = 20;
    Queue<float> _speedQueue;

    private Agent _Agent;

    public bool IsDebuggingAnims = false;
    public bool IsFake = false;

    public AudioSource Ouch;

    public enum anims
    {
        Idle_A,
        Run,
        Roll,
        Spin_Splash
    }
    // 0 = Idle A
    // 1 = Run
    // 2 = Roll
    // 3 = Spin/Splash

    public anims CurrentAnim = anims.Idle_A;
    public anims DebugAnim = anims.Idle_A;

    float _minSpeedIdle_A = float.MinValue;
    public float _minSpeedRun = 0.5f;
    public float _minSpeedRoll = 3;

    public bool IsStunned = false;
    public float StunTime = 2;
    public float _StunStartTime = 0;
    
    private void Start()
    {

        _Agent = GetComponentInParent<Agent>();

        int count = 0;
        _speedQueue = new Queue<float>(_queueCapacity);

        animal = transform;

        for (int i = 0; i < animal.childCount; i++)
            if (animal.GetChild(i).GetComponent<Animator>() != null)
                count++;

        animator = new Animator[count];

        for (int i = 0; i < animal.childCount; i++)
            if (animal.GetChild(i).GetComponent<Animator>() != null)
                animator[i] = animal.GetChild(i).GetComponent<Animator>();

        for (int i = 0; i < _queueCapacity; i++)
        {
            _speedQueue.Enqueue(0);
        }
    }

    private void Update()
    {
        //if (Input.GetKeyDown ("right")) { NextAnim (); }
        //else if (Input.GetKeyDown ("left")) { PrevAnim (); }
        var timeStamp = Time.time;
        var location = animal.position;
        var flatLocation = new Vector2(location.x, location.z);
        var travelDistance = (flatLocation - _prevLocation).sqrMagnitude;
        var travelTime = timeStamp - _prevTimeStamp;
        
        if ( travelTime != 0)
        {
            _speed = travelDistance / travelTime;
            _speedQueue.Enqueue(_speed);
            _speedQueue.Dequeue();
        }

        if(Keyboard.current.oKey.isPressed)
        {
            var x = 0;
        }
        _prevLocation = flatLocation;
        _prevTimeStamp = timeStamp;
        _averageSpeed = _speedQueue.Average();
        //Debug.Log("Average Speed is: " + _averageSpeed);

        if (IsStunned)
        {
            if (Time.time > _StunStartTime + StunTime)
            {
                IsStunned = false;
                _Agent.AgentInput.IsStunned = false;
            } else
            {
                return;
            }
        }

        var targetAnim = anims.Idle_A;

        if (IsStunned == false)
        {
            if (_averageSpeed > _minSpeedIdle_A)
                targetAnim = anims.Idle_A;
            if (_averageSpeed > _minSpeedRun)
                targetAnim = anims.Run;
            if (_averageSpeed > _minSpeedRoll)
                targetAnim = anims.Roll;
            
            if(IsDebuggingAnims && IsFake)
            {
                if (DebugAnim != CurrentAnim)
                {
                    CurrentAnim = DebugAnim;
                    PlayAnim(DebugAnim.ToString());
                }
            } else
            {
                if(targetAnim != CurrentAnim)
                {
                    CurrentAnim = targetAnim;
                    PlayAnim(targetAnim.ToString());
                }
            }
        }

    }

    public void NextAnim()
    {
        //if (dropdown.value >= dropdown.options.Count - 1)
        //    dropdown.value = 0;
        //else
        //    dropdown.value++;

        //PlayAnim();
    }

    public void PrevAnim()
    {
        //if (dropdown.value <= 0)
        //    dropdown.value = dropdown.options.Count - 1;
        //else
        //    dropdown.value--;

        //PlayAnim();
    }

    public void PlayAnim()
    {
        for (int i = 0; i < animator.Length; i++)
        {
            //animator[i].Play(dropdown.options[dropdown.value].text);
        }
    }

    public void PlayAnim(string anim)
    {
        for (int i = 0; i < animator.Length; i++)
        {
            animator[i].Play(anim);
        }
    }

    public void GoToWebsite(string URL)
    {
        Application.OpenURL(URL);
    }

    private void OnTriggerEnter(Collider other)
    {
         //&& _Agent.IsLocal
        if (_Agent != null)
        {
            if(other.gameObject.name == "KCCCollider")
            {
                Debug.Log("FROGG COLLIDED WITH!!!!:" + other.gameObject.name);
                Debug.Log("My anim state is: " + CurrentAnim.ToString());
                var theirAnim = other.gameObject.transform.root.GetComponentInChildren<FrogControllerAnim>().CurrentAnim;
                Debug.Log("Their anim state is: " + theirAnim.ToString());
                if(theirAnim == anims.Roll)
                {
                    IsStunned = true;
                    _StunStartTime = Time.time;
                    PlayAnim(anims.Spin_Splash.ToString());
                    Ouch.Play();
                    _Agent.AgentInput.IsStunned = true;
                }
            } else if (other.gameObject.name == "BreakZone")
            {
                var lightPost = other.gameObject.transform.parent;
                var lightPostIndex = lightPost.GetSiblingIndex();

                _Agent.Context.NetworkGame.MyGameModeManager.MakeLightOff(lightPostIndex);
            }

        }

        if (_Agent != null)
        {
            if (other.gameObject.name == "BreakZone" && _Agent.HasStateAuthority)
            {
                var lightPost = other.gameObject.transform.parent;
                var lightPostIndex = lightPost.GetSiblingIndex();
                (_Agent.Context.GameplayMode as BattleRoyaleGameplayMode).RPC_BreakLight(lightPostIndex, _Agent.Object.InputAuthority);
                Debug.Log("other is :::: " + other.gameObject.name);
            }
            //Debug.Log("FAKE FAKE COLLIDED WITH!!!!:" + other.gameObject.name);
            //Debug.Log("My anim state is: " + CurrentAnim.ToString());
        }
        //var otherAnim = other.gameObject.GetComponent<FrogControllerAnim>().CurrentAnim;
        //if(otherAnim == anims.Roll)
        //{

        //}
    }
}