public class WordPuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
{
    [SerializeField]
    private TMP_Text _text;

    [SerializeField]
    private Image _image;
    public Dimensions Coordinates { get; private set; }

    public string DisplayValue
    {
        get => _text.text;
        set => _text.text = value;
    }

    public bool Occupied { get; set; }

    public bool IsAnswer { get; set; }

    private WordPuzzleBoard _parentBoard;

    private VisualState _visualState;

    public VisualState VisuaState
    {
        get => _visualState;
        set
        {
            _image.sprite = value switch
            {
                VisualState.None => Game.Resources.Block,
                VisualState.Highlighted => Game.Resources.BlockTurquoise,
                VisualState.Correct => Game.Resources.BlockGreen,
                VisualState.Wrong => Game.Resources.BlockRed,
                _ => Game.Resources.Block
            };
            _visualState = value;
        }
    }

    public void Initialize(WordPuzzleBoard parentBoard, Dimensions coordinates)
    {
        _parentBoard = parentBoard;
        Coordinates = coordinates;
    }

    public void Clear()
    {
        IsAnswer = false;
        VisuaState = VisualState.None;
        Occupied = false;
        DisplayValue = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("We starting");
        _parentBoard.OnStartedDrag(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("We starting");
        _parentBoard.OnEnteredElement(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _parentBoard.OnEndDrag(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("We dragging");
    }
}