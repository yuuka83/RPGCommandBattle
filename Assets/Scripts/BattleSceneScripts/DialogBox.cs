using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogBox : MonoBehaviour
{
	[SerializeField]
	TextMeshProUGUI messageText;
	
	public void SetMessage(string message)
	{
		messageText.text = message;
	}
}
