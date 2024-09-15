using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Device;


// 目乖 规过?
// https://laedit.net/2016/11/12/GitHub-commit-with-Octokit-net.html


public class TestCommit : EditorWindow
{

	private class GithubTokenInfo
	{
		public string token;
	}

	[MenuItem("CustomTool/Commit")]
	private static void Do()
	{
		var window = GetWindow<TestCommit>();

		window.Show();

		// Commit();
	}


	private void OnEnable()
	{
		_Initilaize();
	}

	private void _Initilaize()
	{

	}

	private void OnGUI()
	{
		if (GUILayout.Button("目乖"))
		{
			CommitAsync();
		}
	}


	private async UniTask CommitAsync()
	{
		var filePath = $"{GetCachePath("Private")}githubToken.json";

		var tokenInfo = LoadJsonFromFile<GithubTokenInfo>(filePath);

		var client = new GitHubClient(new ProductHeaderValue("my-cool-app"));
		var tokenAuth = new Credentials(tokenInfo.token);
		client.Credentials = tokenAuth;

		var userInfo = await client.User.Get("kzh8055G");



		string owner = "kzh8055G";
		string repo = "sample_commit";
		string branch = "main";


		// client.Repository.conten


		var contents = await client.Repository.Content.GetAllContents(owner, repo, branch);

		//foreach(var content in contents)
		//{
		//	Debug.Log(content.Content);
		//}

		// client.Git.Commit.Get()
	}

	public static void SaveJsonToFile(string filePath, object jsonObject)
	{
		var serialized = JsonConvert.SerializeObject(jsonObject);
		File.WriteAllText(filePath, serialized);
	}

	public static T LoadJsonFromFile<T>(string filePath)
	{
		var text = File.ReadAllText(filePath);
		return JsonConvert.DeserializeObject<T>(text);
	}

	public static string GetProjectRootPath()
	{
		var rootPath = UnityEngine.Device.Application.dataPath;
		return rootPath.Substring(0, rootPath.LastIndexOf(Path.AltDirectorySeparatorChar));
	}

	public static string GetCachePath(string dirName)
	{
		var sep = Path.AltDirectorySeparatorChar;
		var path = $"{GetProjectRootPath()}{sep}{dirName}";
		if(!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		return $"{path}{sep}";
	}
}
