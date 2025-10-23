using Godot;

public class ExportEntry : Panel
{
	public enum EntryTypeEnum
	{
		GRAY,
		RGB,
		RGBA,
		RGB_A,
		R_G_B,
		R_G_B_A
	}

	public enum SingleChannelContentEnum
	{
		BLACK,
		WHITE,
		COLOR_GRAYSCALE,
		COLOR_ALPHA,
		ROUGHNESS,
		METALLICITY,
		HEIGHT,
		EMISSION_GRAYSCALE,
		ROUGHNESS_INV,
		METALLICITY_INV,
		HEIGHT_INV
	}

	public enum FullChannelContentEnum
	{
		BLACK,
		WHITE,
		COLOR,
		NORMAL_OPENGL,
		NORMAL_DIRECTX,
		EMISSION
	}

	private TextureButton checkBox;

	[Export(PropertyHint.None, "")]
	private bool checkBoxPressed = true;

	private OptionButton entryTypeOptionButton;

	[Export(PropertyHint.None, "")]
	private EntryTypeEnum entryType = EntryTypeEnum.RGBA;

	private LineEdit suffixLineEdit;

	[Export(PropertyHint.None, "")]
	private string suffix = "";

	private OptionButton rgbOptionButton;

	[Export(PropertyHint.None, "")]
	private FullChannelContentEnum rgbContent;

	private OptionButton rOptionButton;

	[Export(PropertyHint.None, "")]
	private SingleChannelContentEnum rContent;

	private OptionButton gOptionButton;

	[Export(PropertyHint.None, "")]
	private SingleChannelContentEnum gContent;

	private OptionButton bOptionButton;

	[Export(PropertyHint.None, "")]
	private SingleChannelContentEnum bContent;

	private OptionButton aOptionButton;

	[Export(PropertyHint.None, "")]
	private SingleChannelContentEnum aContent = SingleChannelContentEnum.WHITE;

	private TextureButton deleteButton;

	public bool CheckBoxPressed
	{
		get
		{
			return checkBoxPressed;
		}
		set
		{
			checkBoxPressed = value;
			checkBox.Pressed = checkBoxPressed;
		}
	}

	public EntryTypeEnum EntryType
	{
		get
		{
			return entryType;
		}
		set
		{
			entryType = value;
			entryTypeOptionButton.Select((int)entryType);
			EntryTypeChanged(entryTypeOptionButton.Selected);
		}
	}

	public string Suffix
	{
		get
		{
			return suffix;
		}
		set
		{
			suffix = value;
			suffixLineEdit.Text = suffix;
		}
	}

	public FullChannelContentEnum RgbContent
	{
		get
		{
			return rgbContent;
		}
		set
		{
			rgbContent = value;
			rgbOptionButton.Select((int)rgbContent);
		}
	}

	public SingleChannelContentEnum RContent
	{
		get
		{
			return rContent;
		}
		set
		{
			rContent = value;
			rOptionButton.Select((int)rContent);
		}
	}

	public SingleChannelContentEnum GContent
	{
		get
		{
			return gContent;
		}
		set
		{
			gContent = value;
			gOptionButton.Select((int)gContent);
		}
	}

	public SingleChannelContentEnum BContent
	{
		get
		{
			return bContent;
		}
		set
		{
			bContent = value;
			bOptionButton.Select((int)bContent);
		}
	}

	public SingleChannelContentEnum AContent
	{
		get
		{
			return aContent;
		}
		set
		{
			aContent = value;
			aOptionButton.Select((int)aContent);
		}
	}

