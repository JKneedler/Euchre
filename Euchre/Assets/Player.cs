using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player {

	public List<PlayingCard> hand;
	public float smartness;
	public GameManager game;
	public int[] team;

	public Player(GameManager game, int[] team){
		hand = new List<PlayingCard>();
		this.team = team;
		this.game = game;
	}

	public void SwitchCard(PlayingCard newCard){
		PlayingCard lowestOff = hand[0];
		for(int i = 0; i < 5; i++){
			if(hand[i].suit != game.trump && !hand[i].IsEqual(PlayingCard.GetLeft(game.trump))){
				if((int)hand[i].type < (int)lowestOff.type){
					lowestOff = hand[i];
				}
			}
		}
		hand.Remove(lowestOff);
		hand.Add(newCard);
	}


	public bool ChooseToPickUp(){
		bool pickUp;
		int handValue = 0;
		foreach(PlayingCard card in hand) {
			int cardValue = (int)card.type;
			if(card.suit == game.kittyCard.suit){
				cardValue *= 2;
			}
			if(card.type == PlayingCard.cardtypes.jack && card.suit == game.kittyCard.suit){
				cardValue = 20;
			}
			if(card.IsEqual(PlayingCard.GetLeft(game.kittyCard.suit))){
				cardValue = 15;
			}
			handValue += cardValue;
		}
		int kittyCardValue = 0;
		kittyCardValue = (int)game.kittyCard.type;
		if(game.dealer == team[0] || game.dealer == team[1]){
			kittyCardValue *= 2;
			handValue += kittyCardValue;
			pickUp = MinMaxMiddle(45, 55, handValue);
		} else {
			handValue += kittyCardValue;
			pickUp = MinMaxMiddle(35, 45, handValue);
		}
		return pickUp;
	}

	public bool MinMaxMiddle(int min, int max, int num){
		bool ret = false;
		if(num <= min){
			ret = false;
			Debug.Log("Definetily no " + num);
		} else if(num >= max) {
			ret = true;
			Debug.Log("Definitely yes " + num);
		} else {
			float randNum = Random.Range(0f, 1f);
			if((float)num > (randNum * (max-min)) + min){
				ret = true;
			} else {
				ret = false;
			}
			Debug.Log("Maybe " + num);
		}
		return ret;
	}

	public int[] ChooseSuit(){
		int[] ret = {0, 0};
		PlayingCard.suits bestSuit = PlayingCard.suits.spade;
		int bestSuitValue = 0;
		for(int i = 0; i < 4; i++){
			int suitValue = 0;
			PlayingCard.suits curSuit = (PlayingCard.suits)i;
			foreach(PlayingCard card in hand) {
				int cardValue = (int)card.type;
				if(card.suit == curSuit){
					cardValue *= 2;
				}
				if(card.type == PlayingCard.cardtypes.jack && card.suit == curSuit){
					cardValue = 20;
				}
				if(card.IsEqual(PlayingCard.GetLeft(curSuit))){
					cardValue = 15;
				}
				suitValue += cardValue;
			}
			if(suitValue > bestSuitValue){
				bestSuitValue = suitValue;
				bestSuit = curSuit;
			}
		}
		if(MinMaxMiddle(40, 50, bestSuitValue)) {
			ret[0] = 1;
			ret[1] = (int)bestSuit;
		}
		return ret;
	}

	public PlayingCard ChooseCardToPlay(){
		PlayingCard cardToPlay = hand[0];
		hand.Remove(cardToPlay);
		return cardToPlay;
	}
}
