using System.Collections.Generic;
using Godot;

public class ChannelsContainer : DefaultContainer
{
	private List<ChannelPreview> channelPanelsList = new List<ChannelPreview>();

	public override void _Ready()
	{
		base._Ready();
		for (int i = 0; i < GetChildCount(); i++)
		{
			if (GetChildOrNull<ChannelPreview>(i) != null)
			{
				ChannelPreview channelPanel = GetChildOrNull<ChannelPreview>(i);
				channelPanelsList.Add(channelPanel);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < channelPanelsList.Count; i++)
		{
			channelPanelsList[i].Reset();
		}
	}

	public void ChangeChannelSelected(int index, bool enable)
	{
		channelPanelsList[index].ChangeSelected(enable);
	}

	public void ChangeAllChannelsSelected(bool enable)
	{
		for (int i = 0; i < channelPanelsList.Count; i++)
		{
			channelPanelsList[i].ChangeSelected(enable);
		}
	}

	public void SetChannelTexture(int index, Texture texture, bool updateAspectRatio = false)
	{
		channelPanelsList[index].SetTexture(texture, updateAspectRatio);
	}

	public void SetChannelTexture(Data.ChannelEnum channel, Texture texture, bool updateAspectRatio = false)
	{
		channelPanelsList[(int)(channel - 1)].SetTexture(texture, updateAspectRatio);
	}
}
