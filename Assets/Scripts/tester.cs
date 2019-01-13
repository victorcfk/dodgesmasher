using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class tester : MonoBehaviour {

    enum THINGLIST
    {
        thing1,
        thing2,
        thing3,
        thing4,
    }
		//bug

    readonly string[] thinga = { "str1", "str2", "str3", "str4" };
    string getta(THINGLIST things)
    {
        switch (things)
        {
            case THINGLIST.thing1:
                return thinga[0];
            case THINGLIST.thing2:
                return thinga[1];
            case THINGLIST.thing3:
                return thinga[2];
            case THINGLIST.thing4:
                return thinga[3];
            default:
                return string.Empty;
        }
    }

    string getta2(THINGLIST things)
    {
        return thinga[(int)things];
    }

    string getta3(THINGLIST things)
    {
        switch (things)
        {
            case THINGLIST.thing1:
                return "str1";
            case THINGLIST.thing2:
                return "str2";
            case THINGLIST.thing3:
                return  "str3";
            case THINGLIST.thing4:
                return  "str4";
            default:
                return string.Empty;
        }
    }

    [ContextMenu("test1")]
    void test1()
    {
        UnityEngine.Debug.Log("test1: " + ScriptPerformanceTester.TestAnActionAndGetTheAverageTicksTaken(() => getta(THINGLIST.thing3), 999999,999));
    }

    [ContextMenu("test2")]
    void test2()
    {
        UnityEngine.Debug.Log("test2: " + ScriptPerformanceTester.TestAnActionAndGetTheAverageTicksTaken(() => getta2(THINGLIST.thing3), 999999, 999));
    }

    [ContextMenu("test3")]
    void test3()
    {
        UnityEngine.Debug.Log("test3: " + ScriptPerformanceTester.TestAnActionAndGetTheAverageTicksTaken(() => getta3(THINGLIST.thing3), 999999, 999));
    }

    [ContextMenu("testall")]
    void testall()
    {
        test1();
        test2();
        test3();
    }
}
