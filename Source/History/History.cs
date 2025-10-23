using System;
using System.Collections.Generic;

public class History
{
	public enum CommandTypeEnum
	{
		NONE,
		DRAWING,
		FILTER,
		BAKE,
		UVNODE,
		LAYER,
		SELECTION
	}

	public const int MAX_UNDO_COUNT = 512;

	private Worksheet worksheet;

	private bool isRecording;

	public List<ICommand> commandList;

	private int currentCommandIndex;

	private ICommand currentCommand;

	private Action<string> addCallback;

	private Action<int, int> removeRangeCallback;

	private Action<int> selectCurrentCommandCallback;

	public Worksheet Worksheet => worksheet;

	public History(Worksheet worksheet, Action<string> addCallback, Action<int, int> removeRangeCallback, Action<int> selectCurrentCommandCallback)
	{
		this.worksheet = worksheet;
		commandList = new List<ICommand>();
		this.addCallback = addCallback;
		this.removeRangeCallback = removeRangeCallback;
		this.selectCurrentCommandCallback = selectCurrentCommandCallback;
	}

	public ICommand StartRecording(CommandTypeEnum commandType)
	{
		isRecording = true;
		switch (commandType)
		{
		case CommandTypeEnum.DRAWING:
			currentCommand = new DrawingCommand(worksheet);
			break;
		case CommandTypeEnum.FILTER:
			currentCommand = new FilterCommand(worksheet);
			break;
		case CommandTypeEnum.BAKE:
			currentCommand = new BakeCommand(worksheet);
			break;
		case CommandTypeEnum.UVNODE:
			currentCommand = new UvNodeCommand(worksheet);
			break;
		case CommandTypeEnum.LAYER:
			currentCommand = new LayerCommand(worksheet);
			break;
		case CommandTypeEnum.SELECTION:
			currentCommand = new SelectionCommand(worksheet);
			break;
		}
		return currentCommand;
	}

	public void StopRecording(string historyText = "")
	{
		isRecording = false;
		if (currentCommand != null)
		{
			currentCommand.Execute();
			if (currentCommandIndex < commandList.Count)
			{
				int deltaCount = commandList.Count - currentCommandIndex;
				removeRangeCallback(currentCommandIndex, deltaCount);
				commandList.RemoveRange(currentCommandIndex, deltaCount);
			}
			if (commandList.Count + 1 > 512)
			{
				int deltaCount = commandList.Count + 1 - 512;
				removeRangeCallback(0, deltaCount);
				commandList.RemoveRange(0, deltaCount);
				currentCommandIndex -= deltaCount;
			}
			addCallback(historyText);
			commandList.Add(currentCommand);
			currentCommandIndex++;
			selectCurrentCommandCallback(currentCommandIndex - 1);
			currentCommand = null;
		}
	}

	public void AbortRecording()
	{
		isRecording = false;
		currentCommand = null;
	}

	public ICommand GetCurrentCommand()
	{
		return currentCommand;
	}

	public void UndoLastCommand()
	{
		if (isRecording)
		{
			StopRecording();
		}
		if (commandList.Count > 0 && currentCommandIndex > 0)
		{
			currentCommand = commandList[currentCommandIndex - 1];
			currentCommand?.Undo();
			currentCommand = null;
			currentCommandIndex--;
		}
		selectCurrentCommandCallback(currentCommandIndex - 1);
	}

	public void RedoLastCommand()
	{
		if (isRecording)
		{
			StopRecording();
		}
		if (commandList.Count > 0 && currentCommandIndex < commandList.Count)
		{
			currentCommand = commandList[currentCommandIndex];
			currentCommand?.Redo();
			currentCommand = null;
			currentCommandIndex++;
		}
		selectCurrentCommandCallback(currentCommandIndex - 1);
	}
}
