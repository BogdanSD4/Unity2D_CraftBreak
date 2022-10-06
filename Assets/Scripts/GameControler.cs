using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameControler : MonoBehaviour
{
    [SerializeField] private Transform _inventory;
    [SerializeField] private BoxCollider2D _inventoryColl;

    [SerializeField] private Transform _craftMenu;
    [SerializeField] private Transform _breakMenu;
    [SerializeField] private Transform _itemPack;
    [SerializeField] private Item _itemPrefab;
    [SerializeField] private TextMeshProUGUI _infoMenu;
    [SerializeField] private Image _recipeImage;
    [SerializeField] private Recipe _recipePrefab;
    [SerializeField] private RectTransform _recipeContent;
    [SerializeField] private Collider2D _recipeColl;

    [SerializeField] private List<AllRes> _resourcesList;

    private static List<AllRes> resourceListStatic;
    private List<Resources> craftList = new List<Resources>();
    private List<Item> itemInCraft = new List<Item>();
    public static TextMeshProUGUI infoMenu;
    public static Item currentItem;
    public static Item currentBreakItem;
    public const int CellLayerOpen = 6;
    public const int CellLayerClose = 7;

    private void Start()
    {
        resourceListStatic = _resourcesList;
        infoMenu = _infoMenu;
        CreateRecipeList();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!_inventoryColl.OverlapPoint
                (Camera.main.ScreenToWorldPoint(Input.mousePosition)))
            {
                CloseMenu(infoMenu.transform);
            }
        }
        if (_recipeContent.gameObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!_recipeColl.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
            {
                _recipeColl.gameObject.SetActive(false);
            }
        }
    }

    private void CreateRecipeList()
    {
        int distance = 0;
        for(int i = 0; i < _resourcesList.Count; i++)
        {
            if(_resourcesList[i].Craft.Count != 0)
            {
                Recipe recipe = Instantiate(_recipePrefab, _recipeContent);
                recipe.SetRecource(_resourcesList[i]);

                _recipeContent.sizeDelta += new Vector2(0, 130);

                RectTransform rc = recipe.GetComponent<RectTransform>();
                rc.pivot += new Vector2(0, distance);
                distance++;
            }
        }
    }

    public void GetItem()
    {
        Transform result = null;
        AllRes res = null;
        if((result = GetOpenCellInInventory()) != null)
        {
            Item item = Instantiate(_itemPrefab, _itemPack.position, Quaternion.identity, _itemPack);
            float percent = Random.Range(0.1f, 100);

            if (percent < 40) res = GetResourcesType(Resources.Wood);
            else
            {
                percent -= 40;
                if (percent < 30) res = GetResourcesType(Resources.Metal);
                else
                {
                    percent -= 30;
                    if (percent < 30) res = GetResourcesType(Resources.Rock);
                }
            }

            item.SetGameControler(this);
            item.SetTarget(result, res);
        }
    }

    public Transform GetOpenCellInInventory()
    {
        foreach(Transform i in _inventory)
        {
            if (i.gameObject.layer == CellLayerOpen)
                return i;
        }
        return null;
    }

    private AllRes GetResourcesType(Resources resources)
    {
        for(int i = 0; i < _resourcesList.Count; i++)
        {
            if (_resourcesList[i].Type == resources) return _resourcesList[i];
        }
        return null;
    }

    public void BreakButton() => BreakItem();
    private bool BreakItem()
    {
        if(_breakMenu.gameObject.layer == CellLayerOpen)
        {
            Debug.Log("BreakCell is empty");
            return false;
        }

        foreach(Transform i in _breakMenu)
        {
            if(i.gameObject.layer == CellLayerClose)
            {
                Debug.Log("BreakMenu not empty");
                return false;
            }
        }

        float percent = 0;
        int currentCell = 0;
        int dropCount;
        AllRes newRes = null;
        List<AllRes.BreakParameters> mainRes = currentBreakItem.GetRes().Break;
        dropCount = mainRes.Count;

        if (mainRes.Count != 0)
        {
            foreach (Transform i in _breakMenu)
            {
                for (int j = currentCell; j < mainRes.Count; j++)
                {
                    percent = Random.Range(0.1f, 100);
                    dropCount--;

                    if (percent < mainRes[j].DropChance)
                    {
                        newRes = GetResourcesType(mainRes[j].Break);

                        Item item = Instantiate(_itemPrefab, i.position, Quaternion.identity, _itemPack);

                        item.SetGameControler(this);
                        item.SetTarget(i, newRes);

                        break;
                    }
                }

                if (dropCount == 0) break;
                currentCell++;
            }

            Destroy(currentBreakItem.gameObject);
        }
        else Debug.Log("Resource can`t be broken");

        return true;
    }


    public void CraftItem()
    {
        AllRes newRes = null;
        if (!_recipeImage.gameObject.activeSelf) Debug.Log("Recipe not true");
        else if (_craftMenu.gameObject.layer == CellLayerClose) Debug.Log("Free the CraftCell");
        else
        {
            _recipeImage.gameObject.SetActive(false);
            for (int i = 0; i < _resourcesList.Count; i++)
            {
                if (_resourcesList[i].Sprite == _recipeImage.sprite) newRes = _resourcesList[i];
            }

            for (int i = 0; i < craftList.Count; i++)
            {
                Destroy(itemInCraft[i].gameObject);
            }
            itemInCraft = new List<Item>();
            craftList = new List<Resources>();

            Item item = Instantiate(_itemPrefab, _craftMenu.position, Quaternion.identity, _itemPack);

            item.SetGameControler(this);
            item.SetTarget(_craftMenu, newRes);
        }
    }
    public void RecipeCheck()
    {
        int count = 0;
        int res = 0;

        for(int i = 0; i < _resourcesList.Count; i++)
        {
            res = 0;
            if (_resourcesList[i].Craft.Count != 0)
            {
                for (int j = 0; j < craftList.Count; j++)
                {
                    if (_resourcesList[i].Craft.Contains(craftList[j])) res++;
                }
                if (res == _resourcesList[i].Craft.Count)
                {
                    count++;
                    _recipeImage.sprite = _resourcesList[i].Sprite;
                }
            }
        }

        if (count == 0)
        {
            _recipeImage.gameObject.SetActive(false);
            _recipeImage.sprite = null;
        }
        else _recipeImage.gameObject.SetActive(true);
    }


    public void ItemDell()
    {
        Destroy(currentItem.gameObject);
        CloseMenu(infoMenu.transform);
    }

    public void AddToCraftList(Item resources)
    {
        itemInCraft.Add(resources);
        craftList.Add(resources.GetRes().Type);
    }
    public void DelFromCraftList(Item resources)
    {
        itemInCraft.Remove(resources);
        craftList.Remove(resources.GetRes().Type);
    }

    public static Sprite GetResSprite(Resources resources)
    {
        for (int i = 0; i < resourceListStatic.Count; i++)
        {
            if (resourceListStatic[i].Type == resources) return resourceListStatic[i].Sprite;
        }
        return null;
    }
    public static void CloseMenu(Transform transform) => transform.gameObject.SetActive(false);
    public static void SetInfo(string text)
    {
        infoMenu.text = text;
        infoMenu.gameObject.SetActive(true);
    }

    [ContextMenu("CheckList")]
    private void CheckList()
    {
        int num = 0;
        List<Resources> count = new List<Resources>();
        foreach(var i in System.Enum.GetValues(typeof(Resources)))
        {
            num++;
            if(num > _resourcesList.Count) _resourcesList.Add(new AllRes());
            count.Add((Resources)i);
            
        }

        for (int i = 0; i < _resourcesList.Count; i++)
        {
            _resourcesList[i].Type = count[i];
            _resourcesList[i].name = _resourcesList[i].Type.ToString();

            for (int j = 0; j < _resourcesList[i].Break.Count; j++)
            {
                _resourcesList[i].Break[j].name = _resourcesList[i].Break[j].Break.ToString();
            }
        }
    }
}

[System.Serializable]
public class AllRes
{
    [HideInInspector] public string name;

    public Resources Type;
    public Sprite Sprite;
    public List<Resources> Craft = new List<Resources>();
    public List<BreakParameters> Break = new List<BreakParameters>();

    [System.Serializable]
    public class BreakParameters
    {
        [HideInInspector] public string name;

        public Resources Break;
        public float DropChance;
    }
    [TextArea]
    public string info;
}
public enum Resources
{
    Wood,
    Metal,
    Rock,
    Axe,
    SlingShot,
    Board,
    Hammer,
    Nails,
}
