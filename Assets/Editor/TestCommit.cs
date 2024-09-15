using Octokit;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestCommit : MonoBehaviour
{
	private readonly string token =
		"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";

	[MenuItem("CustomTool/Commit")]
	private void Do()
	{
		Commit();
	}
	private void Commit()
	{

		var client = new GitHubClient(new ProductHeaderValue("my-cool-app"));

		var tokenAuth = new Credentials(token);
		client.Credentials = tokenAuth;

		// client.Git.Commit.Get()
	}
}
