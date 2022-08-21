using System.Collections.Generic;
using UnityEngine;
using Project.Architecture;

public abstract class Grave : BaseMonoBehaviour
{
    [Header("Base Grave Attrubutes")]
    [SerializeField] protected int _exp;
    public int depth;
    [SerializeField] protected List<EarthLayer> _earthLayers = new();
    [SerializeField] protected List<Loot> _loot = new();
    [SerializeField] protected List<GameObject> _layerPrefabs = new();
    [SerializeField] protected GameObject _layerParent;
    [Tooltip("Layer shift by Y")]
    [SerializeField] protected float _shift;
    [SerializeField] protected GameObject _floor;
    [SerializeField] protected GameObject _wall;

    [Header("Layers Attributes")]
    [Header("Hp")]

    [Tooltip("Average HP")]
    public float middleLayerHp;
    [Tooltip("Deviation of HP from the average value (%)")] 
    [SerializeField, Range(0, 1)] protected float _layerHpDeviation;

    [Header("Resistance")]
    [Tooltip("Average Resistance")] 
    [Range(0, 1)] public float middleLayerResistance;
    [Tooltip("Deviation of Resistance from the average value (%)")] 
    [SerializeField, Range(0, 1)] protected float _layerResistanceDeviation;

    protected virtual void OnEnable()
    {
        MainLogic.main.allGraves.Add(this);
    }

    protected void OnDisable()
    {
        foreach (EarthLayer layer in _earthLayers)
        {
            layer.OnEarthLayerDigOut -= DigOut;
        }
    }

    protected virtual void DigOut(EarthLayer earthLayer)
    {
        earthLayer.OnEarthLayerDigOut -= DigOut;
        _earthLayers.Remove(earthLayer);
        Destroy(earthLayer.gameObject);
        if (_earthLayers.Count > 0)
            return;
        print("grave Destroy");
        //Destroy(gameObject);
    }

    public void InstantiateLayers()
    {
        // Earth width
        float _layerWidth = _layerPrefabs[0].GetComponent<BoxCollider>().size.y / 10;
        float _yShift = 0;

        for (int i = 0; i < depth; i++)
        {
            // Instantiate
            int layerNum = Random.Range(0, _layerPrefabs.Count);
            EarthLayer newLayer = Instantiate(_layerPrefabs[layerNum], _layerParent.transform).GetComponent<EarthLayer>();

            newLayer.Grave = this;
            newLayer.OnEarthLayerDigOut += DigOut;

            // Move
            newLayer.transform.localPosition += new Vector3(0, _yShift, 0);
            _yShift -= _layerWidth + _shift;

            // Calculate Parametres
            float hpDevation = middleLayerHp * _layerHpDeviation;
            float resistanceDevation = middleLayerResistance * _layerResistanceDeviation;
            newLayer.Health = middleLayerHp + Random.Range(-hpDevation, hpDevation);
            newLayer.Resistance = middleLayerResistance + Random.Range(-resistanceDevation, resistanceDevation);

            _earthLayers.Add(newLayer);
        }

        InstantiateWallsAndFloor();
    }

    protected void InstantiateWallsAndFloor()
    {
        for (int i = 0; i < depth; i++)
        {
            GameObject wall = Instantiate(_wall, _earthLayers[i].transform);
            wall.transform.parent = _earthLayers[i].transform.parent.parent;
            foreach (var mat in wall.GetComponentsInChildren<MeshRenderer>())
                mat.material = _earthLayers[i].GetComponent<MeshRenderer>().material;

            // ��������
            wall.transform.localScale = new Vector3(1, 1, 1);
        }

        int floorNum = depth - 1;

        GameObject floor = Instantiate(_floor, _earthLayers[floorNum].transform);
        floor.transform.parent = _earthLayers[floorNum].transform.parent.parent;
        floor.GetComponentInChildren<MeshRenderer>().material = _earthLayers[floorNum].GetComponent<MeshRenderer>().material;
        // ��������
        floor.transform.localScale = new Vector3(1, 1, 1);
    }
}