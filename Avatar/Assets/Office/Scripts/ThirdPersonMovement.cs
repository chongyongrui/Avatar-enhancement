using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class ThirdPersonMovement : NetworkBehaviour
{
    public CharacterController controller;
    public float movementspeed = 0.5f;
    Vector3 sphere;
    float grounddist;
    [SerializeField] LayerMask groundMask;
    Vector3 velocity;
    float grav = -1f;
    public Vector3 direction;   
    MovementBase currentstate;
    public Idle basestate = new Idle();
    public Walking walkstate = new Walking();
    public Animator anim;
    float horizontalinput,verticalinput;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
        SwitchState(basestate);

    }

    // Update is called once per frame

    void Update()
    {
        Gravity();
        directionFunction();
        anim.SetFloat("hzinput",horizontalinput);
        anim.SetFloat("vinput",verticalinput);        
        currentstate.UpdateState(this);
    }
    public void directionFunction()
    {   if(!IsOwner)return;
         horizontalinput = Input.GetAxis("Horizontal");
         verticalinput = Input.GetAxis("Vertical");
        direction = transform.forward * verticalinput + transform.right * horizontalinput;
        controller.Move(direction.normalized * movementspeed * Time.deltaTime);
    }
    public void SwitchState(MovementBase state)
    {
        currentstate = state;
        currentstate.EnterState(this);

    }
    bool IsGrounded()
    {
        sphere = new Vector3(transform.position.x, transform.position.y - grounddist, transform.position.z);
        if (Physics.CheckSphere(sphere, controller.radius - 0.05f, groundMask)) { return true; }
        return false;
    }
    void Gravity()
    {
        if (!IsGrounded())
        {
            velocity.y += grav * Time.fixedDeltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2;
        }

        controller.Move(velocity * Time.fixedDeltaTime);
    }
}
