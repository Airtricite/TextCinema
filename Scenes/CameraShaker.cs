using Godot;
using System;

public partial class CameraShaker : Node2D
{
	// Called when the node enters the scene tree for the first time.
	float frequency=30;
	float magnitude=60;
	float magnitude_rot=5;
	//float decay_rate=5;

	RandomNumberGenerator rng=new RandomNumberGenerator();
	FastNoiseLite noise=new();

	private float noise_i = 0.0f;
    private float shake_strength = 0.0f;
	//Camera2D camera;
	public static Marker3D Values=new Marker3D();
    public static Marker2D Multipliers=new Marker2D();
    public override void _Ready()
    {
        
      
        
        rng.Randomize();
        // Randomize the generated noise
        noise.Seed = (int)rng.Randi();
        // Period affects how quickly the noise changes values
        noise.Frequency = 1;

      //  applyButton.Connect("pressed", this, nameof(ApplyShake));
    }

    
    public override void _Process(double delta)
    {
		magnitude=Values.Position.X;
		magnitude_rot=Values.Position.Y;
		frequency=Values.Position.Z;
        // Fade out the intensity over time
        //shake_strength = (float)Mathf.Lerp(shake_strength, 0, decay_rate * delta);

        // Shake by adjusting camera.offset so we can move the camera around the level via its position
		 noise_i += (float)(delta * frequency);
        cinema.Camera.Position = GetNoiseOffset()*Multipliers.Position;
		cinema.Camera.RotationDegrees=GetNoise();
    }

    private Vector2 GetNoiseOffset()
    {
       
        // Set the x values of each call to 'GetNoise2d' to a different value
        // so that our x and y vectors will be reading from unrelated areas of noise
        return new Vector2(
            noise.GetNoise2D(1, noise_i) * magnitude,
            noise.GetNoise2D(100, noise_i) * magnitude
        );
    }
	private float GetNoise()
    {
       
        // Set the x values of each call to 'GetNoise2d' to a different value
        // so that our x and y vectors will be reading from unrelated areas of noise
        return 
            noise.GetNoise2D(200, noise_i) * magnitude_rot;
          
        
    }
}

