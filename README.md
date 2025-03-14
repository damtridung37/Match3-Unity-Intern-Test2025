## TASK 1 

Đổi tên để hiển thị prefabs con cá
```cs
public class Constants 
{
    public const string GAME_SETTINGS_PATH = "gamesettings";

    public const string PREFAB_CELL_BACKGROUND = "prefabs/cellBackground";

    public const string PREFAB_NORMAL_TYPE_ONE = "prefabs/itemNormal01 1";

    public const string PREFAB_NORMAL_TYPE_TWO = "prefabs/itemNormal02 1";

    public const string PREFAB_NORMAL_TYPE_THREE = "prefabs/itemNormal03 1";

    public const string PREFAB_NORMAL_TYPE_FOUR = "prefabs/itemNormal04 1";

    public const string PREFAB_NORMAL_TYPE_FIVE = "prefabs/itemNormal05 1";

    public const string PREFAB_NORMAL_TYPE_SIX = "prefabs/itemNormal06 1";

    public const string PREFAB_NORMAL_TYPE_SEVEN = "prefabs/itemNormal07 1";

    public const string PREFAB_BONUS_HORIZONTAL = "prefabs/itemBonusHorizontal";

    public const string PREFAB_BONUS_VERTICAL = "prefabs/itemBonusVertical";

    public const string PREFAB_BONUS_BOMB = "prefabs/itemBonusBomb";
}
```

## TASK 2 

### Gp 1 + 2: Move items from the board to the bottom cells by tapping on them / Once an item moves to a bottom cell, you can't move it back to the board 


Tạo fuction MoveToBottomCell() dựa theo function Swap() có sẵn trong script Board.cs nhằm đảo 2 cells với nhau 


```cs
    public void MoveToBottomCell(Cell targetCell, Cell bottomCell, Action callback)
    {
        int index = bottomCells.IndexOf(bottomCell);
        Item item = targetCell.Item;
        targetCell.Free();
        bottomCell.Assign(item);
        oldCells[index] = targetCell;

        item.View.DOMove(bottomCell.transform.position, 0.3f).OnComplete(() => { if (callback != null) callback(); });
    }
```

Ở hàm Update() trong BoardController.cs ta có cụm Logic để nhận input người dùng khi tap vào cell bất kì. Trong trường hợp game mode là Timer thì ta có thể chạm vào cả bottom cell


```cs
        if(Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                Cell currentCell = hit.collider.GetComponent<Cell>();

                if(currentCell == null || currentCell.IsEmpty) return;
                Debug.Log("Cell: " + currentCell.name);

                if(m_board.IsBottomCell(currentCell))
                {
                    if(m_gameManager.Mode == GameManager.eLevelMode.MOVES)
                    {
                        return;
                    }
                    else{
                        m_board.Count ++;
                        m_board.MoveBackToOrigin(currentCell,() =>
                        {
                            IsBusy = false;
                        });
                    }
                }
                else
                {
                    Cell bottomCell = m_board.GetFirstEmptyCell();
                    if (bottomCell != null)
                    {
                        OnMoveEvent?.Invoke();
                        IsBusy = true;
                        m_board.Count --;
                        SetSortingLayer(currentCell, bottomCell);
                        m_board.MoveToBottomCell(currentCell, bottomCell, () =>
                        {
                            // Check if bottom cell can collapse
                            m_board.CollapseThreeSimilarCellInBottomCells();

                            if(m_gameManager.Mode == GameManager.eLevelMode.MOVES)
                            {                            
                                bool isFull = m_board.IsBottomCellFull();

                                if (isFull)
                                {
                                    Debug.LogWarning("Game Over");
                                    m_gameManager.GameOver();
                                }
                            }
                            IsBusy = false;

                            if(m_board.Count == 0)
                            {
                                //win
                                m_gameManager.SetState(GameManager.eStateGame.GAME_WIN);
                            }
                        });
                    }
                }

            }
        }
```
### Gp 3: If there are exactly three identical items in the bottom cells, they will be cleared


