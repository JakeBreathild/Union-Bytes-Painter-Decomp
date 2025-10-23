using System;
using Godot;

public class ChannelButton : OptionButton
{
	private Popup popup;

	[Export(PropertyHint.None, "")]
	private bool includeFull;

	[Export(PropertyHint.None, "")]
	private bool includeNormal;

	private Action<Data.ChannelEnum> channelSelectedCallback;

	public Action<Data.ChannelEnum> ChannelSelectedCallback
	{
		get
		{
			return channelSelectedCallback;
		}
		set
		{
			channelSelectedCallback = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		if (includeFull)
		{
			AddItem("Full");
		}
		AddItem("Color");
		AddItem("Roughness");
		AddItem("Metallicity");
		AddItem("Height");
		if (includeNormal)
		{
			AddItem("Normal");
		}
		AddItem("Emission");
		Select(0);
		Connect(Signals.ItemSelected, this, "ItemSelected");
		popup = GetPopup();
		popup.Connect(Signals.AboutToShow, this, "PopupShow");
		popup.Connect(Signals.Hide, this, "PopupHide");
	}

	public void PopupShow()
	{
		InputManager.MouseEnteredUserInterface();
	}

	public void PopupHide()
	{
		InputManager.MouseExitedUserInterface();
		InputManager.SkipInput = true;
	}

	public Data.ChannelEnum GetChannel()
	{
		int index = base.Selected;
		if (!includeFull)
		{
			index++;
		}
		if (!includeNormal && index > 4)
		{
			index++;
		}
		return (Data.ChannelEnum)index;
	}

	public void SetChannel(Data.ChannelEnum channel)
	{
		int index = (int)channel;
		if (!includeFull)
		{
			if (channel == Data.ChannelEnum.FULL)
			{
				return;
			}
			index--;
		}
		if (!includeNormal)
		{
			if (channel == Data.ChannelEnum.NORMAL)
			{
				return;
			}
			if (index > 4)
			{
				index--;
			}
		}
		Select(index);
	}

	public void ItemSelected(int index)
	{
		if (channelSelectedCallback != null)
		{
			if (!includeFull)
			{
				index++;
			}
			if (!includeNormal && index > 4)
			{
				index++;
			}
			channelSelectedCallback((Data.ChannelEnum)index);
		}
	}
}
