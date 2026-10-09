using Godot;
using System;
using System.Linq;
using System.Reflection;
using static ElementExplorer;
public partial class ContentColorProp : Panel
{

	long uid;
	Control panel;
	editor.Event _E;
	CanvasItem _e;
	PropertyInfo prop;
	Node Origin;
	Window Preview;
	ColorPicker picker;
	dynamic VarTo<[MustBeVariant] T>(object obj)
	{
		if (obj is T)
		{return obj;}
		return ((Variant)obj).As<T>();
	} 
	public override void _Ready()
	{
		panel = editor.prop_panel;
		uid = (long)editor.prop_list_uid.Position.X;
		_E = editor.SelectedEvents.Where(e => e.uid == uid).Select(e => e).First();

		Type t = _E.GetType(); //
		var clr=((editor.ElementEvent)_E).ContentColor;
		GetNode<ColorRect>("Indicator").Color =clr;
		//GetNode<LineEdit>("Input").Text=num.ToString();

		Origin = editor.root.GetNode("ContentPreview/Panel/Origin");

		picker = GetTree().Root.GetNode<ColorPicker>("Editor/ColorPicker/ColorPicker");
		picker.Color=clr;
	}
	bool selecting = false;
	public override void _PhysicsProcess(double delta)
	{
		if (_e != null && picker != null)
		{
			_e.Modulate = picker.Color;
		}
	}

	void Edit()//popup color picker, popup preview
	{
		if (Preview == null)
		{ Preview = (Window)editor.root.GetNode("ContentPreview"); }
		ColorPicker();

		//var window=editor.root.GetNode<Window>("ContentPreview");
		if (_E is editor.ElementEvent)
		{
			var _EE = (editor.ElementEvent)_E;
			if (_EE.element != null && _EE.element.type != ElementType.Video && _EE.element.type != ElementType.Custom &&_EE.element.Node!=null)
			{
				Preview.Popup();
				var e = _EE.element.Node;
				_e = (CanvasItem)_EE.element.Node.Duplicate();
				switch (_EE.element.type)
				{
					default://pos, rot
					case ElementType.Image:
						{
							GD.Print("[selected.FrameData.Count]:" + _EE.element.FrameData.Count);
							if (_EE.element.FrameData.Count > 0)
							{
								var tex = new PortableCompressedTexture2D();
								tex._Data = (byte[])_EE.element.FrameData.ElementAt(0).Clone();

								tex.KeepCompressedBuffer = false;
								_e.GetChild<TextureRect>(0).Texture = tex;
							}
							var material = e.GetChild<TextureRect>(0).Material;
							if (material != null)
							{ _e.GetChild<TextureRect>(0).Material = (Material)material.Duplicate(); }
						}
						break;
					case ElementType.Text:
						break;
					case ElementType.Progress:

						break;
					case ElementType.Animation:
						{
							((AnimatedSprite2D)_e).SpriteFrames = (SpriteFrames)((AnimatedSprite2D)e).SpriteFrames.Duplicate();
							((AnimatedSprite2D)_e).SpriteFrames.Clear("default");
							foreach (var frame in _EE.element.FrameData)
							{
								var tex = new PortableCompressedTexture2D();
								tex._Data = (byte[])frame.Clone();
								GD.Print(tex);
								tex.KeepCompressedBuffer = false;
								((AnimatedSprite2D)_e).SpriteFrames.AddFrame("default", tex);
							}
							var material = ((AnimatedSprite2D)e).Material;
							if (material != null)
							{((AnimatedSprite2D)_e).Material = (Material)material.Duplicate();}
						}
						break;
				}
				Origin.AddChild(_e);

			}
		}

	}
	//color picker changed, change preview
	void Apply()
	{
		//_E.button.Modulate=
		if (_E is editor.ElementEvent)
		{
			var _EE = (editor.ElementEvent)_E;
			_EE.ContentColor = picker.Color;

			var n=_EE.element.Node;
			if (n!=null)
			{
				n.Modulate= picker.Color;
			}
			
		}

		GetNode<ColorRect>("Indicator").Color = picker.Color;
		ClosePicker();
		Preview.Visible = false;
		_e = null;
		Origin.GetChildren().All(n => { n.QueueFree(); return true; });
		EventPanel.UpdateStream();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
}
