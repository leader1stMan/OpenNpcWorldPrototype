using System.IO;
using UnityEditor;

namespace PathPainter
{
	[InitializeOnLoad]
	public class CompMgr : Editor
	{
		private const string pkgName = "Path Painter";

		static CompMgr()
		{
			DirectoryInfo dirInfo = new DirectoryInfo("./Assets/" + pkgName + "/Editor/");
			FileInfo[] files = dirInfo.GetFiles(pkgName + "*.dll");
			foreach (var file in files)
			{
				MoveAsset(file.FullName, file.FullName.Replace(".dll", ".dll.pp"));
			}
#if UNITY_5
			files = dirInfo.GetFiles(pkgName + "*U5.dll.pp");
			foreach (var file in files)
			{
				MoveAsset(file.FullName, file.FullName.Replace(".dll.pp", ".dll"));
			}
#elif UNITY_2017
			files = dirInfo.GetFiles(pkgName + "*U2017.dll.pp");
			foreach (var file in files)
			{
				MoveAsset(file.FullName, file.FullName.Replace(".dll.pp", ".dll"));
			}
#elif UNITY_2018
			files = dirInfo.GetFiles(pkgName + "*U2018.dll.pp");
			foreach (var file in files)
			{
				MoveAsset(file.FullName, file.FullName.Replace(".dll.pp", ".dll"));
			}
#endif
			AssetDatabase.Refresh();
		}

		private static void MoveAsset(string oldFullName, string newFullName)
		{
			File.Move(oldFullName, newFullName);
			if (File.Exists(oldFullName + ".meta"))
			{
				File.Move(oldFullName + ".meta", newFullName + ".meta");
			}
		}
	}
}