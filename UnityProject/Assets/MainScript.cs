using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MainScript : MonoBehaviour 
{
	//***** Game Logic *****//

	private const int NUM_STICKS = 20;
	private const int NUM_SHOTS = 3;

	private int shotsLeft;	
	private int startPlayer = 1;
	private int currentPlayer = 1;	
	private int playerScore1 = 0;
	private int playerScore2 = 0;

	private GameObject[] gameSticks = new GameObject[NUM_STICKS];
	private List<int> sticksLeft = new List<int>();

	public bool useCPU = true;
	
	//***** GUI *****//

	public Image gamePanel;
	public Image gameOverPanel;
	public Image playerPanel1;
	public Image playerPanel2;

	public Text playerScoreText1;
	public Text playerScoreText2;
	public Text gameOverText;

	public Button endTurnButton;

	public GameObject stickPrefab;

	//*****//

	public void InitGame ()
	{
		// Instantiate the stick game objects
		for(int i = 0; i < NUM_STICKS; i++)
		{
			gameSticks[i] = GameObject.Instantiate(stickPrefab) as GameObject;			
			gameSticks[i].transform.parent = gamePanel.transform;
			gameSticks[i].GetComponent<Image>().rectTransform.anchoredPosition = new Vector2((i - NUM_STICKS * 0.5f + 0.5f) * 40.0f,0);

			//Fill sticksLeft array
			sticksLeft.Add(i);
		}

		// Init game logic
		currentPlayer = startPlayer;
		SetCurrentPlayer(currentPlayer);

		// Init GUI
		HideGameOverGUI();
		HighlightPlayer(currentPlayer);
	}	

	// Restart function which is called at the end of a game
	public void ResartGame (bool reset = false)
	{
		foreach(var g in gameSticks)
		{
			Object.DestroyImmediate(g);
		}

		sticksLeft.Clear();

		if(reset) startPlayer = 1;
		else startPlayer = (startPlayer == 1) ? 2 : 1;

		InitGame();
	}

	// Reset scores and restart game
	public void ResetGame ()
	{
		playerScore1 = playerScore2 = 0;
		UpdatePlayerScoreGUI(); 
		ResartGame(true);
	}
	
	// This function set player to be the CPU, a reset operation is perform each time this function is called
	public void ToggleUseCPU ()
	{
		useCPU = !useCPU;
		ResetGame();
	}

	public void SwitchCurrentPlayer()
	{
		SetCurrentPlayer((currentPlayer == 1) ? 2 : 1);
	}

	public void SetCurrentPlayer(int player)
	{
		currentPlayer = player; 
		shotsLeft = NUM_SHOTS;

		HighlightPlayer(currentPlayer);
		endTurnButton.gameObject.SetActive(false);

		// This bit makes sure that game sticks are not clickable when the CPU is playing
		foreach(var i in sticksLeft)
		{
			var stickId = i;
			gameSticks[i].GetComponent<Button>().onClick.RemoveAllListeners();

			if(currentPlayer == 1 || !useCPU)
			{
				gameSticks[i].GetComponent<Button>().onClick.AddListener(() => OnClick(stickId));
			}
		}

		// Trigger AI routine if CPU is enabled
		if(useCPU && currentPlayer == 2)
		{
			StartCoroutine(DoCPU());
		}
	}

	// AI logic, cf. report for more details
	IEnumerator DoCPU()
	{
		// find the number of stick CPU has to remove in order to get into a win-configuration
		int n = (sticksLeft.Count - 1) % (NUM_SHOTS + 1);

		// If opponent already in a win-configuration select a random number of sticks
		if(n == 0) n = UnityEngine.Random.Range(1,NUM_SHOTS);

		// Force the system to wait one second for smoother visuals
		yield return new WaitForSeconds(1);

		for(int i = 0; i < n; i++)
		{			
			// Choose the first available stick from the array
			OnClick(sticksLeft.First());
			
			if(sticksLeft.Count == 1)
			{
				break;
			}
			
			// Force the system to wait one second for smoother visuals
			yield return new WaitForSeconds(1);
		}

		if(sticksLeft.Count != 1 && n != NUM_SHOTS) SetCurrentPlayer(1);
	}

	// Click callback function, disable stick button state and switch current player is shotsLeft == 0
	void OnClick(int value)
	{
		if(useCPU && currentPlayer == 2) endTurnButton.gameObject.SetActive(false);
		else endTurnButton.gameObject.SetActive(true);

		gameSticks[value].GetComponent<Button>().onClick.RemoveAllListeners();
		gameSticks[value].GetComponent<Button>().interactable = false;

		sticksLeft.Remove(value);
		shotsLeft --;

		if(sticksLeft.Count == 1) 
			GameOver();
		else if(shotsLeft == 0) 
			SwitchCurrentPlayer();
	}
	
	// End of game, shows game over GUI and increment the winner's score
	void GameOver()
	{
		ShowGameOverGUI();
	
		if(currentPlayer == 1) playerScore1 ++;
		else playerScore2 ++;
		
		UpdatePlayerScoreGUI();
	}

	// Use this for initialization
	void Start () 
	{
		InitGame();
	}
	
	//***** GUI functions//	
	
	void ShowGameOverGUI()
	{		
		gameOverText.text = "Player " + currentPlayer + " wins";

		gameOverPanel.gameObject.SetActive(true);
		gamePanel.gameObject.SetActive(false);		
	}
	
	void HideGameOverGUI()
	{		
		gamePanel.gameObject.SetActive(true);
		gameOverPanel.gameObject.SetActive(false);
		endTurnButton.gameObject.SetActive(false);	
	}
	
	public void UpdatePlayerScoreGUI ()
	{
		playerScoreText1.text = "" + playerScore1;
		playerScoreText2.text = "" + playerScore2; 
	}

	public void HighlightPlayerGUI(int player)
	{
		if(player == 1)
		{
			playerPanel1.color = new Color(1,1,1,1);
			playerPanel2.color = new Color(1,1,1,0);
		}
		else
		{
			playerPanel1.color = new Color(1,1,1,0);
			playerPanel2.color = new Color(1,1,1,1);
		}
	}
}
