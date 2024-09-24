using System.Collections.Generic;
using UnityEngine;

public class TestRunner : MonoBehaviour
{
    public bool LogSuccesses = true;
    private int _successCounter = 0;
    private int _failureCounter = 0;
    [field: Tagged, SerializeField] public string Example { get; private set; } // Use with fields 

    [Tagged] public int MisusedAttribute;
    [Tagged] public List<float> MisusedAttributes;
    [Tagged] public string CorrectlyUsedAttribute;
    [Tagged] public List<string> CorrectlyUsedAttributes;

    private int _totalCounter => _successCounter + _failureCounter;

    private void Start()
    {
        var taggedPoco = new ExampleTaggedPOCO();
        var normalPoco = new ExampleNormalPOCO();

        // Check target nulls
        TestCondition(Tagged.IsTagged((Component)null, "test"), false);
        TestCondition(Tagged.IsTagged((GameObject)null, "test"), false);
        TestCondition(Tagged.IsTagged((ITagged)null, "test"), false);
        TestCondition(Tagged.IsTagged((ExampleTaggedPOCO)null, "test"), false);
        TestCondition(Tagged.IsTagged((ExampleNormalPOCO)null, "test"), false);

        // Check tag nulls
        TestCondition(Tagged.IsTagged(this, null), false);
        TestCondition(Tagged.IsTagged(gameObject, null), false);
        TestCondition(Tagged.IsTagged(taggedPoco, null), false);
        TestCondition(Tagged.IsTagged(normalPoco, null), false);

        // Check tag zero length
        TestCondition(Tagged.IsTagged(this), false);
        TestCondition(Tagged.IsTagged(gameObject), false);
        TestCondition(Tagged.IsTagged(taggedPoco), false);
        TestCondition(Tagged.IsTagged(normalPoco), false);

        // Check target and tag nulls
        TestCondition(Tagged.IsTagged((Component)null, null), false);
        TestCondition(Tagged.IsTagged((GameObject)null, null), false);
        TestCondition(Tagged.IsTagged((ITagged)null, null), false);
        TestCondition(Tagged.IsTagged((ExampleTaggedPOCO)null, null), false);
        TestCondition(Tagged.IsTagged((ExampleNormalPOCO)null, null), false);

        // Check non-tagged
        TestCondition(Tagged.IsTagged(this, "test"), false);
        TestCondition(Tagged.IsTagged(gameObject, "test"), false);
        TestCondition(Tagged.IsTagged(taggedPoco, "test"), false);
        TestCondition(Tagged.IsTagged(normalPoco, "test"), false);

        // Check singular tags - component
        TestCondition(Tagged.AddTags(this, "TAG_0"), true);
        TestCondition(Tagged.AddTags(this, "TAG_0"), false);
        TestCondition(Tagged.IsTagged(this, "TAG_0"), true);
        TestCondition(Tagged.RemoveTags(this, "TAG_0"), true);
        // Check singular tags - game object
        TestCondition(Tagged.AddTags(gameObject, "TAG_0"), true);
        TestCondition(Tagged.AddTags(gameObject, "TAG_0"), false);
        TestCondition(Tagged.IsTagged(gameObject, "TAG_0"), true);
        TestCondition(Tagged.RemoveTags(gameObject, "TAG_0"), true);
        // Check singular tags - ITagged
        TestCondition(Tagged.AddTags(taggedPoco, "TAG_0"), true);
        TestCondition(Tagged.AddTags(taggedPoco, "TAG_0"), false);
        TestCondition(Tagged.IsTagged(taggedPoco, "TAG_0"), true);
        TestCondition(Tagged.RemoveTags(taggedPoco, "TAG_0"), true);
        // Check singular tags - normal POCO
        TestCondition(Tagged.AddTags(normalPoco, "TAG_0"), false);
        TestCondition(Tagged.IsTagged(normalPoco, "TAG_0"), false);
        TestCondition(Tagged.RemoveTags(normalPoco, "TAG_0"), false);

        // Check multiple tags - component
        TestCondition(Tagged.AddTags(this, "TAG_0", "TAG_1"), true);
        TestCondition(Tagged.AddTags(this, "TAG_0", "TAG_1"), false);
        TestCondition(Tagged.IsTagged(this, "TAG_0", "TAG_1"), true);
        TestCondition(Tagged.RemoveTags(this, "TAG_0"), true);
        TestCondition(Tagged.RemoveTags(this, "TAG_1"), true);
        // Check multiple tags - game object
        TestCondition(Tagged.AddTags(gameObject, "TAG_0", "TAG_1"), true);
        TestCondition(Tagged.AddTags(gameObject, "TAG_0", "TAG_1"), false);
        TestCondition(Tagged.IsTagged(gameObject, "TAG_0", "TAG_1"), true);
        TestCondition(Tagged.RemoveTags(gameObject, "TAG_0"), true);
        TestCondition(Tagged.RemoveTags(gameObject, "TAG_1"), true);
        // Check multiple tags - duplicate - ITagged
        TestCondition(Tagged.AddTags(taggedPoco, "TAG_0", "TAG_1"), true);
        TestCondition(Tagged.AddTags(taggedPoco, "TAG_0", "TAG_1"), false);
        TestCondition(Tagged.IsTagged(taggedPoco, "TAG_0", "TAG_1"), true);
        TestCondition(Tagged.RemoveTags(taggedPoco, "TAG_0"), true);
        TestCondition(Tagged.RemoveTags(taggedPoco, "TAG_1"), true);
        // Check multiple tags - normal POCO
        TestCondition(Tagged.AddTags(normalPoco, "TAG_0", "TAG_1"), false);
        TestCondition(Tagged.IsTagged(normalPoco, "TAG_0", "TAG_1"), false);
        TestCondition(Tagged.RemoveTags(normalPoco, "TAG_0"), false);
        TestCondition(Tagged.RemoveTags(normalPoco, "TAG_1"), false);

        // Check lots of tags
        TestCondition(Tagged.AddTags(this, "TAG_1", "TAG_2", "TAG_3", "TAG_4", "TAG_5", "TAG_6", "TAG_7", "TAG_8", "TAG_9"), true);
        TestCondition(Tagged.IsTagged(this, "TAG_1", "TAG_2", "TAG_3", "TAG_4", "TAG_5", "TAG_6", "TAG_7", "TAG_8", "TAG_9"), true);
        TestCondition(Tagged.IsTagged(this, "TAG_1", "TAG_9"), true);
        TestCondition(Tagged.RemoveTags(this, "TAG_1", "TAG_2", "TAG_3", "TAG_4", "TAG_5", "TAG_6", "TAG_7", "TAG_8", "TAG_9"), true);


        LogTestResults();
    }

