using Sandbox;
using Sandbox.Citizen;



//BIG HELP from Carson Kompon

public sealed class PlayerMovement : Component
{
	[Property] public float GroundControl { get; set; } = 4.0f;
	[Property] public float AirControl { get; set; } = 0.1f;
	[Property] public float MaxForce { get; set; } = 50f;
	[Property] public float Speed { get; set; } = 160f;
	[Property] public float RunSpeed { get; set; } = 290f;
	[Property] public float CrouchSpeed { get; set; } = 90f;
	[Property] public float JumpForce { get; set; } = 400f;

	//References
	[Property] private GameObject _head { get; set; }
	[Property] private GameObject _body { get; set; }


	//member vairalbes
	public Vector3 WishVelocity = Vector3.Zero;
	public bool isCrouching = false;
	public bool isSprinting = false;

	private CharacterController _charController;
	private CitizenAnimationHelper _animationHelper;



	protected override void OnAwake()
	{
		_charController = Components.Get<CharacterController>();
		_animationHelper = Components.Get<CitizenAnimationHelper>();

	}

	protected override void OnUpdate()
	{
		//set sprinting and crouching state
		isCrouching = Input.Down( "Crouch" );
		isSprinting = Input.Down( "Run" );

	}

	protected override void OnFixedUpdate()
	{
		BuildWishVelocity();
		Move();
	}

	private void BuildWishVelocity()
	{
		WishVelocity = 0;

		//get head location
		var rot = _head.Transform.Rotation;

		if ( Input.Down( "Forward" ) ) WishVelocity += rot.Forward;
		if ( Input.Down( "Backward" ) ) WishVelocity += rot.Backward;
		if ( Input.Down( "Left" ) ) WishVelocity += rot.Left;
		if ( Input.Down( "Right" ) ) WishVelocity += rot.Right;


		//make sure were on the ground
		WishVelocity = WishVelocity.WithZ( 0 );

		if ( !WishVelocity.IsNearZeroLength ) WishVelocity = WishVelocity.Normal;

		if ( isCrouching ) WishVelocity *= CrouchSpeed;
		else if ( isSprinting ) WishVelocity *= RunSpeed;
		else WishVelocity *= Speed;

	}

	void Move()
	{
		// Get gravity from scene
		Vector3 gravity = Scene.PhysicsWorld.Gravity;

		if ( _charController.IsOnGround )
		{
			//reset velocity if we are hitting ground
			_charController.Velocity = _charController.Velocity.WithZ( 0 );
			_charController.Accelerate( WishVelocity );
			_charController.ApplyFriction( GroundControl );
		}
		else
		{
			//we are in the air now - slowly move down
			_charController.Velocity += gravity * Time.Delta * 0.5f; //first second half of gravity
			_charController.Accelerate( WishVelocity.ClampLength( MaxForce ) );
			_charController.ApplyFriction( AirControl );
		}

		//move the character controller.
		_charController.Move();

		//apply second half of gravity 
		if ( !_charController.IsOnGround )
		{
			_charController.Velocity += gravity * Time.Delta * 0.5f;

		}
		else
		{
			_charController.Velocity = _charController.Velocity.WithZ( 0 );
		}
	}

}