using Godot;
using System;
using System.Linq;
using System.Reflection;
public partial class BoolProp : Panel
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
		var bl=(bool)prop.GetValue(_E);
		
			GetNode<CheckBox>("Toggle").ButtonPressed=bl;
		
		
		
	}
	void Confirm()
	{
		
		
		
			Type t = _E.GetType(); PropertyInfo prop = t.GetProperty(GetNode<Label>("PropName").Text);
		prop.SetValue(_E,	GetNode<CheckBox>("Toggle").ButtonPressed);
		EventPanel.UpdateStream();
		
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
}