	public override void _Ready()
	{
		base._Ready();
		checkBox = GetChildOrNull<TextureButton>(0);
		checkBox.Pressed = checkBoxPressed;
		checkBox.Connect(Signals.Toggled, this, "CheckBoxToggled");
		entryTypeOptionButton = GetChildOrNull<OptionButton>(1);
		entryTypeOptionButton.AddItem("Gray");
		entryTypeOptionButton.AddItem("RGB");
		entryTypeOptionButton.AddItem("RGBA");
		entryTypeOptionButton.AddItem("RGB+A");
		entryTypeOptionButton.AddItem("R+G+B");
		entryTypeOptionButton.AddItem("R+G+B+A");
		entryTypeOptionButton.Select((int)entryType);
		entryTypeOptionButton.Connect(Signals.ItemSelected, this, "EntryTypeChanged");
		suffixLineEdit = GetChildOrNull<LineEdit>(2);
		suffixLineEdit.Text = suffix;
		suffixLineEdit.Connect(Signals.TextChanged, this, "SuffixTextEntered");
		rgbOptionButton = GetChildOrNull<HBoxContainer>(3).GetChildOrNull<OptionButton>(0);
		AddChannelOptions(rgbOptionButton, singleChannel: false);
		rgbOptionButton.Select((int)rgbContent);
		rgbOptionButton.Connect(Signals.ItemSelected, this, "RgbOptionButtonItemSelected");
		rOptionButton = GetChildOrNull<HBoxContainer>(3).GetChildOrNull<OptionButton>(1);
		AddChannelOptions(rOptionButton, singleChannel: true);
		rOptionButton.Select((int)rContent);
		rOptionButton.Connect(Signals.ItemSelected, this, "ROptionButtonItemSelected");
		gOptionButton = GetChildOrNull<HBoxContainer>(3).GetChildOrNull<OptionButton>(2);
		AddChannelOptions(gOptionButton, singleChannel: true);
		gOptionButton.Select((int)gContent);
		gOptionButton.Connect(Signals.ItemSelected, this, "GOptionButtonItemSelected");
		bOptionButton = GetChildOrNull<HBoxContainer>(3).GetChildOrNull<OptionButton>(3);
		AddChannelOptions(bOptionButton, singleChannel: true);
		bOptionButton.Select((int)bContent);
		bOptionButton.Connect(Signals.ItemSelected, this, "BOptionButtonItemSelected");
		aOptionButton = GetChildOrNull<HBoxContainer>(3).GetChildOrNull<OptionButton>(4);
		AddChannelOptions(aOptionButton, singleChannel: true);
		aOptionButton.Select((int)aContent);
		aOptionButton.Connect(Signals.ItemSelected, this, "AOptionButtonItemSelected");
		deleteButton = GetChildOrNull<TextureButton>(4);
		deleteButton.Connect(Signals.Pressed, this, "Delete");
		EntryTypeChanged(entryTypeOptionButton.Selected);
	}

	private void AddChannelOptions(OptionButton optionButton, bool singleChannel)
	{
		if (singleChannel)
		{
			optionButton.AddItem("Black");
			optionButton.AddItem("White");
			optionButton.AddItem("Color Grayscale");
			optionButton.AddItem("Color Alpha");
			optionButton.AddItem("Roughness");
			optionButton.AddItem("Metallicity");
			optionButton.AddItem("Height");
			optionButton.AddItem("Emission Grayscale");
			optionButton.AddItem("Roughness Inverse");
			optionButton.AddItem("Metallicity Inverse");
			optionButton.AddItem("Height Inverse");
		}
		else
		{
			optionButton.AddItem("Black");
			optionButton.AddItem("White");
			optionButton.AddItem("Color");
			optionButton.AddItem("Normal OpenGL");
			optionButton.AddItem("Normal DirectX");
			optionButton.AddItem("Emission");
		}
	}

	public void CheckBoxToggled(bool pressed)
	{
		checkBoxPressed = pressed;
	}

	public void EntryTypeChanged(int index)
	{
		entryType = (EntryTypeEnum)index;
		switch (entryType)
		{
		case EntryTypeEnum.GRAY:
			rgbOptionButton.Visible = false;
			rOptionButton.Visible = true;
			gOptionButton.Visible = false;
			bOptionButton.Visible = false;
			aOptionButton.Visible = false;
			break;
		case EntryTypeEnum.RGB:
		case EntryTypeEnum.RGBA:
			rgbOptionButton.Visible = true;
			rOptionButton.Visible = false;
			gOptionButton.Visible = false;
			bOptionButton.Visible = false;
			aOptionButton.Visible = false;
			break;
		case EntryTypeEnum.RGB_A:
			rgbOptionButton.Visible = true;
			rOptionButton.Visible = false;
			gOptionButton.Visible = false;
			bOptionButton.Visible = false;
			aOptionButton.Visible = true;
			break;
		case EntryTypeEnum.R_G_B:
			rgbOptionButton.Visible = false;
			rOptionButton.Visible = true;
			gOptionButton.Visible = true;
			bOptionButton.Visible = true;
			aOptionButton.Visible = false;
			break;
		case EntryTypeEnum.R_G_B_A:
			rgbOptionButton.Visible = false;
			rOptionButton.Visible = true;
			gOptionButton.Visible = true;
			bOptionButton.Visible = true;
			aOptionButton.Visible = true;
			break;
		}
	}

	public void SuffixTextEntered(string text)
	{
		suffix = text;
	}

	public void RgbOptionButtonItemSelected(int index)
	{
		rgbContent = (FullChannelContentEnum)index;
	}

	public void ROptionButtonItemSelected(int index)
	{
		rContent = (SingleChannelContentEnum)index;
	}

	public void GOptionButtonItemSelected(int index)
	{
		gContent = (SingleChannelContentEnum)index;
	}

	public void BOptionButtonItemSelected(int index)
	{
		bContent = (SingleChannelContentEnum)index;
	}

	public void AOptionButtonItemSelected(int index)
	{
		aContent = (SingleChannelContentEnum)index;
	}

	public void Delete()
	{
		QueueFree();
	}
}
