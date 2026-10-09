using Godot;
using System;
using System.Linq;
using System.Reflection;
public partial class NumberProp : Panel
{
// Called when the node enters the scene tree for the first time.
	long uid;
	Control panel;
	editor.Event _E;
	PropertyInfo prop;
	public override void _Ready()
	{
		panel=editor.prop_panel;
		uid= (long)editor.prop_list_uid.Position.X;
		_E=editor.SelectedEvents.Where(e=>e.uid==uid).Select(e=>e).First();

		Type t = _E.GetType(); //
		//PropertyInfo prop = t.GetProperties().Where(p=>p.Name==GetNode<Label>("PropName").Text).Select(p=>p).First();
		GD.Print(GetNode<Label>("PropName").Text);

		PropertyInfo prop = t.GetProperty(GetNode<Label>("PropName").Text);
		var num=(float)prop.GetValue(_E);
		
			GetNode<SpinBox>("Input").Value=num;
		
		
		
	}
	void Confirm(float value)
	{
		
		
		
			Type t = _E.GetType(); PropertyInfo prop = t.GetProperty(GetNode<Label>("PropName").Text);
		prop.SetValue(_E,value);
		EventPanel.UpdateStream();
		
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
}
