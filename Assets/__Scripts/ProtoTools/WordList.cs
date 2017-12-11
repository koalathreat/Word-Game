using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordList : MonoBehaviour {

    public static WordList s;
    public TextAsset wordListText;
    public int numToParseBeforeYield = 10000;
    public int wordLengthMin = 3;
    public int wordLengthMax = 7;
    public bool __________;
    public int currLine = 0;
    public int totalLines;
    public int longWordCount;
    public int wordCount;

    private string[] lines;
    private List<string> longWords;
    private List<string> words;

    void Awake()
    {
        s = this;    
    }

    void Start()
    {
        lines = wordListText.text.Split('\n');
        totalLines = lines.Length;
        StartCoroutine(ParseLines());
    }

    public IEnumerator ParseLines()
    {
        string word;
        longWords = new List<string>();
        words = new List<string>();

        for(currLine = 0; currLine < totalLines; currLine++)
        {
            word = lines[currLine];
            if(word.Length == wordLengthMax)
            {
                longWords.Add(word);
            }
            if(word.Length >=wordLengthMin && word.Length <= wordLengthMax)
            {
                words.Add(word);
            }
            if(currLine % numToParseBeforeYield == 0)
            {
                longWordCount = longWords.Count;
                wordCount = words.Count;
                yield return null;
            }
        }
    }

    public List<string> GetWords()
    {
        return (words);
    }

    public string GetWord(int ndx)
    {
        return (words[ndx]);
    }

    public List<string> GetLongWords()
    {
        return (longWords);
    }

    public string GetLongWord(int ndx)
    {
        return (longWords[ndx]);
    }
}
