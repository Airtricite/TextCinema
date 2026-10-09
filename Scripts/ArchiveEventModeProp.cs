using Godot;
using System;
using System.Linq;
using System.Reflection;

public partial class ArchiveEventModeProp : Panel
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

		PropertyInfo prop = t.GetProperty("Mode");
		var mode=(editor.ArchiveEventMode)prop.GetValue(_E);
		switch (mode)
		{
			
			default:
			case editor.ArchiveEventMode.Save:GetNode<OptionButton>("Edit").Selected=0;
			break;
			case editor.ArchiveEventMode.Load:GetNode<OptionButton>("Edit").Selected=1;
			break;
			case editor.ArchiveEventMode.New:GetNode<OptionButton>("Edit").Selected=2;
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
			case 0:((editor.ArchiveEvent)_E).Mode=editor.ArchiveEventMode.Save;
			break;
			case 1:((editor.ArchiveEvent)_E).Mode=editor.ArchiveEventMode.Load;
			break;
			case 2:((editor.ArchiveEvent)_E).Mode=editor.ArchiveEventMode.New;
			break;
		}
	}
}
