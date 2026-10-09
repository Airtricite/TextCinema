using Godot;
using System;

public partial class GeneralTextUnit : Control
{
	// Called when the node enters the scene tree for the first time.
	RichTextLabel TextCase;
	public RichTextLabel HeightMeasurer;
	Marker2D tweened_val;
	public override void _Ready()
	{
		TextCase = GetNode<RichTextLabel>("TextCase");
		tweened_val=GetNode<Marker2D>("_TweenedVal");
		HeightMeasurer = new RichTextLabel();
		HeightMeasurer.Modulate=new Color(1,1,1,0);
        HeightMeasurer.Text = TextCase.Text;
        HeightMeasurer.Size = new Vector2(0, 0);
        HeightMeasurer.VisibleCharacters = -1;
        HeightMeasurer.FitContent = true;
		HeightMeasurer.AddThemeFontSizeOverride("normal_font_size", (int)tweened_val.Position.Y);
		GetTree().Root.AddChild(HeightMeasurer);

	}
	int GetAdjustingHeight(int h)//h=target height of textcase
	{
         

        // 计算 Control_1 的高度
        float H = (float)((h - TextCase.OffsetTop - TextCase.OffsetBottom) / Mathf.Clamp(TextCase.AnchorBottom - TextCase.AnchorTop,0.1,2));

        // 设置 Control_1 的高度
        return (int)MathF.Ceiling(H);

	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		///auto adjust according to text_case
		HeightMeasurer.CustomMinimumSize=new Vector2(TextCase.Size.X,0);
		GetParent<Control>().CustomMinimumSize=Vector2.Down*GetAdjustingHeight((int)HeightMeasurer.Size.Y)*tweened_val.Position.X;
		
	}
}