    public void TestCondition(bool condition, bool expected)
    {
        if(condition != expected)
        {
            _failureCounter++;
            Debug.LogWarning($"[Test {_totalCounter}] Failed");
        }
        else
        {
            _successCounter++;
            if(LogSuccesses) Debug.Log($"[Test {_totalCounter}] Passed");
        }
    }

    public void LogTestResults()
    {
        if(_failureCounter == 0)
        {
            Debug.Log($"All {_successCounter} tests passed successfully.");
        }
        else
        {
            Debug.LogError($"{_failureCounter} tests failed out of {_totalCounter}.");
        }
    }
}

public class ExampleTaggedPOCO : ITagged
{
    private HashSet<string> _tagSet = new HashSet<string>();

    public ExampleTaggedPOCO()
    {
        _tagSet = new HashSet<string>();
    }

    public bool AddTags(params string[] tags)
    {
        if(tags == null || tags.Length == 0) return false;
        bool addedAtLeastOne = true;
        foreach(var tag in tags)
        {
            addedAtLeastOne &= _tagSet.Add(tag);
        }
        return addedAtLeastOne;
    }

    public bool IsTagged(params string[] tags)
    {
        if(tags == null || tags.Length == 0) return false;
        foreach(var tag in tags)
        {
            if(!_tagSet.Contains(tag))
            {
                return false;
            }
        }
        return true;
    }

    public bool RemoveTags(params string[] tags)
    {
        if(tags == null || tags.Length == 0) return false;
        bool removedAtLeastOne = true;
        foreach(var tag in tags)
        {
            removedAtLeastOne &= _tagSet.Remove(tag);
        }
        return removedAtLeastOne;
    }
}

public class ExampleNormalPOCO
{
    public ExampleNormalPOCO()
    {
    }
}