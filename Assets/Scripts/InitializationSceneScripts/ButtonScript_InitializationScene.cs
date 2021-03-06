﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonScript_InitializationScene : MonoBehaviour {

	InizializationManager InitM;

	void Awake()
	{
		InitM = GameObject.Find ("InitializationManager").GetComponent<InizializationManager> ();
	}

	public void OnSet_Button () 
	{
		InitM.WriteToPlayerStats ();
		Application.LoadLevel("TitleScreen");
	}

	public void OnCharacter01_Button()
	{
		InitM.SetSelected ("character_01");
	}
	public void OnCharacter02_Button()
	{
		InitM.SetSelected ("character_02");
	}
	public void OnCharacter03_Button()
	{
		InitM.SetSelected ("character_03");
	}
	public void OnCharacter04_Button()
	{
		InitM.SetSelected ("character_04");
	}}
