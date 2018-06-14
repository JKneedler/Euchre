using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class GameManager : SerializedMonoBehaviour {

	public enum gameStages {
		kittyRound,
		pickRound,
		Round1,
		Round2,
		Round3,
		Round4,
		Round5
	}

	public enum screenLayouts {
		kittyYourChoice,
		chooseDiscard,
		kittyOthersChoice,
		chooseSuit,
		choosePlayCard,
		othersChooseCard
	}

	public PlayingCard[] cards;
	public Queue<PlayingCard> cardQ;
	public PlayingCard[] trickCards;
	public int playerScore;
	public int opposingScore;
	public int playerTricksScore;
	public int opposingTricksScore;
	private int[] dealOrder = {3,2,3,2,2,3,2,3};
	public Player[] players;
	public PlayingCard kittyCard;
	public gameStages curStage;
	public int turn;
	public int dealer;
	public PlayingCard.suits trump;
	public screenLayouts layout;
	public bool stickTheDealer;
	public bool goingAlone;
	public int calledIt;
	public int leader;


	[FoldoutGroup("UI")]
	public Sprite[][] cardSprites;
	[FoldoutGroup("UI")]
	public GameObject[] optionButtons;
	[FoldoutGroup("UI")]
	public GameObject[] suitButtons;
	[FoldoutGroup("UI")]
	public Image kittyImage;
	[FoldoutGroup("UI")]
	public Image[] trickCardImages;
	[FoldoutGroup("UI")]
	public GameObject playerHand;
	[FoldoutGroup("UI")]
	public Object cardObject;
	[FoldoutGroup("UI")]
	public GameObject[] aiChoices;
	[FoldoutGroup("UI")]
	public Animator trickAnim;
	[FoldoutGroup("UI")]
	public Image trumpImage;
	[FoldoutGroup("UI")]
	public Sprite[] trumpSprites;
	[FoldoutGroup("UI")]
	public Animator playerCardAnim;
	[FoldoutGroup("UI")]
	public Animator kittyCardAnim;

	// Use this for initialization
	void Start () {
		curStage = gameStages.kittyRound;
		players = new Player[4];
		for(int i = 0; i < 4; i++){
			int[] team = {i, i + 2};
			if(i > 1) team[1] = i - 2;
			players[i] = new Player(this, team);
		}
		cardQ = new Queue<PlayingCard>();
		cards = CreateDeck();
		StartGame();
	}

	// Update is called once per frame
	void Update () {

	}

	public PlayingCard[] CreateDeck(){
		PlayingCard[] cardArray = new PlayingCard[24];
		for(int i = 0; i < 4; i++){
			for(int j = 0; j < 6; j++){
				int index = (i * 6) + j;
				cardArray[index] = new PlayingCard((PlayingCard.cardtypes)j, (PlayingCard.suits)i, true);
			}
		}
		return cardArray;
	}

	public void StartGame(){
		dealer = Random.Range(0, 4);
		NewRound();
	}

	public void NewRound(){
		playerCardAnim.SetTrigger ("New Round");
		kittyCardAnim.SetTrigger ("BringIn");
		Color newTrumpImage = new Color (255, 255, 255, 0);
		trumpImage.color = newTrumpImage;
		playerTricksScore = 0;
		opposingTricksScore = 0;
		trickCards = new PlayingCard[4];
		if(dealer == 3){
			turn = 0;
			leader = 0;
		} else {
			turn = dealer + 1;
			leader = dealer + 1;
		}
		Debug.Log(players[0].hand.Count);
		foreach (Transform child in playerHand.transform){
			Destroy(child.gameObject);
		}
		for(int i = 0; i < 4; i++){
			players[i].hand.Clear();
			trickCards[i] = new PlayingCard((PlayingCard.cardtypes)0, (PlayingCard.suits)0, false);
		}
		for (int i = 0; i < 4; i++) {
			trickCardImages [i].enabled = false;
		}
		Shuffle();
		Deal();
		for(int i = 0; i < 5; i++){
			AddCardToPlayer(players[0].hand[i]);
		}
		playerCardAnim.SetTrigger ("RaiseHand");
		DisplayKitty(true);
		curStage = gameStages.kittyRound;
		if(turn == 0){
			layout = screenLayouts.kittyYourChoice;
		} else {
			layout = screenLayouts.kittyOthersChoice;
			StartCoroutine("AITurn");
		}
		UpdateView();
	}

	public void Shuffle(){
		List<PlayingCard> tempL = new List<PlayingCard>();
		for(int i = 0; i < 24; i++){
			tempL.Add(cards[i]);
		}
		cardQ.Clear();
		for(int i = 0; i < 24; i++){
			int num = Random.Range(0, tempL.Count);
			PlayingCard randCard = tempL[num];
			tempL.RemoveAt(num);
			cardQ.Enqueue(randCard);
		}
	}

	public void Deal(){
		int[] order = {1,2,3,4,1,2,3,4};
		for(int i = 0; i < 8; i++){
			for(int j = 0; j < dealOrder[i]; j++){
				players[order[i]-1].hand.Add(cardQ.Dequeue());
			}
		}
	}

	public void DisplayKitty(bool doDisplay){
		kittyImage.enabled = doDisplay;
		if(doDisplay){
			kittyCard = cardQ.Dequeue();
			int suitIndex = (int)kittyCard.suit;
			int typeIndex = (int)kittyCard.type;
			kittyImage.sprite = cardSprites[suitIndex][typeIndex];
		}
	}

	public void PickUp(){
		calledIt = turn;
		trump = kittyCard.suit;
		SetTrumpImage ();
		kittyCardAnim.SetInteger ("PlayerToPickUp", dealer);
		kittyCardAnim.SetTrigger ("PickUp");
		if(dealer == 0){
			layout = screenLayouts.chooseDiscard;
		} else {
			players[dealer].SwitchCard(kittyCard);
			StartCoroutine ("DiscardWait");
		}
		UpdateView();
	}

	IEnumerator DiscardWait(){
		yield return new WaitForSeconds (2);
		kittyCardAnim.SetTrigger ("Discard");
		NextStage(2);
	}

	public void SetTrumpImage(){
		Color newTrumpColor = new Color (255, 255, 255, 255);
		trumpImage.color = newTrumpColor;
		trumpImage.sprite = trumpSprites [(int)trump];
	}

	public void Pass(){
		if(turn == dealer){
			if(curStage == gameStages.kittyRound){
				NextStage(1);
			} else if(curStage == gameStages.pickRound) {
				NewRound();
			}
		} else {
			NextTurn();
		}
	}

	public void GoAlone(){

	}

	public void CalculateRoundScore(){
		if(calledIt == 0 || calledIt == 2){
			if(playerTricksScore > opposingTricksScore && playerTricksScore < 5){
				playerScore++;
			} else if(playerTricksScore == 5){
				playerScore += 2;
			} else if(playerTricksScore < opposingTricksScore){
				opposingScore += 2;
			}
		} else if(calledIt == 1 || calledIt == 3){
			if(opposingTricksScore > playerTricksScore && opposingTricksScore < 5){
				opposingScore++;
			} else if(opposingTricksScore == 5){
				opposingScore += 2;
			} else if(opposingTricksScore < playerTricksScore){
				playerScore += 2;
			}
		}
	}

	public void NextStage(int jumps){
		if ((int)curStage < (int)gameStages.Round1) {
			turn = dealer;
		}
		if(curStage == gameStages.Round5){
			if(dealer == 3){
				dealer = 0;
			} else {
				dealer++;
			}
			DetermineTrickWinner();
			CalculateRoundScore();
			NewRound();
		} else {
			if (curStage == gameStages.kittyRound && jumps == 1) {
				kittyCardAnim.SetTrigger ("BringOut");
			}
			curStage = (gameStages)((int)curStage + jumps);
			if((int)curStage > (int)gameStages.Round1){
				leader = DetermineTrickWinner();
				if(leader == 0){
					turn = 3;
				} else {
					turn = leader-1;
				}
			}
			for(int i = 0; i < 4; i++){
				trickCards[i] = new PlayingCard(PlayingCard.cardtypes.nine, PlayingCard.suits.spade, false);
				trickCardImages [i].enabled = false;
			}
			if((int)curStage >= (int)gameStages.Round1){
				DisplayKitty(false);
			}
			NextTurn();
		}
	}

	public void ChoseSuit(int suitNum){
		trump = (PlayingCard.suits)suitNum;
		SetTrumpImage ();
		NextStage(1);
	}

	public void NextTurn(){
		if(turn == 3){
			turn = 0;
		} else {
			turn++;
		}
		if(curStage == gameStages.kittyRound){
			if(turn == 0){
				layout = screenLayouts.kittyYourChoice;
			} else if(turn != 0){
				layout = screenLayouts.kittyOthersChoice;
				StartCoroutine("AITurn");
			}
		} else if(curStage == gameStages.pickRound){
			if(turn == 0){
				layout = screenLayouts.chooseSuit;
			} else {
				layout = screenLayouts.kittyOthersChoice;
				StartCoroutine("AITurn");
			}
		} else if((int)curStage >= (int)gameStages.Round1){
			int amt = 0;
			for(int i = 0; i < 4; i++){
				if(trickCards[i].notfake) amt++;
			}
			if(amt == 4){
				int winner = DetermineTrickWinner();
				if(winner == 0 || winner == 2){
					playerTricksScore++;
				} else {
					opposingTricksScore++;
				}
				switch (winner) {
				case 0:
					trickAnim.SetTrigger ("Player");
					break;
				case 1:
					trickAnim.SetTrigger ("Left");
					break;
				case 2:
					trickAnim.SetTrigger ("Partner");
					break;
				case 3:
					trickAnim.SetTrigger ("Right");
					break;
				}
				//NextStage(1);
			} else {
				if(turn == 0){
					layout = screenLayouts.choosePlayCard;
				} else {
					layout = screenLayouts.othersChooseCard;
					StartCoroutine("AITurn");
				}
			}
		}
		UpdateView();
	}

	public void PlayCard(PlayingCard cardToPlay){
		trickCards[turn] = cardToPlay;
		trickAnim.SetTrigger ("Out " + turn);
		trickCardImages[turn].enabled = true;
		int suitIndex = (int)trickCards[turn].suit;
		int typeIndex = (int)trickCards[turn].type;
		trickCardImages[turn].sprite = cardSprites[suitIndex][typeIndex];
		trickAnim.SetTrigger ("In " + turn);
		StartCoroutine("WaitAfterPlay");
	}

	public void AddCardToPlayer(PlayingCard cardToAdd){
		int suitIndex = (int)cardToAdd.suit;
		int typeIndex = (int)cardToAdd.type;
		GameObject pCard = (GameObject)Instantiate(cardObject, transform.position, Quaternion.identity);
		pCard.GetComponent<CardObject>().card = cardToAdd;
		pCard.transform.SetParent(playerHand.transform);
		pCard.GetComponent<Button>().onClick.AddListener(delegate () { this.ClickedCard(cardToAdd); });
		pCard.GetComponent<Image>().sprite = cardSprites[suitIndex][typeIndex];
	}

	public void RemoveCardFromPlayer(PlayingCard cardToRemove){
		int cardIndex = 0;
		for(int i = 0; i < playerHand.transform.childCount; i++){
			if(playerHand.transform.GetChild(i).GetComponent<CardObject>().card.IsEqual(cardToRemove)){
				cardIndex = i;
			}
		}
		Destroy(playerHand.transform.GetChild(cardIndex).gameObject);
		players[0].hand.Remove(cardToRemove);
	}

	public int DetermineTrickWinner(){
		int bestCardValue = 0;
		int bestCard = 0;
		for(int i = 0; i < 4; i++){
			int cardValue = (int)trickCards[i].type;
			if(trickCards[i].suit == trump){
				cardValue *= 2;
			}
			if(trickCards[i].type == PlayingCard.cardtypes.jack && trickCards[i].suit == trump){
				cardValue = 20;
			}
			if(trickCards[i].IsEqual(PlayingCard.GetLeft(trump))){
				cardValue = 15;
			}
			if(cardValue > bestCardValue){
				bestCard = i;
				bestCardValue = cardValue;
			}
		}
		return bestCard;
	}

	public void ClickedCard(PlayingCard cardClicked){
		if(curStage == gameStages.kittyRound){
			RemoveCardFromPlayer(cardClicked);
			AddCardToPlayer(kittyCard);
			StartCoroutine ("DiscardWait");
		} else {
			RemoveCardFromPlayer(cardClicked);
			PlayCard(cardClicked);
		}
	}

	public void UpdateView(){
		ClearView();
		switch(layout){
			case screenLayouts.kittyYourChoice:
				foreach (GameObject option in optionButtons){
					option.SetActive(true);
				}
				break;
			case screenLayouts.chooseDiscard:
				for (int i = 0; i < players[0].hand.Count; i++){
					playerHand.transform.GetChild(i).GetComponent<Button>().enabled = true;
				}
				break;
			case screenLayouts.kittyOthersChoice:
				break;
			case screenLayouts.chooseSuit:
				foreach (GameObject suitButton in suitButtons){
					suitButton.SetActive(true);
				}
				optionButtons[1].SetActive(true);
				break;
			case screenLayouts.choosePlayCard:
				for (int i = 0; i < players[0].hand.Count; i++){
					playerHand.transform.GetChild(i).GetComponent<Button>().enabled = true;
				}
				break;
			case screenLayouts.othersChooseCard:
				break;
		}
	}

	public void ClearView(){
		for (int i = 0; i < players[0].hand.Count; i++){
			playerHand.transform.GetChild(i).GetComponent<Button>().enabled = false;
		}
		foreach (GameObject option in optionButtons){
			option.SetActive(false);
		}
		foreach (GameObject suitButton in suitButtons){
			suitButton.SetActive(false);
		}
	}

	IEnumerator AITurn(){
		yield return new WaitForSeconds(2);
		if(curStage == gameStages.kittyRound){
			if(players[turn].ChooseToPickUp()){
				aiChoices [turn - 1].GetComponent<Animator> ().SetTrigger ("Take");
				PickUp();
			} else {
				aiChoices[turn-1].GetComponent<Animator>().SetTrigger("Pass");
				yield return new WaitForSeconds(2);
				Pass();
			}
		} else if(curStage == gameStages.pickRound){
			int[] suitChoice = players[turn].ChooseSuit();
			if(suitChoice[0] == 1){
				ChoseSuit(suitChoice[1]);
			} else {
				aiChoices[turn-1].GetComponent<Animator>().SetTrigger("Pass");
				yield return new WaitForSeconds(2);
				Pass();
			}
		} else if((int)curStage >= (int)gameStages.Round1){
			PlayCard(players[turn].ChooseCardToPlay());
		}
	}

	IEnumerator WaitAfterPlay(){
		yield return new WaitForSeconds(2);
		NextTurn();
	}

}
