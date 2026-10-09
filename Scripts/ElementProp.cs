using Godot;
using System;
using System.Linq;
using System.Reflection;
using static ElementExplorer;
public partial class ElementProp : Panel
{
	
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
		if (((editor.ElementEvent)_E).element==null)
		{
			return;
		}
		PropertyInfo prop = t.GetProperty(GetNode<Label>("PropName").Text);
		var element=(editor.Element)prop.GetValue(_E);
		string node_info="";

		if(element!=null&&editor.IsValid(element.Node))
		{
			node_info=" and node: "+element.Node;
		}
		GD.PrintErr("The Element of "+_E.uid+" is "+element+node_info);
		
		GetNode<Marker2D>("Element/euid").Position=Vector2.Right*((editor.ElementEvent)_E).element.euid;
			//GetNode<LineEdit>("Input").Text=num.ToString();
		//
		
		
	}
	bool selecting=false;
	void Expand(){
		GD.Print("Expand");
		var node=GetNode<Button>("Element");
		node.Visible=!node.Visible;
	var e=((editor.ElementEvent)_E).element;
	if (e==null)
	{
		return;
	}
		var typename="";
			switch (e.type)
		{
			
			default:
			case ElementType.Image:typename="🖼Image";break;
			case ElementType.Animation:typename="🎞Animation";break;
			case ElementType.Video:typename="📽Video";break;
			case ElementType.Text:typename="📃Text";break;
			case ElementType.Custom:typename="Custom";break;
			case ElementType.Progress:typename="ProgressBar";break;
		}
			
		GetNode<Button>("Element").Text="["+e.euid+"]"+"["+typename+"]\n"+e.Name;
	}
	void Select()
	{
		selecting=!selecting;
		if (selecting)
		{
			editor.camera.Position=Vector2.Left*1400;
			GetNode<Button>("Select").Text="Apply";

		}else
		{
			GetNode<Button>("Select").Text="Select";
			Type t = ((editor.ElementEvent)_E).GetType(); PropertyInfo prop = t.GetProperty(GetNode<Label>("PropName").Text);
			prop.SetValue(_E,editor.SelectedElements.First());
			GetNode<Marker2D>("Element/euid").Position=Vector2.Right*((editor.ElementEvent)_E).element.euid;
			if (((editor.ElementEvent)_E).element!=null)
			{
				WriteFrame();
			}
			
		}
		
		//prop.SetValue(_E,);
		EventPanel.UpdateStream();
		editor.Element e=null;
		long euid=(long)GetNode<Marker2D>("Element/euid").Position.X;
		foreach (var _e in editor.Elements)
		{
			if (_e.euid==euid)
			{
				e=_e;
				
			}
		}
		if (e==null)
	{
		return;
	}
		var typename="";
			switch (e.type)
		{
			
			default:
			case ElementType.Image:typename="🖼Image";break;
			case ElementType.Animation:typename="🎞Animation";break;
			case ElementType.Video:typename="📽Video";break;
			case ElementType.Text:typename="📃Text";break;
			case ElementType.Custom:typename="Custom";break;
			case ElementType.Progress:typename="ProgressBar";break;
		}
			
		GetNode<Button>("Element").Text="["+typename+"]\n"+e.Name;
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	//void write(){WriteFrame((long)GetNode<Marker2D>("../euid").Position.X);}
}
