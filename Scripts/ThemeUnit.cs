using Godot;
using System;

public partial class ThemeUnit : Button
{
	void Select()
	{
		 CustomTheme.DeselectAll();
		CustomTheme.selected_tuid=(long)GetNode<Marker2D>("tuid").Position.X;
		GetNode<ReferenceRect>("Highlight").Visible=true;
		CustomTheme.ThemeSelected();
	}
	
}
