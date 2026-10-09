using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
public partial class ControlWindow : Window
{
	// Called when the node enters the scene tree for the first time.
	bool opened = false;
	static Control Units;
	static Control unit;
public static 	editor.Event _e;
	static editor.Element _element;
	public static long sel_uid=0;
	public static dynamic VarToData(object obj)
	{
		if (obj is Variant)
		{
			var variant=(Variant)obj;
		switch (variant.VariantType)
		{
			case Variant.Type.Float:
				return variant.As<float>();
			case Variant.Type.Vector2:
				return variant.As<Vector2>();
			case Variant.Type.Color:
				return variant.As<Color>();
		}
		return null;
		}else{
			return obj;
		}
	}
	static int LongToInt(object data)
	{  
		string str=data.ToString();
		return int.Parse(str);
	}
	public override void _Ready()
	{
		Units = GetNode<Control>("TweenPanel/Scr/Grid");
		unit = GetNode<Control>("TweenPanel/Unit");
	}
	void Open()
	{
		opened = !opened;
		if (opened)
		{
			//var uid = GetTree().Root.GetNode<Marker2D>("Editor/PropertyPanel/current_uid").Position.X;
			_e = editor._select_event(sel_uid);
			Popup();
			Edit();
		}
		else
		{
			Visible = false;
			_e = null;
			_element = null;
		}

	}
	void OpenTweenSheet()
	{
		GetNode<Window>("TweenChart").Popup();
	}
	void CloseTweenSheet()
	{
		GetNode<Window>("TweenChart").Visible=false;
	}
	int _i=0;
    /*public override void _PhysicsProcess(double delta)
    {	
		if (_i%6==0&&_e!=null)
		{
			//	GD.PrintErr("progress: "+(((editor.ElementEvent)_e).element_props)["progress"]);
			GD.PrintErr("uid: "+(((editor.ElementEvent)_e).uid));
		}
		_i++;
    }*/
	static bool enable_by_default=true;
    public static void Edit()
	{
		Units.GetChildren().All(c => { c.QueueFree(); return true; });
		if (_e == null) return;

		Type t = _e.GetType(); PropertyInfo[] properties = t.GetProperties();

		Type[] Types = { typeof(float), typeof(Color), typeof(Vector2) };
		if (_e is editor.ElementEvent)
		{
			//GD.Print("label '_e is element event'");
			
			_element = ((editor.ElementEvent)_e).element;
			if (_element != null)
			{
				var prop_e = ((editor.ElementEvent)_e).element_props;
				//GD.Print("label 'element'");
					
				foreach (string prop_name in prop_e.Keys)
				{
					
					var prop = VarToData(prop_e[prop_name]);
					if (prop != null)
					{
						Type t_ep = prop.GetType();//Type of element prop
						GD.Print("label name:" + prop_name);
						GD.Print("label t_ep:" + t_ep+"\n");
						if (t_ep == Types[0] || t_ep == Types[1])
						{
							//GD.Print("label 'foreach element prop&&if t=0/1'");
							var u = (Panel)unit.Duplicate();
							u.GetNode<Label>("PropName").Text = prop_name;
							var dur = 0.001;
							var opt = 0;
							var ease_type = 0;
							var enabled = enable_by_default;

							if (_e.tweens != null && _e.tweens.ContainsKey(prop_name))
							{
								GD.Print("label 'Read Data'");
								var tween_prop = (List<object>)_e.tweens[prop_name];
								dur = (double)tween_prop.ElementAt(2);
								opt = LongToInt(tween_prop.ElementAt(0));
								ease_type = LongToInt(tween_prop.ElementAt(1));
								enabled = (bool)tween_prop.ElementAt(4);
							}
							u.GetNode<SpinBox>("Duration").Value = dur;
							u.GetNode<CheckBox>("IsElementProp").ButtonPressed = true;
							u.GetNode<OptionButton>("TweenOptions").Selected = opt;
							u.GetNode<OptionButton>("EaseType").Selected = ease_type;
							u.GetNode<CheckBox>("Enabled").ButtonPressed = enabled;
							Units.AddChild(u);
							u.Visible = true;
						}
						else if (t_ep == Types[2])
						{
							GD.Print("label 'foreach element prop&&if t=2'");
							string[] prtl = new string[] { ".x", ".y" };//if get extension="x" then...
							foreach (var xy in prtl)
							{
								var u = (Panel)unit.Duplicate();

								u.GetNode<Label>("PropName").Text = prop_name + xy;
								var dur = 0.001;
								var opt = 0;
								var ease_type = 0;
								var enabled = enable_by_default;
								if (_e.tweens != null && _e.tweens.ContainsKey(prop_name + xy))
								{
									var tween_prop = (List<object>)_e.tweens[prop_name + xy];
									dur = (double)tween_prop.ElementAt(2);
									opt =LongToInt(tween_prop.ElementAt(0));
									ease_type = LongToInt(tween_prop.ElementAt(1));
									enabled = (bool)tween_prop.ElementAt(4);

								}
								u.GetNode<SpinBox>("Duration").Value = dur;
								u.GetNode<CheckBox>("IsElementProp").ButtonPressed = true;
								u.GetNode<OptionButton>("TweenOptions").Selected = opt;
								u.GetNode<OptionButton>("EaseType").Selected = ease_type;
								u.GetNode<CheckBox>("Enabled").ButtonPressed = enabled;
								Units.AddChild(u); u.Visible = true;
							}

						}
					}
				}
			}
		
		}
		foreach (var prop in properties)
		{
			if (
				prop.Name == "uid" ||
				prop.Name == "button" ||
				prop.Name == "Entity" ||
				prop.Name == "frame_stamp" ||
				prop.Name == "posX" ||
				prop.Name == "type" ||
				prop.Name == "Color"//||
									//prop.Name=="CameraRotation"
				)
			{
				//do nothing
			}
			else
			{
				//GD.Print("label 'foreach props'");
				var prop_t = prop.PropertyType;

				//Control[] units = { str_prop, num_prop, color_prop, elem_prop,belongs_2_prop, bool_prop };
			//	GD.Print("label:" + prop_t.ToString());
				if (prop_t == Types[0] || prop_t == Types[1])
				{
				//	GD.Print("label 'if type'");
					var u = (Panel)unit.Duplicate();
					u.GetNode<Label>("PropName").Text = prop.Name;
					var dur = 0.001;
					var opt = 0;
					var ease_type = 0;
					var enabled = enable_by_default;
					
					if (_e.tweens != null && _e.tweens.ContainsKey(prop.Name))
					{
						var tween_prop = (List<object>)_e.tweens[prop.Name];
						dur = (double)tween_prop.ElementAt(2);
						opt = LongToInt(tween_prop.ElementAt(0));
						ease_type = LongToInt(tween_prop.ElementAt(1));
						enabled = (bool)tween_prop.ElementAt(4);

					}
					u.GetNode<SpinBox>("Duration").Value = dur;
					u.GetNode<CheckBox>("IsElementProp").ButtonPressed = true;
					u.GetNode<OptionButton>("TweenOptions").Selected = opt;
					u.GetNode<OptionButton>("EaseType").Selected = ease_type;
					u.GetNode<CheckBox>("Enabled").ButtonPressed = enabled;
					Units.AddChild(u); u.Visible = true;
				}
				else if (prop_t == Types[2])
				{
					string[] prtl = new string[] { ".x", ".y" };
					foreach (var xy in prtl)
					{
						var u = (Panel)unit.Duplicate();

						u.GetNode<Label>("PropName").Text = prop.Name + xy;
						var dur = 0.001;
						var opt = 0;
						var ease_type = 0;
						var enabled = enable_by_default;
//GD.Print("label: ",_e.tweens != null," ", _e.tweens.ContainsKey(prop.Name));
						if (_e.tweens != null && _e.tweens.ContainsKey(prop.Name+ xy))
						{
//GD.Print("label 'if tween'");
							var tween_prop = (List<object>)_e.tweens[prop.Name + xy];
							dur = (double)tween_prop.ElementAt(2);
							opt = LongToInt(tween_prop.ElementAt(0));
							ease_type = LongToInt(tween_prop.ElementAt(1));
							enabled = (bool)tween_prop.ElementAt(4);
			
						}
						u.GetNode<SpinBox>("Duration").Value = dur;
						u.GetNode<CheckBox>("IsElementProp").ButtonPressed = true;
						u.GetNode<OptionButton>("TweenOptions").Selected = opt;
						u.GetNode<OptionButton>("EaseType").Selected = ease_type;
						u.GetNode<CheckBox>("Enabled").ButtonPressed = enabled;
						Units.AddChild(u); u.Visible = true;
					}

				}


			}
		}




	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public static void Apply()
	{
		//Hashtable vectors=new Hashtable();
		TweenPanel.selected_units.Clear();
	
		foreach (var u in Units.GetChildren().ToList())
		{
			
			
			var full_name = u.GetNode<Label>("PropName").Text;
			var ext = full_name.GetExtension();
			var name = full_name.Substring(0, full_name.Length - ext.Length);


			Type t = _e.GetType(); PropertyInfo property = t.GetProperty(name);
			if (ext == "x" || ext == "y")
			{

				
				List<object> tprop = new List<object>
				{
					u.GetNode<OptionButton>("TweenOptions").Selected,
					u.GetNode<OptionButton>("EaseType").Selected,
					u.GetNode<SpinBox>("Duration").Value,
					u.GetNode<CheckBox>("IsElementProp").ButtonPressed,
 					u.GetNode<CheckBox>("Enabled").ButtonPressed
				};
				_e.tweens[full_name] = tprop;

				GD.Print("label 'list' ",full_name,"   ",tprop[4]);

			}else
			{
				List<object> tprop = new List<object>
				 {
					u.GetNode<OptionButton>("TweenOptions").Selected,
					u.GetNode<OptionButton>("EaseType").Selected,
					u.GetNode<SpinBox>("Duration").Value,
					u.GetNode<CheckBox>("IsElementProp").ButtonPressed,
					u.GetNode<CheckBox>("Enabled").ButtonPressed
				};
				//GD.Print("Write Single/Color:" + name);
				_e.tweens[full_name] = tprop;
			}

		}

		//if event is elementevent
		
		_e = null;
		_element = null;
		
	
	}
	void Close()
	{
		Visible = false;
		opened=false;
	}
	
	/*var tween = GetTree().CreateTween().BindNode(this).SetTrans(Tween.TransitionType.Elastic);
	void 
tween.TweenProperty(GetNode("Sprite"), "modulate", Colors.Red, 1.0f);
tween.TweenProperty(GetNode("Sprite"), "scale", Vector2.Zero, 1.0f);*/
}
