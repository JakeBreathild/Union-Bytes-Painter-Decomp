public interface ICommand
{
	string Name { get; set; }

	Data Data { get; }

	int Type { get; }

	void Execute();

	void Undo();

	void Redo();
}
