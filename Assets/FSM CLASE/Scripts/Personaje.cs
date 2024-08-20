using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;
using System;

public class Personaje : MonoBehaviour
{
    public enum PlayerInputs { MOVE, JUMP, IDLE, DIE }
    private EventFSM<PlayerInputs> _myFsm;
    private Rigidbody _myRb;
    public Renderer _myRen;

    private void Awake()
    {
        _myRb = gameObject.GetComponent<Rigidbody>();

        //PARTE 1: SETEO INICIAL

        //Creo los estados
        var idle = new State<PlayerInputs>("IDLE");
        var moving = new State<PlayerInputs>("Moving");
        var jumping = new State<PlayerInputs>("Jumping");
        var die = new State<PlayerInputs>("DIE");

        //Creo las transiciones
        StateConfigurer.Create(idle)
            .SetTransition(PlayerInputs.MOVE, moving)
            .SetTransition(PlayerInputs.JUMP, jumping)
            .SetTransition(PlayerInputs.DIE, die)
            .Done(); //aplico y asigno

        StateConfigurer.Create(moving)
            .SetTransition(PlayerInputs.IDLE, idle)
            .SetTransition(PlayerInputs.JUMP, jumping)
            .SetTransition(PlayerInputs.DIE, die)
            .Done();

        StateConfigurer.Create(jumping)
            .SetTransition(PlayerInputs.IDLE, idle)
            .SetTransition(PlayerInputs.MOVE, moving)
            .SetTransition(PlayerInputs.JUMP, jumping)
            .SetTransition(PlayerInputs.DIE, die)
            .Done();


        //Casos donde tengo estados que no pasen a otros:
        //Die no va a tener ninguna transición HACIA nada (uno puede morirse, pero no puede pasar de morirse a caminar)
        //entonces solo lo creo e inmediatamente lo aplico asi el diccionario de transiciones no es nulo y no se rompe nada.
        StateConfigurer.Create(die).Done();




        //PARTE 2: SETEO DE LOS ESTADOS
        //IDLE
        idle.OnUpdate += () =>
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                SendInputToFSM(PlayerInputs.MOVE);
            else if (Input.GetKeyDown(KeyCode.Space))
                SendInputToFSM(PlayerInputs.JUMP);
        };

        //Si no hago nada es opcional agregar comportamiento.
        /*idle.OnEnter += x =>
        {
        };*/

        //En cualquier momento del codigo puedo agregar cualquier tipo de comportamiento a los estados:
        /* idle.OnUpdate += () => {
             if(Input.GetMouseButtonDown(0))
                //SendInputToFSM(PlayerInputs.Attack)
         };*/

        //Para este ejemplo en particular no puedo quitar este comportamiento (Ya que esta en forma de lambda)

        //Si en algun momento quiero quitarlo, deberia guardar el comportamiento tanto en una funcion como en un Action:
        //Action AttackInput += () => { if(Input.GetMouseButtonDown(0)) SendInputToFSM(PlayerInputs.Attack)};

        //Lo agrego:
        //idle.OnUpdate += AttackInput;

        //Para quitar:
        //idle.OnUpdate -= AttackInput;


        //MOVING
        moving.OnEnter += x =>
        {
            _myRen.material.color = Color.blue;
        };

        moving.OnUpdate += () =>
        {
            if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
                SendInputToFSM(PlayerInputs.IDLE);
            else if (Input.GetKeyDown(KeyCode.Space))
                SendInputToFSM(PlayerInputs.JUMP);
        };

        moving.OnFixedUpdate += () =>
        {
            _myRb.velocity += (transform.forward * Input.GetAxis("Vertical") * 20f + transform.right * Input.GetAxis("Horizontal") * 20f) * Time.deltaTime;
        };
        moving.OnExit += x =>
        {
            //x es el input que recibí, por lo que puedo modificar el comportamiento según a donde estoy llendo
            if (x != PlayerInputs.JUMP)
                _myRb.velocity = Vector3.zero;
        };

        //JUMPING
        jumping.OnEnter += x =>
        {
            //tambien uso el rigidbody, pero en vez de tener una variable en cada estado, tengo una sola referencia compartida...
            _myRb.AddForce(transform.up * 10f, ForceMode.Impulse);
        };
        
        jumping.OnUpdate += () =>
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                SendInputToFSM(PlayerInputs.MOVE);
            else if (Input.GetKeyDown(KeyCode.Space))
                SendInputToFSM(PlayerInputs.JUMP);
        };

        //DIE
        die.OnEnter += x =>
        {
            _myRen.material.color = Color.gray;
        };

        //Dado que nuestras transiciones son una clase en si, le agregamos la funcionalidad de llamar a una accion al momento de hacerse esa transicion en si
        //Esto es aparte del Exit de los estados!
        //En este caso si pasamos de el estado "jumping" con el input PlayerInputs.JUMP se ejecuta esto
        jumping.GetTransition(PlayerInputs.JUMP).OnTransition += x =>
        {
            _myRen.material.color = Color.green;
        };
        //En cambio si estamos en "jumping" y se le pone el input de PlayerInputs.IDLE se ejecutaria esto
        jumping.GetTransition(PlayerInputs.IDLE).OnTransition += x =>
        {
            _myRen.material.color = Color.white;
        };

        //En cambio si estamos en "jumping" y se le pone el input de PlayerInputs.IDLE se ejecutaria esto
        moving.GetTransition(PlayerInputs.IDLE).OnTransition += x =>
        {
            _myRen.material.color = Color.black;
        };

        //con todo ya creado, creo la FSM y le asigno el primer estado
        _myFsm = new EventFSM<PlayerInputs>(idle);
    }

    private void SendInputToFSM(PlayerInputs inp) => _myFsm.SendInput(inp);

    private void Update()
    {
        _myFsm.Update();

        if (Input.GetKeyDown(KeyCode.R))
            SendInputToFSM(PlayerInputs.DIE);
    }

    private void FixedUpdate()
    {
        _myFsm.FixedUpdate();
    }

    /*void LateUpdate()
     {
         _myFsm.LateUpdate();
     }*/

    private void OnCollisionEnter(Collision collision)
    {
        SendInputToFSM(PlayerInputs.IDLE);
    }
}
