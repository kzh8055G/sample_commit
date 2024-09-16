using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Device;
// using LibGit2Sharp;
// using LibGit2Sharp;
// using LibGit2


// 이거 왜이래
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
		// Repository
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
		if (GUILayout.Button("커밋"))
		{
			// ReleaseData\1.2.3.4
			CommitAsync(@"ReleaseData/1.2.3.4").Forget();
		}
	}


	//private string MyPathCombine(string basename, string filename)
	//{
	//	int idx = basename.Length;
	//	if (idx == 0) return filename;
	//	if (basename[idx - 1] == '/') --idx;
	//	return filename;
	//}
	//// General technique:


	//IEnumerable<string> GetFilesSlash(string dirname) =>
	//	Directory.GetFiles(
	//		dirname.Replace('/', Path.DirectorySeparatorChar)).Select((p) =>
	//		{
	//		MyPathCombine(dirname, Path.GetFileName(p)))
	
	private async UniTask<List<(string path, string content)>> GetCommitTargetInfo(string targetPath)
	{

		var result = new List<(string path, string base64Content)>();

		var filePaths = Directory.GetFiles(targetPath, "*", SearchOption.AllDirectories);
			

		foreach(var path in filePaths)
		{

			string consistentPath = 
				path.Replace('/', Path.AltDirectorySeparatorChar).Replace('\\', Path.AltDirectorySeparatorChar);


			var bytes = await File.ReadAllTextAsync(path);
			var content = Convert.ToBase64String(Encoding.UTF8.GetBytes(bytes));

			result.Add((consistentPath, content));
		}
		return result;
	}


	private async UniTask CommitAsync(string targetPath)
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

		// 저장소의 기본 브랜치 정보를 가져오기 (브랜치의 SHA 값 필요)
		var repoInfo = await client.Repository.Get(owner, repo);
		var reference = await client.Git.Reference.Get(owner, repo, $"heads/{branch}");
		var lastestCommitSha = reference.Object.Sha;

		// 해당 커밋에서 트리 정보 가져오기
		var lastestCommit = await client.Git.Commit.Get(owner, repo, lastestCommitSha);
		var baseTree = lastestCommit.Tree.Sha;

		var newTree = new NewTree { BaseTree = baseTree };


		// string targetPath = "";
		var fileInfos = await GetCommitTargetInfo(targetPath);

		foreach(var info in fileInfos)
		{

			var blob = new NewBlob
			{
				Encoding = EncodingType.Base64,
				Content = info.content
			};

			var blobRef = await client.Git.Blob.Create(owner, repo, blob);

			var treeItem = new NewTreeItem
			{
				Path = info.path,
				Mode = "100644",
				Type = TreeType.Blob,
				Sha = blobRef.Sha
			};

			newTree.Tree.Add(treeItem);
		}

		// 새로운 트리 생성
		var treeResponse = await client.Git.Tree.Create(owner, repo, newTree);
		// 새 커밋 생성
		var newCommit = new NewCommit("Test", treeResponse.Sha, lastestCommitSha);
		var commit = await client.Git.Commit.Create(owner, repo, newCommit);

		await client.Git.Reference.Update(owner, repo, $"heads/{branch}", new ReferenceUpdate(commit.Sha));

		//// 새로운 파일을 블롭으로 만들기
		//var newBlob = new NewBlob
		//{
		//	Encoding = EncodingType.Base64,
		//	Content = content
		//};


		// client.Repository.conten



		//var detail = 
		//	await client.Repository.Content.GetAllContentsByRef(owner, repo, "a.txt", branch);


		//await client.Repository.Content.CreateFile(owner, repo, "a.txt", new CreateFileRequest()

		//UnityEngine.Debug.Log(detail);

		// client.Repository.Content.GetAllContents()
		// var contents = await client.Repository.Content.GetAllContents(owner, repo, branch);

		//foreach (var content in contents)
		//{
		//	Debug.Log(content.Content);
		//}

		// client.Git.Commit.Get()
	}


	// private 


	//


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
