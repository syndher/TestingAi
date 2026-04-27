/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 * Author: Nuno Fachada
 * */

using System;
using UnityEngine;
using URandom = UnityEngine.Random;
using LibGameAI.FSMs;
using UnityEngine.Rendering;

// The script that controls an agent using an FSM
public class ScoutAI : MonoBehaviour
{
    private float minDistanceToPlayer = 2f;
    public float MinDistanceToPlayer { get => minDistanceToPlayer; }
    private float distanceToBackaway = 15f;
    private float maxSpeed = 5f;
    public float MaxSpeed { get => maxSpeed; }
    private float ammo = 5f;
    private float maxAmmo = 5f;
    private bool shooting = false;
    private float timeToShoot = 0f;
    private float timeToReload = 0f;
    [SerializeField]
    private GameObject player;
    public GameObject Player { get => player; }
    private float backawayEntryTime = 0f;

    private StateMachine fsm;
    public StateMachine Fsm => fsm;


    // Create the FSM
    public void Start()
    {
        // Create the states
        State onIdle = new State(
            "OnIdle",
            null,
            null,
            null
            );
        State onChase = new State(
            "OnChase",
            null,
            OnChase,
            null
            );       
        State onAttack = new State(
            "OnAttack",
            null,
            OnAttack,
            null
            );
        State onShoot = new State(
            "OnShoot",
            null,
            Shooting,
            StopShooting()
            );
        State onBackaway = new State(
            "OnBackaway",
            OnBackawayEnter,  // Entry action
            OnBackaway,
            null
            );

        // Add the transitions
        Transition farFromPlayer = new Transition(
            () => CheckFarFromPlayer(),
            null,
            onShoot
        );

        Transition backedAwayEnough = new Transition(
            () => (player.transform.position - transform.position).magnitude > distanceToBackaway 
                  && Time.time > backawayEntryTime + 3f,
            null,
            onShoot
        );
        onBackaway.AddTransition(backedAwayEnough);

        Transition noAmmo = new Transition(
            () => CheckAmmoEmpty(),
            null,
            onChase
        );
        onShoot.AddTransition(noAmmo);

        Transition fullAmmo = new Transition(
            () => CheckAmmoFull(),
            null,
            onBackaway
        );
        onChase.AddTransition(fullAmmo);
        onAttack.AddTransition(fullAmmo);

        Transition closeToPlayer = new Transition(
            () => CheckCloseToPlayer(),
            null,
            onAttack
        );
        onChase.AddTransition(closeToPlayer);
        onAttack.AddTransition(farFromPlayer);
        onIdle.AddTransition(farFromPlayer);
        onIdle.AddTransition(closeToPlayer);

        // Instantiate the state machine
        UpdateFSM(new StateMachine(onIdle));
    }

    // Request actions to the FSM and perform them
    private void Update()
    {
        // Get actions from state machine and invoke them
        if (shooting == false) Reload(); 
        fsm.Update()?.Invoke();               
    }
    protected void UpdateFSM(StateMachine newFsm) => fsm = newFsm;

    // Entry action for backaway state
    private void OnBackawayEnter()
    {
        Debug.Log("Entered Backaway state");
        backawayEntryTime = Time.time;
    }

    public virtual void OnChase()
    {
        Debug.Log("Chasing player!");
        Vector3 direction = (player.transform.position - transform.position).normalized;
        transform.position += direction * maxSpeed * Time.deltaTime;
    }
    
    public virtual void OnBackaway()
    {   
        Debug.Log("Backing away from player!");
        Vector3 direction = (transform.position - player.transform.position).normalized;
        transform.position += direction * maxSpeed * Time.deltaTime;
    }
    
    public virtual void Shooting()
    {
        shooting = true;
        if (ammo > 0)
        {
            timeToShoot += Time.deltaTime;
            if (timeToShoot >= 3f)
            {
                ammo--;
                Debug.Log($"Shooting player! {ammo}");
                timeToShoot = 0f;            
            }
        }
    }    
    public virtual void OnAttack()
    {
        Debug.Log("Attacking player!");
    }
    
    private bool CheckCloseToPlayer()
    {
        return (player.transform.position - transform.position).magnitude <= MinDistanceToPlayer;
    }
    
    private bool CheckFarFromPlayer()
    {
        return (player.transform.position - transform.position).magnitude > distanceToBackaway;
    }
    
    private void Reload()
    {
        timeToReload += Time.deltaTime;
        if (ammo < maxAmmo && timeToReload >= 10f)
        {
            ammo = maxAmmo;
            Debug.Log($"Reloading! {ammo}");
            timeToReload = 0f;
        }   
    }
    
    private bool CheckAmmoFull()
    {
        return ammo == maxAmmo;
    }
    
    private bool CheckAmmoEmpty()
    {
        return ammo == 0;
    }
    
    private Action StopShooting()
    {
        return () => shooting = false;
    }
}