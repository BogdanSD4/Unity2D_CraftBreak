using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [SerializeField] private AllRes _currentResource = new AllRes();
    [SerializeField] private Image _imageRenderer;

    private GameControler gameControler;
    private Transform currentCell;
    private bool beingDrag;
    private bool moveEnd;

    private void Start()
    {
        _imageRenderer.sprite = _currentResource.Sprite;
    }
    private void Update()
    {
        if (beingDrag)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(pos.x, pos.y);
        }
        if (moveEnd)
        {
            Vector3 target = new Vector3(currentCell.position.x, currentCell.position.y, transform.position.z);

            transform.position = Vector3.MoveTowards(transform.position, target, 10f * Time.deltaTime);
            if(transform.position == target)
            {
                currentCell.gameObject.layer = GameControler.CellLayerClose;
                //_imageRenderer.sortingOrder = defaultLayer;
                moveEnd = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
       //Gizmos.DrawSphere(transform.position, .7f);
    }
    private void PointerExit()
    {
        Collider2D[] coll = Physics2D.OverlapCircleAll(transform.position, .7f);

        Transform result = null;
        float distance = Mathf.Infinity;
        for (int i = 0; i < coll.Length; i++)
        {
            if(coll[i].gameObject.layer == GameControler.CellLayerOpen)
            {
                float dis = Vector3.Distance(transform.position, coll[i].transform.position);
                if (dis < distance)
                {
                    distance = dis;
                    result = coll[i].transform;
                } 
            }
        }

        if (result != null)
        {
            if (result.gameObject.CompareTag("BreakPoint"))
                GameControler.currentBreakItem = this;
            else if (result.gameObject.CompareTag("CraftPoint"))
            {
                gameControler.AddToCraftList(this);
                gameControler.RecipeCheck();
            }
            else GameControler.CloseMenu(GameControler.infoMenu.transform);

            currentCell = result;
        }

        moveEnd = true;
    }

    private void Info()
    {
        if (currentCell.gameObject.CompareTag("Inventory")) 
        {
            GameControler.currentItem = this;
            GameControler.SetInfo(_currentResource.info);
        }
    }

    public AllRes GetRes() => _currentResource;
    public void SetGameControler(GameControler controler) => gameControler = controler;
    public void SetTarget(Transform transform, AllRes res)
    {
        _currentResource.Type = res.Type;
        _currentResource.Sprite = res.Sprite;
        _currentResource.name = res.name;
        _currentResource.info = res.info;
        _currentResource.Craft = res.Craft;
        _currentResource.Break = res.Break;

        currentCell = transform;
        currentCell.gameObject.layer = GameControler.CellLayerClose;
        moveEnd = true;
    }

    private void OnDestroy()
    {
        currentCell.gameObject.layer = GameControler.CellLayerOpen;
    }

    public void MouseUp()
    {
        if (beingDrag)
        {
            PointerExit();
            beingDrag = false;
        }
    }

    public void MouseDown()
    {
        Info();
    }

    public void MouseDrag()
    {
        if (GameControler.currentBreakItem == this) GameControler.currentBreakItem = null;
        if (currentCell.gameObject.CompareTag("CraftPoint"))
        {
            gameControler.DelFromCraftList(this);
            gameControler.RecipeCheck();
        }

        currentCell.gameObject.layer = GameControler.CellLayerOpen;
        //_spriteRenderer.sortingOrder = 2;
        beingDrag = true;
    }
}