Ở hàm CollapseThreeSimilarCellInBottomCells() trong script Board.cs ta tiến hành xóa 3 cell cùng loại Item và cạnh nhau. Trong mode Timer ta đồng thời xóa cả cell khởi tạo của Item đấy

```cs
    public void CollapseThreeSimilarCellInBottomCells()
    {
        int count = 0;
        Cell currentCellType = null;
        for (int i = 0; i < bottomCells.Count; i++)
        {
            if(bottomCells[i].IsEmpty) break;

            if(currentCellType == null || !currentCellType.IsSameType(bottomCells[i]))
            {
                count = 1;
                currentCellType = bottomCells[i];
            }
            else
            {
                count++;
            }

            if(count == 3)
            {
                for (int j = i; j > i - 3; j--)
                {
                    bottomCells[j].ExplodeItem();
                    oldCells[j] = null;
                }
                break;
            }
        }
    }
```


### Gp 4: Clear the board to win


Ta tạo biến count trong script Board.cs để đếm số cell chứa Item còn lại trên Board. Mỗi khi tap vào cell trên Board thì count--. Trong mode Timer khi tap vào bottom cell thì count++. Lý do dùng count là để tránh duyệt toàn bộ Board liên tục để đếm số cell chứa Item còn lại.
Code chứa logic này nằm ở mục Gp 1 + 2 

### Gp 5: The player loses if he/she fills up all the bottom cells


Ở function IsBottomCellFull() trong script Board.cs ta kiểm tra bottom cell có full hay không bằng cách kiểm tra phần tử cuối có chữa Item hay không


```cs
    public bool IsBottomCellFull()
    {
        return !bottomCells[^1].IsEmpty;
    }
```

### Req 1: The number of identical item on the initial board must be divisible by 3 

Ở function Fill() trong script Board.cs ta tiến hành lưu tất cả các con cá vào trong list sau đó các cell trên board sẽ random dựa trên list này 

```cs
        Array values = Enum.GetValues(typeof(NormalItem.eNormalType));
        int count = values.Length;
        List<NormalItem.eNormalType> items = new List<NormalItem.eNormalType>();

        while(items.Count != boardSizeX * boardSizeY)
        {
            if(count > 0)
            foreach(NormalItem.eNormalType t in values)
            {
                // add 3 times
                items.Add(t);
                items.Add(t);
                items.Add(t);
                count --;
                if(items.Count == boardSizeX * boardSizeY) break;
            }
            else
            {
                NormalItem.eNormalType type = (NormalItem.eNormalType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
                // add 3 times
                items.Add(type);
                items.Add(type);
                items.Add(type);
            }
        }
```

### Req 2: The bottom area contains 5 cells


Ở function CreateBoard() trong script Board.cs ta tạo thêm 5 cell ở dưới board 

```cs
        // create bottom cells
        // 5 cell in the bottom -> -5
        float cacheY = (-boardSizeY+1) * 0.5f + 0.5f;
        Vector3 org = new Vector3(-5 * 0.5f + 0.5f, cacheY, 0f);
        for (int x = 0; x < 5; x++)
        {
            GameObject go = GameObject.Instantiate(prefabBG);
            go.name = "BOTTOM-CELL";
            go.transform.position = org + new Vector3(x, cacheY, 0f);
            go.transform.SetParent(m_root);

            Cell cell = go.GetComponent<Cell>();

            bottomCells.Add(cell);
            oldCells.Add(null);
        }
```
### Req 3 + 4: Show a simple Winning screen and Losing screen when the player wins


Ta tạo thêm UIPanelWin.cs dựa theo UIPanelGameOver.cs có sẵn và state mới là GameManager.eStateGame.GAME_WIN để xử lý trường hợp win. Sau đó ta thay đổi function OnGameStateChange() trong UIMainManager.cs


