using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Perminus.Unity
{
    public class WordPuzzleBoard : MonoBehaviour, ICrosswordBoard
    {
        [SerializeField]
        private GridLayoutGroup _gridLayout;

        [SerializeField]
        private WordPuzzlePiece _pieceTemplate;

        private Dimensions _startPos;

        WordPuzzlePiece[,] _pieces;

        private Dimensions _dimensions;

        private Coroutine _corWaitForEnd;

        private bool _hasContent;

        public Dimensions Dimensions => _dimensions;

        public event Action<bool> ContentHighlighted;

        public void Start()
        {
            Make(new(8, 8));

            HideContent("Hello");
        }

        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// Creates the board. This method instantiates the pieces and places them in the grid layout.
        /// </summary>
        public void Make(Dimensions dimensions)
        {
            _pieces = new WordPuzzlePiece[dimensions.X, dimensions.Y];
            _dimensions = dimensions;
            _gridLayout.constraintCount = dimensions.X;

            for (int y = 0; y < dimensions.Y; y++)
            {
                for (int x = 0; x < dimensions.X; x++)
                {
                    //instantiate template:
                    var piece = Instantiate(_pieceTemplate.gameObject, _gridLayout.transform).GetComponent<WordPuzzlePiece>();

                    piece.Initialize(this, new(x, y));
                    _pieces[x, y] = piece;
                }
            }
        }

        private WordPuzzlePiece _currentPiece;
        private WordPuzzlePiece _startPiece;
        private bool _isDragging;


        /// <summary>
        /// Called when the user enters a piece. This method highlights the pieces that are intersecting the line between the start piece and the current piece. 
        /// The algorithm used is Bresenham's line algorithm.
        /// </summary>
        public void OnEnteredElement(WordPuzzlePiece piece)
        {
            if (_isDragging && _currentPiece != piece && _hasContent)
            {
                _currentPiece = piece;
                //Get the items intersecting us:
                List<Dimensions> intesecting = new(BresenhamLineAlgorithm.GetPointsOnLine(_startPiece.Coordinates, _currentPiece.Coordinates));

                foreach (var item in _pieces)
                {
                    item.VisuaState = intesecting.Contains(item.Coordinates)
                        ? VisualState.Highlighted
                        : VisualState.None;
                }
                AnimateUtility.Animate(piece.gameObject, LeanTweenType.easeOutQuint, 0.25f);
            }
        }

        /// <summary>
        /// Called when the user starts dragging a piece. This method simply sets the current piece and the start piece, and starts the drag state
        /// </summary>
        public void OnStartedDrag(WordPuzzlePiece piece)
        {
            if (!_hasContent)
            {
                return;
            }

            if (_corWaitForEnd != null)
            {
                StopCoroutine(_corWaitForEnd);
            }

            //Debug.Log("Started drag");
            _isDragging = true;
            _currentPiece = piece;
            _startPiece = piece;
            OnEnteredElement(_startPiece);
        }

        public void OnEndDrag(WordPuzzlePiece piece)
        {
            _currentPiece = null;
            _isDragging = false;

            bool isAnswer = _pieces
                .ToEnumerable()
                .Where(x => x.VisuaState == VisualState.Highlighted)
                .All(x => x.IsAnswer)
                &&
                _pieces.ToEnumerable()
                .Where(x => x.IsAnswer)
                .All(x => x.VisuaState == VisualState.Highlighted);


            _corWaitForEnd = StartCoroutine(CorWaitForEnd(isAnswer));

        }

        /// <summary>
        /// Waits for a second, then highlights the correct answers and animates them.
        /// </summary>
        private IEnumerator CorWaitForEnd(bool isAnswer)
        {
            yield return new WaitForSeconds(1);

            foreach (var item in _pieces)
            {
                if (item.IsAnswer)
                {
                    item.VisuaState = VisualState.Correct;
                    AnimateUtility.Animate(item.gameObject, LeanTweenType.easeOutQuint, 0.25f);
                }
                else if (item.VisuaState == VisualState.Highlighted)
                {
                    item.VisuaState = VisualState.Wrong;
                    AnimateUtility.Animate(item.gameObject, LeanTweenType.easeOutBounce, 0.25f);
                }
            }

            _hasContent = false;

            ContentHighlighted?.Invoke(isAnswer);
        }

        /// <summary>
        /// Hides the content in the board. The content must be within the dimensions of the board. The content will be hidden in a random direction,
        /// and the rest of the board will be filled with random characters.
        /// </summary>
        public void HideContent(string content)
        {

            //ensure content is within range:
            content = Regex.Replace(content, @"\s+", "");
            if (content.Length > _dimensions.Fittable)
            {
                throw new ArgumentOutOfRangeException(nameof(content), "Content length must not be bigger than can fit the dimensions");
            }

            HashSet<char> characterSetHash = new();

            content = content.ToUpperInvariant();

            //Construct the character set:
            foreach (var c in content)
            {
                characterSetHash.Add(c);
            }

            foreach (var c in ALPHABET)
            {
                characterSetHash.Add(c);
            }

            //clear all kwandanda:
            foreach (var item in _pieces)
            {
                item.Clear();
            }

            int start_x;
            int start_y;
            int iterations = 0;
            //fit in the content
            while (true)
            {
                int dir_x, dir_y;

                do
                {
                    dir_x = Random.Range(-1, 2);
                    dir_y = Random.Range(-1, 2);
                } while (dir_x == 0 && dir_y == 0);

                //pick a random starting point:
                start_x = Random.Range(0, _dimensions.X);
                start_y = Random.Range(0, _dimensions.Y);

                //Foresee into the future:
                int max_x = start_x + (dir_x * content.Length);
                int max_y = start_y + (dir_y * content.Length);

                //If the future is untenable, restart:
                if (max_x < 0 || max_x >= _dimensions.X || max_y < 0 || max_y >= _dimensions.Y)
                {
                    iterations++;

                    if (iterations > 1000)
                    {
                        Debug.LogWarning("Cannot process: " + content);
                        throw new ArgumentOutOfRangeException(nameof(content), "could not process");
                    }
                    continue;
                }

                //Set the start posi
                _startPos = new(start_x, start_y);

                //insert the content first:
                for (int i = 0; i < content.Length; i++)
                {
                    char c = content[i];

                    int x = start_x + (dir_x * i);
                    int y = start_y + (dir_y * i);

                    WordPuzzlePiece piece = _pieces[x, y];

                    piece.DisplayValue = c.ToString();
                    piece.Occupied = true;
                    piece.IsAnswer = true;

                }

                List<char> characterSet = new(characterSetHash);

                //fill everything else:
                foreach (var item in _pieces)
                {
                    if (item.Occupied)
                    {
                        continue;
                    }

                    int rnd = Random.Range(0, characterSet.Count);


                    item.DisplayValue = characterSet[rnd].ToString();
                    item.Occupied = true;
                    item.IsAnswer = false;
                }

                _hasContent = true;

                break;
            }
        }

        /// <summary>
        /// Shows a hint. This method highlights the start piece.
        /// </summary>
        public void ShowHint()
        {
            if (_hasContent)
            {
                _pieces[_startPos.X, _startPos.Y].VisuaState = VisualState.Highlighted;
            }
        }
    }
}