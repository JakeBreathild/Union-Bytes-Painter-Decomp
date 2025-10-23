using System;
using System.IO;
using System.IO.Compression;
using Godot;

public static class FileSystem
{
	public enum FileTypeEnum
	{
		NONE,
		PROJECT
	}

	public enum ErrorEnum
	{
		NONE,
		FILE,
		VERSION
	}

	public const int FILE_MAGICNUMBER = 28137474;

	public const int FILE_VERSION = 10;

	public static string GetPath()
	{
		string path = ((!OS.HasFeature("editor")) ? OS.GetExecutablePath().GetBaseDir() : "D:\\Tools\\Godot\\Projects\\UB Painter");
		if (!System.IO.Directory.Exists(path))
		{
			return "";
		}
		return System.IO.Path.GetFullPath(path).TrimEnd(new char[1] { '\\' });
	}

	public static string GetPath(string directory)
	{
		return GetPath() + "\\" + directory;
	}

	public static bool IsFilePathValid(string path)
	{
		if (path.Trim() == string.Empty)
		{
			return false;
		}
		string pathname;
		string filename;
		try
		{
			pathname = System.IO.Path.GetPathRoot(path);
			filename = System.IO.Path.GetFileName(path);
		}
		catch (ArgumentException)
		{
			return false;
		}
		if (filename.Trim() == string.Empty)
		{
			return false;
		}
		if (pathname.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
		{
			return false;
		}
		if (filename.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
		{
			return false;
		}
		return true;
	}

	public static ErrorEnum SaveProject(Workspace workspace, string file)
	{
		return ErrorEnum.NONE;
	}

	private static void ReadProjectFile_008(BinaryReader binaryReader, Workspace workspace)
	{
		Color color = new Color(0f, 0f, 0f);
		Channel.NormalMode = (Channel.NormalModeEnum)binaryReader.ReadInt32();
		Channel.NormalStrength = binaryReader.ReadSingle();
		workspace.DeleteWorksheets();
		workspace.AddWorksheet(Worksheet.ReadFromBinaryStream_009(binaryReader), resetWorkspace: false);
		Register.PreviewspaceMeshManager.ReadFromBinaryStream(binaryReader);
		Register.CollisionManager.Update();
		Register.LibraryManager.RequestThumbnails(workspace.Worksheet.DrawingPresetsList);
		workspace.Reset();
		workspace.UpdateShaders();
		workspace.EnvironmentManager.SetEnvironmentRotation(binaryReader.ReadSingle());
		Register.PreviewspaceMeshManager.SetMeshScale(binaryReader.ReadSingle());
		Register.PreviewspaceMeshManager.SetUvScale(binaryReader.ReadSingle());
		workspace.EnvironmentManager.EnableGlow(binaryReader.ReadBoolean());
		workspace.EnvironmentManager.SetGlowIntensity(binaryReader.ReadSingle());
		MaterialManager.SetEmissionStrength(binaryReader.ReadSingle());
		color.r = binaryReader.ReadSingle();
		color.g = binaryReader.ReadSingle();
		color.b = binaryReader.ReadSingle();
		workspace.EnvironmentManager.SetEnvironmentAmbientColor(color);
		workspace.EnvironmentManager.SetEnvironmentAmbientEnergy(binaryReader.ReadSingle());
		workspace.EnvironmentManager.SetEnvironmentSkyContribution(binaryReader.ReadSingle());
		workspace.EnvironmentManager.EnableDirectionalLight(binaryReader.ReadBoolean());
		color.r = binaryReader.ReadSingle();
		color.g = binaryReader.ReadSingle();
		color.b = binaryReader.ReadSingle();
		workspace.EnvironmentManager.SetDirectionalLightColor(color);
		workspace.EnvironmentManager.SetDirectionalLightEnergy(binaryReader.ReadSingle());
		Register.PreviewspaceViewport.ReadPointLightFromBinaryStream(binaryReader, 0);
		Register.PreviewspaceViewport.ReadPointLightFromBinaryStream(binaryReader, 1);
		Register.PreviewspaceViewport.ReadPointLightFromBinaryStream(binaryReader, 2);
	}

	private static void ReadProjectFile_009(BinaryReader binaryReader, Workspace workspace)
	{
		Color color = new Color(0f, 0f, 0f);
		int thumbnailWidth = binaryReader.ReadInt32();
		int thumbnailHeight = binaryReader.ReadInt32();
		binaryReader.ReadBytes(thumbnailWidth * thumbnailHeight * 16);
		Channel.NormalMode = (Channel.NormalModeEnum)binaryReader.ReadInt32();
		Channel.NormalStrength = binaryReader.ReadSingle();
		workspace.DeleteWorksheets();
		workspace.AddWorksheet(Worksheet.ReadFromBinaryStream_009(binaryReader), resetWorkspace: false);
		Register.PreviewspaceMeshManager.ReadFromBinaryStream(binaryReader);
		Register.CollisionManager.Update();
		Register.LibraryManager.RequestThumbnails(workspace.Worksheet.DrawingPresetsList);
		workspace.Reset();
		workspace.UpdateShaders();
		workspace.EnvironmentManager.SetEnvironmentRotation(binaryReader.ReadSingle());
		Register.PreviewspaceMeshManager.SetMeshScale(binaryReader.ReadSingle());
		Register.PreviewspaceMeshManager.SetUvScale(binaryReader.ReadSingle());
		workspace.EnvironmentManager.EnableGlow(binaryReader.ReadBoolean());
		workspace.EnvironmentManager.SetGlowIntensity(binaryReader.ReadSingle());
		MaterialManager.SetEmissionStrength(binaryReader.ReadSingle());
		color.r = binaryReader.ReadSingle();
		color.g = binaryReader.ReadSingle();
		color.b = binaryReader.ReadSingle();
		workspace.EnvironmentManager.SetEnvironmentAmbientColor(color);
		workspace.EnvironmentManager.SetEnvironmentAmbientEnergy(binaryReader.ReadSingle());
		workspace.EnvironmentManager.SetEnvironmentSkyContribution(binaryReader.ReadSingle());
		workspace.EnvironmentManager.EnableDirectionalLight(binaryReader.ReadBoolean());
		color.r = binaryReader.ReadSingle();
		color.g = binaryReader.ReadSingle();
		color.b = binaryReader.ReadSingle();
		workspace.EnvironmentManager.SetDirectionalLightColor(color);
		workspace.EnvironmentManager.SetDirectionalLightEnergy(binaryReader.ReadSingle());
		Register.PreviewspaceViewport.ReadPointLightFromBinaryStream(binaryReader, 0);
		Register.PreviewspaceViewport.ReadPointLightFromBinaryStream(binaryReader, 1);
		Register.PreviewspaceViewport.ReadPointLightFromBinaryStream(binaryReader, 2);
		Register.ColorPalette.ReadFromBinaryStream_009(binaryReader);
	}

	private static void ReadProjectFile(BinaryReader binaryReader, Workspace workspace)
	{
		Color color = new Color(0f, 0f, 0f);
		int thumbnailWidth = binaryReader.ReadInt32();
		int thumbnailHeight = binaryReader.ReadInt32();
		binaryReader.ReadBytes(thumbnailWidth * thumbnailHeight * 4);
		Channel.NormalMode = (Channel.NormalModeEnum)binaryReader.ReadInt32();
		Channel.NormalStrength = binaryReader.ReadSingle();
		binaryReader.ReadBytes(32);
		workspace.DeleteWorksheets();
		workspace.AddWorksheet(Worksheet.ReadFromBinaryStream(binaryReader), resetWorkspace: false);
		Register.PreviewspaceMeshManager.ReadFromBinaryStream(binaryReader);
		Register.CollisionManager.Update();
		Register.LibraryManager.RequestThumbnails(workspace.Worksheet.DrawingPresetsList);
		workspace.Reset();
		workspace.UpdateShaders();
		workspace.EnvironmentManager.SetEnvironmentRotation(binaryReader.ReadSingle());
		Register.PreviewspaceMeshManager.SetMeshScale(binaryReader.ReadSingle());
		Register.PreviewspaceMeshManager.SetUvScale(binaryReader.ReadSingle());
		workspace.EnvironmentManager.EnableGlow(binaryReader.ReadBoolean());
		workspace.EnvironmentManager.SetGlowIntensity(binaryReader.ReadSingle());
		MaterialManager.SetEmissionStrength(binaryReader.ReadSingle());
		color.r8 = binaryReader.ReadByte();
		color.g8 = binaryReader.ReadByte();
		color.b8 = binaryReader.ReadByte();
		workspace.EnvironmentManager.SetEnvironmentAmbientColor(color);
		workspace.EnvironmentManager.SetEnvironmentAmbientEnergy(binaryReader.ReadSingle());
		workspace.EnvironmentManager.SetEnvironmentSkyContribution(binaryReader.ReadSingle());
		workspace.EnvironmentManager.EnableDirectionalLight(binaryReader.ReadBoolean());
		color.r8 = binaryReader.ReadByte();
		color.g8 = binaryReader.ReadByte();
		color.b8 = binaryReader.ReadByte();
		workspace.EnvironmentManager.SetDirectionalLightColor(color);
		workspace.EnvironmentManager.SetDirectionalLightEnergy(binaryReader.ReadSingle());
		Register.PreviewspaceViewport.ReadPointLightFromBinaryStream(binaryReader, 0);
		Register.PreviewspaceViewport.ReadPointLightFromBinaryStream(binaryReader, 1);
		Register.PreviewspaceViewport.ReadPointLightFromBinaryStream(binaryReader, 2);
		Register.ColorPalette.ReadFromBinaryStream(binaryReader);
		binaryReader.ReadBytes(127);
		if (binaryReader.ReadBoolean())
		{
			Data.ReadSelectionsFromBinaryStream(binaryReader, workspace.Worksheet.Data);
		}
	}

	public static ErrorEnum OpenProject(Workspace workspace, string file)
	{
		file = System.IO.Path.GetFullPath(file);
		if (System.IO.File.Exists(file))
		{
			FileStream fileStream = System.IO.File.OpenRead(file);
			if (System.IO.Path.GetExtension(file) == ".dat")
			{
				BinaryReader binaryReader = new BinaryReader(fileStream);
				switch (binaryReader.ReadInt32())
				{
				case 8:
					ReadProjectFile_008(binaryReader, workspace);
					break;
				case 9:
					ReadProjectFile_009(binaryReader, workspace);
					break;
				default:
					binaryReader.Dispose();
					fileStream.Dispose();
					return ErrorEnum.VERSION;
				}
				binaryReader.Dispose();
			}
			else
			{
				GZipStream compressor = new GZipStream(fileStream, CompressionMode.Decompress);
				BinaryReader binaryReader2 = new BinaryReader(compressor);
				if (binaryReader2.ReadInt32() != 28137474)
				{
					binaryReader2.Dispose();
					compressor.Dispose();
					fileStream.Dispose();
					return ErrorEnum.FILE;
				}
				binaryReader2.ReadInt32();
				if (binaryReader2.ReadInt32() != 10)
				{
					binaryReader2.Dispose();
					compressor.Dispose();
					fileStream.Dispose();
					return ErrorEnum.VERSION;
				}
				ReadProjectFile(binaryReader2, workspace);
				binaryReader2.Dispose();
				compressor.Dispose();
			}
			fileStream.Dispose();
			Register.Gui.ResetSettingsContainer();
			workspace.Worksheet.FileName = System.IO.Path.GetFullPath(file);
			OS.SetWindowTitle(Main.WindowTitle + " - " + workspace.Worksheet.FileName);
			GD.Print("Load File: " + workspace.Worksheet.FileName);
			Register.CameraManager.ResetPreviewspaceCamera();
			return ErrorEnum.NONE;
		}
		return ErrorEnum.FILE;
	}

	public static Image LoadProjectThumbnail(string file)
	{
		Image image = null;
		file = System.IO.Path.GetFullPath(file);
		if (System.IO.File.Exists(file))
		{
			FileStream fileStream = System.IO.File.OpenRead(file);
			if (System.IO.Path.GetExtension(file) == ".dat")
			{
				BinaryReader binaryReader = new BinaryReader(fileStream);
				if (binaryReader.ReadInt32() == 9)
				{
					int width = binaryReader.ReadInt32();
					int height = binaryReader.ReadInt32();
					image = new Image();
					image.Create(width, height, useMipmaps: false, Image.Format.Rgba8);
					image.Lock();
					Color color = default(Color);
					for (int y = 0; y < height; y++)
					{
						for (int x = 0; x < width; x++)
						{
							color.r = binaryReader.ReadSingle();
							color.g = binaryReader.ReadSingle();
							color.b = binaryReader.ReadSingle();
							color.a = binaryReader.ReadSingle();
							image.SetPixel(x, y, color);
						}
					}
					image.Unlock();
				}
				binaryReader.Dispose();
			}
			else
			{
				GZipStream compressor = new GZipStream(fileStream, CompressionMode.Decompress);
				BinaryReader binaryReader2 = new BinaryReader(compressor);
				if (binaryReader2.ReadInt32() == 28137474)
				{
					binaryReader2.ReadInt32();
					if (binaryReader2.ReadInt32() == 10)
					{
						int width2 = binaryReader2.ReadInt32();
						int height2 = binaryReader2.ReadInt32();
						image = new Image();
						image.Create(width2, height2, useMipmaps: false, Image.Format.Rgba8);
						image.Lock();
						Color color2 = default(Color);
						for (int i = 0; i < height2; i++)
						{
							for (int j = 0; j < width2; j++)
							{
								color2.r8 = binaryReader2.ReadByte();
								color2.g8 = binaryReader2.ReadByte();
								color2.b8 = binaryReader2.ReadByte();
								color2.a8 = binaryReader2.ReadByte();
								image.SetPixel(j, i, color2);
							}
						}
						image.Unlock();
					}
					binaryReader2.Dispose();
					compressor.Dispose();
				}
			}
			fileStream.Dispose();
		}
		return image;
	}
}