```cs
    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.SETUP:
                break;
            case GameManager.eStateGame.MAIN_MENU:
                ShowMenu<UIPanelMain>();
                break;
            case GameManager.eStateGame.GAME_STARTED:
                ShowMenu<UIPanelGame>();
                break;
            case GameManager.eStateGame.PAUSE:
                ShowMenu<UIPanelPause>();
                break;
            case GameManager.eStateGame.GAME_OVER:
                ShowMenu<UIPanelGameOver>();
                break;
            case GameManager.eStateGame.GAME_WIN:
                ShowMenu<UIPanelWin>();
                break;
        }
    }
```

### Req 5+6: Create a simple Home screen with an 'Autoplay'/'Autolose' button. Once clicked, the game will autoplay until it wins/losing, with each action having a 0.5s delay


Ở giao diện ta thêm 2 Toggle để chọn mode AutoWin hoặc AutoLose sau đó gán vào UIPanelMain.cs


```cs
    [SerializeField] private Toggle autoWin;
    [SerializeField] private Toggle autoLose;

    private void Awake()
    {
        autoLose.onValueChanged.AddListener(autoLoseValueChanged);
        autoWin.onValueChanged.AddListener(autoWinValueChanged);
    }

    private void autoWinValueChanged(bool arg0)
    {
        if (arg0)
        {
            autoLose.isOn = false;
        }
        m_mngr.SetAuto(autoWin.isOn|| autoLose.isOn, true);
    }

    private void autoLoseValueChanged(bool arg0)
    {
        if (arg0)
        {
            autoWin.isOn = false;
        }
    m_mngr.SetAuto(autoWin.isOn|| autoLose.isOn, false);
    }
```

Đây là logic tự động chơi. Cũng giống như người chơi chỉ khác là nó tự động chọn cell trên Board theo một số điều kiện nhất định


```cs
    private void AutoFill()
    {
        Cell lastBottomCell = m_board.GetLastFilledCell();
        Cell selectedCell = null;
        if (lastBottomCell == null)
        {
            selectedCell = m_board.GetFirstInBoardThatNotEmpty();
        }
        else
        {
            if (m_gameManager.isAutoWin)
                selectedCell = m_board.FindSimilarCellInBoard(lastBottomCell);
            else
                selectedCell = m_board.FindDifferentCellInBoard(lastBottomCell);
        }

        if (!m_gameManager.isAutoWin && m_gameManager.Mode == GameManager.eLevelMode.TIMER && m_board.IsBottomCellFull()) return;

        Cell bottomCell = m_board.GetFirstEmptyCell();
        if (bottomCell != null)
        {
            OnMoveEvent?.Invoke();
            IsBusy = true;
            m_board.Count--;
            SetSortingLayer(selectedCell, bottomCell);
            m_board.MoveToBottomCell(selectedCell, bottomCell, () =>
            {
                // Check if bottom cell can collapse
                m_board.CollapseThreeSimilarCellInBottomCells();

                if (m_gameManager.Mode == GameManager.eLevelMode.MOVES)
                {
                    bool isFull = m_board.IsBottomCellFull();

                    if (isFull)
                    {
                        Debug.LogWarning("Game Over");
                        m_gameManager.GameOver();
                    }
                }
                IsBusy = false;

                if (m_board.Count == 0)
                {
                    //win
                    m_gameManager.SetState(GameManager.eStateGame.GAME_WIN);
                }
            });
        }

    }
```
## TASK 3: 

### req1: Ensure the initial board contains all types of fish

Xem ở TASK 2 req 1 

### req2: Add an animation when an item move from the board to the cells and another animation whe identical item are cleared (scaling to 0)

Animation Move đã có sẵn logic trong hàm Swap() trong Board.cs (sử dụng dotWeen)
Animation clear sử dụng hàm ExplodeItem() ở Cell.cs đã có sẵn animation scale to 0 

### req3: Add a Time Attack Mode

Tạo thêm button ở UIPanelMain để chơi mode Timer sau đó gán vào script cùng tên 
logic thì đã kể rõ trong TASK 2 
Chế độ tự động chơi vẫn hoạt động trong mode này
