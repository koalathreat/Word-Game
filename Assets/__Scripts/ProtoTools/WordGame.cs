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

    [Header("Set in Inspector")]
    public GameObject prefabLetter;
    public Rect wordArea = new Rect(-24, 19, 48, 28);
    public float letterSize = 1.5f;
    public bool showAllWyrds = true;
    public float bigLetterSize = 4f;
    public Color bigColorDim = new Color(0.8f, 0.8f, 0.8f);
    public Color bigColorSelected = new Color(1f, 0.9f, 0.7f);
    public Vector3 bigLetterCenter = new Vector3(0, -16, 0);
    public Color[] wyrdPalette;
    [Header("Set Dynamically")]
    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;
    public List<Wyrd> wyrds;
    public List<Letter> bigLetters;
    public List<Letter> bigLettersActive;
    public string testWord;
    private string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private Transform letterAnchor, bigLetterAnchor;

    void Awake()
    {
        S = this;
        letterAnchor = new GameObject("LetterAnchor").transform;
        bigLetterAnchor = new GameObject("BigLetterAnchor").transform;
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
        Layout();
    }

    void Layout()
    {
        wyrds = new List<Wyrd>();
        GameObject go;
        Letter lett;
        string word;
        Vector3 pos;
        float left = 0;
        float columnWidth = 3;
        char c;
        Color col;
        Wyrd wyrd;

        int numRows = Mathf.RoundToInt(wordArea.height / letterSize);
        for(int i=0;i<currLevel.subWords.Count; i++)
        {
            wyrd = new Wyrd();
            word = currLevel.subWords[i];
            columnWidth = Mathf.Max(columnWidth, word.Length);
            for(int j=0; j<word.Length; j++)
            {
                c = word[j];
                go = Instantiate(prefabLetter) as GameObject;
                go.transform.SetParent(letterAnchor);
                lett = go.GetComponent<Letter>();
                lett.c = c;
                pos = new Vector3(wordArea.x + left + j * letterSize, wordArea.y);
                pos.y -= (i % numRows) * letterSize;
                lett.posImmediate = pos + Vector3.up * (20 + i % numRows);
                lett.pos = pos;
                lett.timeStart = Time.time + i * 0.05f;
                go.transform.localScale = Vector3.one * letterSize;
                wyrd.Add(lett);
            }
            if (showAllWyrds) wyrd.visible = true;
            wyrd.color = wyrdPalette[word.Length - WordList.WORD_LENGTH_MIN];
            wyrds.Add(wyrd);
            if(i%numRows == numRows-1)
            {
                left += (columnWidth + 0.5f) * letterSize;
            }
        }

        bigLetters = new List<Letter>();
        bigLettersActive = new List<Letter>();

        for (int i = 0; i < currLevel.word.Length; i++)
        {
            // This is similar to the process for a normal Letter
            c = currLevel.word[i];
            go = Instantiate(prefabLetter) as GameObject;
            go.transform.SetParent(bigLetterAnchor);
            lett = go.GetComponent<Letter>();
            lett.c = c;
            go.transform.localScale = Vector3.one * bigLetterSize;

            // Set the initial position of the big Letters below screen
            pos = new Vector3(0, -100, 0);
            lett.posImmediate = pos;
            lett.pos = pos;    // You'll add more code around this line later
            lett.timeStart = Time.time + currLevel.subWords.Count * 0.05f;
            lett.easingCuve = Easing.Sin + "-0.18";
            col = bigColorDim;
            lett.color = col;
            lett.visible = true; // This is always true for big letters
            lett.big = true;
            bigLetters.Add(lett);
        }
        // Shuffle the big letters
        bigLetters = ShuffleLetters(bigLetters);
        // Arrange them on screen
        ArrangeBigLetters();

        // Set the mode to be in-game
        mode = GameMode.inLevel;
    }

    // This method shuffles a List<Letter> randomly and returns the result
    List<Letter> ShuffleLetters(List<Letter> letts)
    {
        List<Letter> newL = new List<Letter>();
        int ndx;
        while (letts.Count > 0)
        {
            ndx = Random.Range(0, letts.Count);
            newL.Add(letts[ndx]);
            letts.RemoveAt(ndx);
        }
        return (newL);
    }

    // This method arranges the big Letters on screen
    void ArrangeBigLetters()
    {
        // The halfWidth allows the big Letters to be centered
        float halfWidth = ((float)bigLetters.Count) / 2f - 0.5f;
        Vector3 pos;
        for (int i = 0; i < bigLetters.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            bigLetters[i].pos = pos;
        }
        // bigLettersActive
        halfWidth = ((float)bigLettersActive.Count) / 2f - 0.5f;
        for (int i = 0; i < bigLettersActive.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            pos.y += bigLetterSize * 1.25f;
            bigLettersActive[i].pos = pos;
        }
    }

    void Update()
    {
        // Declare a couple useful local variables
        Letter ltr;
        char c;

        switch (mode)
        {
            case GameMode.inLevel:
                // Iterate through each char input by the player this frame
                foreach (char cIt in Input.inputString)
                {
                    // Shift cIt to UPPERCASE
                    c = System.Char.ToUpperInvariant(cIt);

                    // Check to see if it's an uppercase letter
                    if (upperCase.Contains(c))
                    { // Any uppercase letter
                      // Find an available Letter in bigLetters with this
                      //  char
                        ltr = FindNextLetterByChar(c);
                        // If a Letter was returned
                        if (ltr != null)
                        {
                            // ... then add this char to the testWord and
                            //  move the returned big Letter to
                            //  bigLettersActive
                            testWord += c.ToString();
                            // Move it from the inactive to the active List<>
                            bigLettersActive.Add(ltr);
                            bigLetters.Remove(ltr);
                            ltr.color = bigColorSelected; // Make it look
                                                          // active
                            ArrangeBigLetters();     // Rearrange the big
                                                     // Letters
                        }
                    }

                    if (c == '\b')
                    { // Backspace
                        // Remove the last Letter in bigLettersActive
                        if (bigLettersActive.Count == 0) return;
                        if (testWord.Length > 1)
                        {
                            // Clear the last char of testWord
                            testWord = testWord.Substring(0, testWord.Length - 1);
                        }
                        else
                        {
                            testWord = "";
                        }

                        ltr = bigLettersActive[bigLettersActive.Count - 1];
                        // Move it from the active to the inactive List<>
                        bigLettersActive.Remove(ltr);
                        bigLetters.Add(ltr);
                        ltr.color = bigColorDim;    // Make it the inactive
                                                    // color
                        ArrangeBigLetters();        // Rearrange the big
                                                    //  Letters
                    }

                    if (c == '\n')
                    { // Return/Enter
                        // Test the testWord against the words in WordLevel
                        CheckWord();
                    }

                    if (c == ' ')
                    { // Space
                        // Shuffle the bigLetters
                        bigLetters = ShuffleLetters(bigLetters);
                        ArrangeBigLetters();
                    }
                }
                break;
        }
    }

    // This finds an available Letter with the char c in bigLetters.
    // If there isn't one available, it returns null.
    Letter FindNextLetterByChar(char c)
    {
        // Search through each Letter in bigLetters
        foreach (Letter ltr in bigLetters)
        {
            // If one has the same char as c
            if (ltr.c == c)
            {
                // ...then return it
                return (ltr);
            }
        }
        return (null);  // Otherwise, return null
    }

    public void CheckWord()
    {
        // Test testWord against the level.subWords
        string subWord;
        bool foundTestWord = false;

        // Create a List<int> to hold the indices of other subWords that are
        //  contained within testWord
        List<int> containedWords = new List<int>();

        // Iterate through each word in currLevel.subWords
        for (int i = 0; i < currLevel.subWords.Count; i++)
        {

            // Check whether the Wyrd has already been found
            if (wyrds[i].found)
            {                                      // a
                continue;
            }

            subWord = currLevel.subWords[i];
            // Check whether this subWord is the testWord or is contained in
            //  it
            if (string.Equals(testWord, subWord))
            {                    // b
                HighlightWyrd(i);
                ScoreManager.SCORE(wyrds[i], 1);
                foundTestWord = true;
            }
            else if (testWord.Contains(subWord))
            {
                containedWords.Add(i);
            }
        }

        if (foundTestWord)
        { // If the test word was found in subWords
            // ...then highlight the other words contained in testWord
            int numContained = containedWords.Count;
            int ndx;
            // Highlight the words in reverse order
            for (int i = 0; i < containedWords.Count; i++)
            {
                ndx = numContained - i - 1;
                HighlightWyrd(containedWords[ndx]);
                ScoreManager.SCORE(wyrds[containedWords[ndx]], i + 2);
            }
        }

        // Clear the active big Letters regardless of whether testWord
        //  was valid
        ClearBigLettersActive();
    }

    // Highlight a Wyrd
    void HighlightWyrd(int ndx)
    {
        // Activate the subWord
        wyrds[ndx].found = true;   // Let it know it's been found
        // Lighten its color
        wyrds[ndx].color = (wyrds[ndx].color + Color.white) / 2f;
        wyrds[ndx].visible = true; // Make its 3D Text visible
    }

    // Remove all the Letters from bigLettersActive
    void ClearBigLettersActive()
    {
        testWord = "";             // Clear the testWord
        foreach (Letter ltr in bigLettersActive)
        {
            bigLetters.Add(ltr);     // Add each Letter to bigLetters
            ltr.color = bigColorDim; // Set it to the inactive color
        }
        bigLettersActive.Clear();  // Clear the List<>
        ArrangeBigLetters();       // Rearrange the Letters on screen
    }
}
