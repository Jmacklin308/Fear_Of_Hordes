using Sandbox;

public sealed class CameraMovement : Component
{
	[Property] public PlayerMovement Player { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Head { get; set; }
	[Property] public float Distance { get; set; } //camera distance
	[Property] public float RealCameraDistance { get; set; }

	[Range( 0.01f, 1f )]
	[Property] public float MouseSensitivity { get; set; }

	// for shoulder swapping
	[Property] public bool isRightShoulder { get; set; } = true;
	[Property] public float shoulderOffsetX { get; set; } = -53.6f;
	[Property] public float shoulderOffsetY { get; set; }

	public bool isFirstPerson => Distance == 0f;
	private CameraComponent Camera;

	// lerp shoulder switching
	private Vector3 currentShoulderOffset;
	private Vector3 targetShoulderOffset;
	[Property] private float shoulderLerpFactor = 1f; // Adjust this value for smoothness
	[Property] private float cameraLerpFactor = 1f; // Adjust this value for smoothness

	// for camera trailing
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
		var eyeAngle = Head.Transform.Rotation.Angles();
		eyeAngle.pitch += Input.MouseDelta.y * MouseSensitivity;
		eyeAngle.yaw -= Input.MouseDelta.x * MouseSensitivity;
		eyeAngle.pitch = eyeAngle.pitch.Clamp( -89f, 89f );
		Head.Transform.Rotation = Rotation.From( eyeAngle );

		if ( Camera != null )
		{
			var camPos = Head.Transform.Position;
			var camRotation = eyeAngle.ToRotation();

			// Switch shoulders
			if ( Input.Pressed( "SwitchShoulder" ) )
			{
				isRightShoulder = !isRightShoulder;
				targetShoulderOffset = new Vector3(
					shoulderOffsetX,
					isRightShoulder ? shoulderOffsetY : -shoulderOffsetY,
					0f
				);
			}

			// Lerp towards target shoulder offset
			currentShoulderOffset = Vector3.Lerp( currentShoulderOffset, targetShoulderOffset, shoulderLerpFactor * Time.Delta );
			var localOffset = camRotation * currentShoulderOffset;
			camPos += localOffset;

			bool camHit = false;

			if ( !isFirstPerson )
			{
				// Trace in smaller steps for more precise collision detection
				var camForward = eyeAngle.ToRotation().Forward;
				var step = 0.06f; // Adjust step size as needed
				var totalSteps = (int)(Distance / step);
				var lastValidPosition = camPos;

				for ( int i = 1; i <= totalSteps; i++ )
				{
					var testPos = camPos - camForward * (i * step);
					var camTrace = Scene.Trace
						.Ray( camPos, testPos )
						.Size( BBox.FromPositionAndSize( 5, 10 ) )
						.WithoutTags( "player", "trigger" ) // don't hit the player object or a trigger
						.UseHitboxes( true )
						.Run();

					camHit = camTrace.Hit;

					if ( camHit )
					{
						RealCameraDistance = camTrace.Distance;

						camPos = camTrace.HitPosition + camTrace.Normal * 1f; // Adjust the push-out factor as needed
						break;
					}
					else
					{
						lastValidPosition = testPos;
					}
				}

				// If no hit, use the last valid position
				if ( !camHit )
				{
					camPos = lastValidPosition;
				}
			}

			// Lerp the camera position for a trailing effect
			var newCameraPosition = Vector3.Lerp( previousCameraPosition, camPos, cameraLerpFactor * Time.Delta );
			previousCameraPosition = newCameraPosition;

			// Set camera position and rotation
			Camera.Transform.Position = newCameraPosition;
			Camera.Transform.Rotation = eyeAngle.ToRotation();
		}
	}
}
