using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    CharacterController controller;
    Vector2 movement;
    public float movementspeed;
    Vector3 sphere;
    float grounddist;
    [SerializeField]LayerMask groundMask;
    Vector3 velocity;
    float grav = -1f;
    public Vector3 direction;
    MovementBase currentstate;
    public Idle basestate = new Idle();

    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {   anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        SwitchState(basestate);

    }

    // Update is called once per frame
    public void SwitchState(MovementBase state){
        currentstate = state;
        currentstate.EnterState(this);
    
    }
    void Update()
    {
        var horizontalinput = Input.GetAxis("Horizontal");
        var verticalinput = Input.GetAxis("Vertical");
        movement = new Vector2(horizontalinput, verticalinput);
         direction = new Vector3(movement.x, 0, movement.y).normalized;
        

        if (direction.magnitude >= 0.1f)
        {
            controller.Move(direction * movementspeed * Time.fixedDeltaTime);
        }
        Gravity();
        anim.SetFloat("hzinput",horizontalinput);
        anim.SetFloat("vinput",verticalinput);
        currentstate.UpdateState(this);
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
