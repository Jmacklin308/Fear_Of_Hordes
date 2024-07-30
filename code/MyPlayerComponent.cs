
using System.Diagnostics;

public sealed class MyPlayerComponent : Component
{
	[Property] public float movementSpeed { get; set; }

	protected override void OnUpdate()
	{
		if ( Input.Down( "Forward" ) )
		{
			Transform.Position += Vector3.Forward * 100 * Time.Delta;
		}
		if ( Input.Down( "Backward" ) )
		{
			Transform.Position += Vector3.Backward * 100 * Time.Delta;
		}

		if ( Input.Down( "Jump" ) )
		{
			Transform.Position += Vector3.Up * 100 * Time.Delta;
		}

		if ( Input.Down( "Left" ) )
		{
			Transform.Position += Vector3.Left * 100 * Time.Delta;
		}

		if ( Input.Down( "Right" ) )
		{
			Transform.Position += Vector3.Right * 100 * Time.Delta;
		}

	}
}
