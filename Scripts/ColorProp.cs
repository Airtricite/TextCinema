using Godot;
using System;
using System.Linq;
using System.Reflection;
using static ElementExplorer;
public partial class ColorProp : Panel
{
	
	long uid;
	Control panel;
	editor.Event _E;
	PropertyInfo prop;
	ColorPicker picker;
	public override void _Ready()
	{
		panel=editor.prop_panel;
		uid= (long)editor.prop_list_uid.Position.X;
		_E=editor.SelectedEvents.Where(e=>e.uid==uid).Select(e=>e).First();

		Type t = _E.GetType(); //
		//PropertyInfo prop = t.GetProperties().Where(p=>p.Name==GetNode<Label>("PropName").Text).Select(p=>p).First();
		GD.Print(GetNode<Label>("PropName").Text);

		PropertyInfo prop = t.GetProperty(GetNode<Label>("PropName").Text);
		var clr=(Color)prop.GetValue(_E);
		GetNode<ColorRect>("Indicator").Color=clr;
			//GetNode<LineEdit>("Input").Text=num.ToString();
		picker = GetTree().Root.GetNode<ColorPicker>("Editor/ColorPicker/ColorPicker");
		picker.Color=clr;
		
	}
	bool selecting=false;

	void Apply()
	{
		//_E.button.Modulate=
	
				Type t = _E.GetType(); //
		PropertyInfo prop = t.GetProperty(GetNode<Label>("PropName").Text);
		prop.SetValue(_E,picker.Color);
		GetNode<ColorRect>("Indicator").Color=picker.Color;
		picker.GetParent<Window>().Visible=false;
		EventPanel.UpdateStream();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
}
