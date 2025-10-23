using System;
using System.Collections.Generic;
using System.Threading;
using Godot;

public class ThreadsManager : Node
{
	private struct ThreadChunk
	{
		public bool Complete;

		public int ChunkX;

		public int ChunkY;

		public int PositionX;

		public int PositionY;

		public int Width;

		public int Height;

		public float[,] OutputArray;

		public void Clear()
		{
			Complete = false;
			ChunkX = 0;
			ChunkY = 0;
			PositionX = 0;
			PositionY = 0;
			Width = 0;
			Height = 0;
			OutputArray = null;
		}
	}

	private Worksheet worksheet;

	private int threadsCount = 1;

	private System.Threading.Thread[] threads;

	private int threadChunkSize = 32;

	private int threadChunksCount;

	private int threadChunksCounter;

	private int threadChunksArrayWidth;

	private int threadChunksArrayHeight;

	private static Queue<ThreadChunk> threadChunksQueue = new Queue<ThreadChunk>();

	private static System.Threading.Mutex threadChunksQueueMutex = new System.Threading.Mutex();

	private static Queue<ThreadChunk> threadCompleteChunksQueue = new Queue<ThreadChunk>();

	private static System.Threading.Mutex threadCompleteChunksQueueMutex = new System.Threading.Mutex();

	private static Func<int, int, int, int, float[,]> threadFunctionCallback = null;

	private Action<float[,], bool> threadCompleteFunctionCallback;

	private bool isBusy;

	private float[,] outputArray;

	public bool IsBusy => isBusy;

	public ThreadsManager()
	{
		Register.ThreadsManager = this;
	}

	public ThreadsManager(Workspace workspace)
	{
		Register.ThreadsManager = this;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "ThreadsManager";
	}

	public override void _Ready()
	{
		base._Ready();
		threadsCount = Mathf.Max(Mathf.RoundToInt((float)System.Environment.ProcessorCount * 0.5f), 2);
		threads = new System.Threading.Thread[threadsCount];
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (!isBusy)
		{
			return;
		}
		for (int i = 0; i < threadsCount; i++)
		{
			if (threads[i] == null)
			{
				threads[i] = new System.Threading.Thread(Thread);
			}
			if (threads[i].ThreadState == ThreadState.Stopped)
			{
				threads[i].Abort();
				threads[i] = new System.Threading.Thread(Thread);
			}
			if (threads[i].ThreadState == ThreadState.Unstarted)
			{
				threads[i].Start();
			}
		}
		threadCompleteChunksQueueMutex.WaitOne();
		if (threadCompleteChunksQueue.Count > 0)
		{
			while (threadCompleteChunksQueue.Count > 0)
			{
				ThreadChunk threadChunk = threadCompleteChunksQueue.Dequeue();
				int xOffset = threadChunk.ChunkX * threadChunkSize;
				int yOffset = threadChunk.ChunkY * threadChunkSize;
				for (int y = 0; y < threadChunkSize; y++)
				{
					for (int x = 0; x < threadChunkSize; x++)
					{
						if (xOffset + x < worksheet.Data.Width && yOffset + y < worksheet.Data.Height)
						{
							outputArray[xOffset + x, yOffset + y] = threadChunk.OutputArray[x, y];
						}
					}
				}
				threadChunksCounter--;
			}
			threadCompleteChunksQueueMutex.ReleaseMutex();
			threadCompleteFunctionCallback(outputArray, threadChunksCounter == 0);
			if (threadChunksCounter == 0)
			{
				for (int j = 0; j < threadsCount; j++)
				{
					threads[j]?.Abort();
					threads[j] = null;
				}
				outputArray = null;
				isBusy = false;
			}
		}
		else
		{
			threadCompleteChunksQueueMutex.ReleaseMutex();
		}
	}

	public void Update(Worksheet worksheet)
	{
		this.worksheet = worksheet;
		threadChunksArrayWidth = Mathf.CeilToInt((float)this.worksheet.Data.Width / (float)threadChunkSize);
		threadChunksArrayHeight = Mathf.CeilToInt((float)this.worksheet.Data.Height / (float)threadChunkSize);
		threadChunksCount = Mathf.Max(threadChunksArrayWidth * threadChunksArrayHeight, 1);
	}

	public void Start(Func<int, int, int, int, float[,]> threadFunctionCallback, Action<float[,], bool> threadCompleteFunctionCallback)
	{
		ThreadsManager.threadFunctionCallback = threadFunctionCallback;
		this.threadCompleteFunctionCallback = threadCompleteFunctionCallback;
		threadChunksCounter = threadChunksCount;
		outputArray = new float[worksheet.Data.Width, worksheet.Data.Height];
		threadChunksQueueMutex.WaitOne();
		for (int y = 0; y < threadChunksArrayHeight; y++)
		{
			for (int x = 0; x < threadChunksArrayWidth; x++)
			{
				ThreadChunk threadChunk = new ThreadChunk
				{
					ChunkX = x,
					ChunkY = y,
					PositionX = x * threadChunkSize,
					PositionY = y * threadChunkSize,
					Width = threadChunkSize,
					Height = threadChunkSize
				};
				threadChunksQueue.Enqueue(threadChunk);
			}
		}
		threadChunksQueueMutex.ReleaseMutex();
		isBusy = true;
	}

	public void Abort()
	{
		if (isBusy)
		{
			for (int i = 0; i < threadsCount; i++)
			{
				threads[i]?.Abort();
				threads[i] = null;
			}
			threadChunksQueueMutex?.WaitOne();
			threadChunksQueue?.Clear();
			threadChunksQueueMutex?.ReleaseMutex();
			threadCompleteChunksQueueMutex?.WaitOne();
			threadCompleteChunksQueue?.Clear();
			threadCompleteChunksQueueMutex?.ReleaseMutex();
			outputArray = null;
			isBusy = false;
		}
	}

	private static void Thread()
	{
		threadChunksQueueMutex.WaitOne();
		if (threadChunksQueue.Count > 0)
		{
			ThreadChunk threadChunk = threadChunksQueue.Dequeue();
			threadChunksQueueMutex.ReleaseMutex();
			threadChunk.OutputArray = threadFunctionCallback(threadChunk.PositionX, threadChunk.PositionY, threadChunk.Width, threadChunk.Height);
			threadChunk.Complete = true;
			threadCompleteChunksQueueMutex.WaitOne();
			threadCompleteChunksQueue.Enqueue(threadChunk);
			threadCompleteChunksQueueMutex.ReleaseMutex();
		}
		else
		{
			threadChunksQueueMutex.ReleaseMutex();
		}
	}
}
