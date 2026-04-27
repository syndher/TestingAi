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
public class ScoutAIRed : ScoutAI
{
    // Create the FSM
    public new void Start()
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

        // Add the transitions
        Transition farFromPlayer = new Transition(
            () => CheckFarFromPlayer(),
            null,
            onChase
        );
        Transition closeToPlayer = new Transition(
            () => CheckCloseToPlayer(),
            null,
            onAttack
        );
        onIdle.AddTransition(farFromPlayer);
        onChase.AddTransition(closeToPlayer);
        onAttack.AddTransition(farFromPlayer);

        UpdateFSM(new StateMachine(onIdle));

    }

    // Request actions to the FSM and perform them
    private void Update()
    {
        // Get actions from state machine and invoke them 
        Fsm.Update()?.Invoke();               
    }

    public override void OnChase()
    {
        Debug.Log("Chasing player!");
        Vector3 direction = (Player.transform.position - transform.position).normalized;
        transform.position += direction * MaxSpeed * Time.deltaTime;
    }   
  
    public override void OnAttack()
    {
        Debug.Log("Attacking player!");
    }
    
    private bool CheckCloseToPlayer()
    {
        return (Player.transform.position - transform.position).magnitude <= MinDistanceToPlayer;
    }
    
    private bool CheckFarFromPlayer()
    {
        return (Player.transform.position - transform.position).magnitude > MinDistanceToPlayer;
    }
}