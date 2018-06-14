using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayingCard {

	public enum cardtypes{
		nine,
		ten,
		jack,
		queen,
		king,
		ace
	}

	public enum suits{
		spade,
		club,
		heart,
		diamond
	}

	public cardtypes type;
	public suits suit;
	public bool notfake;

	public PlayingCard(cardtypes type, suits suit, bool notfake){
		this.type = type;
		this.suit = suit;
		this.notfake = notfake;
	}

	public static PlayingCard GetLeft(suits trump){
		PlayingCard left = new PlayingCard(cardtypes.jack, suits.spade, true);
		if(trump == suits.spade) left.suit = suits.club;
		if(trump == suits.club) left.suit = suits.spade;
		if(trump == suits.heart) left.suit = suits.diamond;
		if(trump == suits.diamond) left.suit = suits.heart;
		return left;
	}

	public bool IsEqual(PlayingCard card2){
		if(this.suit == card2.suit && this.type == card2.type){
			return true;
		} else {
			return false;
		}
	}
}
