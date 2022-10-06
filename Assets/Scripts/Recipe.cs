using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Recipe : MonoBehaviour
{
    [SerializeField] private AllRes _resource;
    [SerializeField] private Image _meinRes;
    [SerializeField] private List<Image> _ingredients;

    private void Start()
    {
        _meinRes.sprite = _resource.Sprite;
        for(int i = 0; i < _resource.Craft.Count; i++)
        {
            _ingredients[i].sprite = GameControler.GetResSprite(_resource.Craft[i]);
            _ingredients[i].color = Color.white;
        }
    }

    public void SetRecource(AllRes res) => _resource = res;
}
