using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attribute
{
    public string key;
    public AttributeValue value;
}

[System.Serializable]
public class AttributeValue
{
    public enum ValueType { Float, Int, String }
    public ValueType type;
    public float floatValue;
    public int intValue;
    public string stringValue;
    public bool networked = false;

    public object GetValue()
    {
        switch (type)
        {
            case ValueType.Float:
                return floatValue;
            case ValueType.Int:
                return intValue;
            case ValueType.String:
                return stringValue;
            default:
                return null;
        }
    }

    public void SetValue( object value)
    {
        switch (type)
        {
            case ValueType.Float: floatValue = (float)value; break;
            case ValueType.Int: intValue = (int)value; break;
            case ValueType.String: stringValue = value.ToString(); break;
        }
    }
}

public class Attributes : MonoBehaviour
{
    public List<Attribute> AttributesList = new List<Attribute>();
    private Dictionary<string, AttributeValue> OAttributeDictionary = new Dictionary<string, AttributeValue>();
    private Dictionary<string, AttributeValue> AttributeDictionary = new Dictionary<string, AttributeValue>();
    public List<string> NetworkAttributes = new List<string>();

    void Start()
    {
        for (int i = 0; i < AttributesList.Count; i++ )
        {
            OAttributeDictionary.Add(AttributesList[i].key, AttributesList[i].value);
            AttributeDictionary.Add(AttributesList[i].key, AttributesList[i].value);

            if (AttributesList[i].value.networked)
            {
                NetworkAttributes.Add(AttributesList[i].key);
            }
        }

        GameMain.RegisterAttributes(this);
    }

    public AttributeValue GetOriginal(string key)
    {
        return OAttributeDictionary[key.ToLower()];
    }

    public AttributeValue Get(string key) 
    {
        return AttributeDictionary[key.ToLower()];
    }

    public void Reset(string key)
    {
        AttributeDictionary[key.ToLower()].intValue = OAttributeDictionary[key.ToLower()].intValue;
        AttributeDictionary[key.ToLower()].stringValue = OAttributeDictionary[key.ToLower()].stringValue;
        AttributeDictionary[key.ToLower()].floatValue = OAttributeDictionary[key.ToLower()].floatValue;
    }
}
