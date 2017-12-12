using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum GameMode
{
    preGame,
    loading,
    makeLevel,
    levelPrep,
    inLevel
}

public class WordGame : MonoBehaviour {
    static public WordGame S;
    [Header("Set Dynamically")]
    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;

    void Awake()
    {
        S = this;    
    }

    void Start () {
        mode = GameMode.loading;
        WordList.INIT();
	}

    public void WordListParseComplete()
    {
        mode = GameMode.makeLevel;
        currLevel = MakeWordLevel();
    }
	
    public WordLevel MakeWordLevel(int levelNum = -1)
    {
        WordLevel level = new WordLevel();
        if(levelNum == -1)
        {
            level.longWordIndex = Random.Range(0, WordList.LONG_WORD_COUNT);
        }
        else
        {

        }
        level.levelNum = levelNum;
        level.word = WordList.GET_LONG_WORD(level.longWordIndex);
        level.charDict = WordLevel.MakeCharDict(level.word);
        StartCoroutine(FindSubWordsCoroutine(level));
        return (level);
    }

    public IEnumerator FindSubWordsCoroutine(WordLevel level)
    {
        level.subWords = new List<string>();
        string str;
        List<string> words = WordList.GET_WORDS();
        for(int i =0; i<WordList.WORD_COUNT; i++)
        {
            str = words[i];
            if(WordLevel.CheckWordInLevel(str, level))
            {
                level.subWords.Add(str);
            }
            if(i%WordList.NUM_TO_PARSE_BEFORE_YIELD == 0)
            {
                yield return null;
            }
        }

        level.subWords.Sort();
        level.subWords = SortWordsByLength(level.subWords).ToList();
        SubWordSearchComplete();
    }

    public static IEnumerable<string> SortWordsByLength(IEnumerable<string> ws)
    {
        ws = ws.OrderBy(s => s.Length);
        return ws;
    }

    public void SubWordSearchComplete()
    {
        mode = GameMode.levelPrep;
    }
}
