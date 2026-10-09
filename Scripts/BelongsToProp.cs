using Godot;
using System;
using System.Linq;
using System.Reflection;

public partial class BelongsToProp : Panel
{long uid;
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
		//GD.Print(GetNode<Label>("PropName").Text);

		PropertyInfo prop = t.GetProperty("belongs_to");
		var where=(editor.Where)prop.GetValue(_E);
		switch (where)
		{
			
			default:
			case editor.Where.General:GetNode<OptionButton>("Edit").Selected=0;
			break;
			case editor.Where.BaseLine:GetNode<OptionButton>("Edit").Selected=1;
			break;
			case editor.Where.Camera:GetNode<OptionButton>("Edit").Selected=2;
			break;
		}

			//GetNode<LineEdit>("Input").Text=num.ToString();
		
		
		
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	void Selected(int id)
	{
		switch (id)
		{
			
			default:
			case 0:((editor.ElementEvent)_E).belongs_to=editor.Where.General;
			break;
			case 1:((editor.ElementEvent)_E).belongs_to=editor.Where.BaseLine;
			break;
			case 2:((editor.ElementEvent)_E).belongs_to=editor.Where.Camera;
			break;
		}
	}
}
