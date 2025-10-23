using System.Collections.Generic;
using Godot;

public class UvNodeCommand : ICommand
{
	private struct Node
	{
		public PreviewspaceMeshManager.UvNode UvNode;

		public Vector2 Position;
	}

	private Data data;

	private PreviewspaceMeshManager previewMeshManager;

	private History.CommandTypeEnum type = History.CommandTypeEnum.UVNODE;

	private string name = "UV Node";

	private List<Node> nodesList;

	public Data Data => data;

	public PreviewspaceMeshManager PreviewMeshManager => previewMeshManager;

	public int Type => (int)type;

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public UvNodeCommand(Worksheet worksheet)
	{
		previewMeshManager = Register.PreviewspaceMeshManager;
		data = worksheet.Data;
		nodesList = new List<Node>();
	}

	public void AddNode(PreviewspaceMeshManager.UvNode uvNode)
	{
		Node node = new Node
		{
			UvNode = uvNode,
			Position = uvNode.Position
		};
		nodesList.Add(node);
	}

	public void Execute()
	{
	}

	public void Undo()
	{
		for (int i = 0; i < nodesList.Count; i++)
		{
			Node node = nodesList[i];
			node.Position = node.UvNode.Position;
			nodesList[i].UvNode.ChangePosition(nodesList[i].Position);
			Transform transform = Transform.Identity;
			transform.origin.x = nodesList[i].Position.x * (float)data.Width * 0.25f;
			transform.origin.z = nodesList[i].Position.y * (float)data.Height * 0.25f;
			previewMeshManager.UvNodesMultiMesh.SetInstanceTransform(nodesList[i].UvNode.InstanceIndex, transform);
			if ((Mathf.PosMod(nodesList[i].UvNode.Position.x * (float)data.Width, 1f) < 0.05f || Mathf.PosMod(nodesList[i].UvNode.Position.x * (float)data.Width, 1f) > 0.95f) && (Mathf.PosMod(nodesList[i].UvNode.Position.y * (float)data.Height, 1f) < 0.05f || Mathf.PosMod(nodesList[i].UvNode.Position.y * (float)data.Height, 1f) > 0.95f))
			{
				previewMeshManager.UvNodesMultiMesh.SetInstanceColor(nodesList[i].UvNode.InstanceIndex, Settings.NodeColor);
			}
			else
			{
				previewMeshManager.UvNodesMultiMesh.SetInstanceColor(nodesList[i].UvNode.InstanceIndex, Settings.NodeErrorColor);
			}
			nodesList[i] = node;
		}
		previewMeshManager.CreateUvLayout();
		previewMeshManager.CreateMesh();
	}

	public void Redo()
	{
		Undo();
	}
}
