using Sandbox;

public sealed class CameraMovement : Component
{

	[Property] public PlayerMovement Player { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Head { get; set; }
	[Property] public float Distance { get; set; } //camera distance

	//for shoulder swapping
	[Property] public bool isRightShoulder { get; set; } = true;
	[Property] public float shoulderOffsetX { get; set; } = -53.6f;
	[Property] public float shoulderOffsetY { get; set; }


	//
	public bool isFirstPerson => Distance == 0f;
	private CameraComponent Camera;

	//lerp shoulder switching
	private Vector3 currentShoulderOffset;
	private Vector3 targetShoulderOffset;
	[Property] private float shoulderLerpFactor = 1f; // Adjust this value for smoothness
	[Property] private float cameraLerpFactor = 1f; // Adjust this value for smoothness


	//for camera trailing
	private Vector3 previousCameraPosition;

	protected override void OnAwake()
	{
		Camera = Components.Get<CameraComponent>();
		currentShoulderOffset = new Vector3( shoulderOffsetX, isRightShoulder ? shoulderOffsetY : -shoulderOffsetY, 0f );
		targetShoulderOffset = currentShoulderOffset;
		previousCameraPosition = Camera.Transform.Position;
	}


	protected override void OnUpdate()
	{

		//TODO: Add camera shake
		var eyeAngle = Head.Transform.Rotation.Angles();
		eyeAngle.pitch += Input.MouseDelta.y * 0.1f;
		eyeAngle.yaw -= Input.MouseDelta.x * 0.1f;
		eyeAngle.roll += 0f;
		eyeAngle.pitch = eyeAngle.pitch.Clamp( -89f, 89f );
		Head.Transform.Rotation = Rotation.From( eyeAngle );


		//set pos of camera
		if ( Camera != null )
		{
			var camPos = Head.Transform.Position;
			var camRotation = eyeAngle.ToRotation();

			//Switch shoulders
			if ( Input.Pressed( "SwitchShoulder" ) )
			{
				isRightShoulder = !isRightShoulder;
				targetShoulderOffset = new Vector3(
					shoulderOffsetX,
					isRightShoulder ? -shoulderOffsetY : shoulderOffsetY,
					0f
				);
			}

			// Lerp towards target shoulder offset
			currentShoulderOffset = Vector3.Lerp( currentShoulderOffset, targetShoulderOffset, shoulderLerpFactor / 10 );
			var localOffset = camRotation * currentShoulderOffset;
			camPos += localOffset;

			if ( !isFirstPerson )
			{

				//trace backwards to see where we can place the camera
				var camForward = eyeAngle.ToRotation().Forward;
				var camTrace = Scene.Trace.Ray( camPos, camPos - (camForward * Distance) )
				.WithoutTags( "player", "trigger" ) //don't hit the player object or a trigger
				.Run();

				if ( camTrace.Hit )
				{
					//just barely push out camera
					camPos = camTrace.HitPosition + camTrace.Normal;
				}
				else
				{
					camPos = camTrace.EndPosition;
				}
			}

			// Lerp the camera position for a trailing effect
			var newCameraPosition = Vector3.Lerp( previousCameraPosition, camPos, cameraLerpFactor * Time.Delta );
			previousCameraPosition = newCameraPosition;

			//Set camera position
			Camera.Transform.Position = newCameraPosition;
			Camera.Transform.Rotation = eyeAngle.ToRotation();

		}
	}





}