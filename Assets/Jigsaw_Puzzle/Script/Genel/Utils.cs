using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum DebugType
{
    Log,
    Warning,
    Error
}
public static class Utils
{
    public static void WaitAndDo(float waitinTime, System.Action action)
    {
        // Bekle
        DOVirtual.DelayedCall(waitinTime,
            () =>
            {
                // Fonksiyonu aktif et.
                action?.Invoke();
            });
    }
    public static List<Button> FindAllButtons(Transform buttonParent, bool incLudeParent = false)
    {
        List<Button> allButtons = new List<Button>();

        List<Transform> list = new List<Transform>();
        list.Add(buttonParent);
        for (int e = 0; e < list.Count; e++)
        {
            if (list[e].TryGetComponent(out Button buton))
            {
                if (e == 0)
                {
                    if (incLudeParent)
                    {
                        allButtons.Add(buton);
                    }
                }
                else
                {
                    allButtons.Add(buton);
                }
            }
            for (int i = 0; i < list[e].childCount; i++)
            {
                list.Add(list[e].GetChild(i));
            }
        }
        return allButtons;
    }
    public static void Debug(string mesaj, DebugType debugType = DebugType.Log)
    {
        if (debugType == DebugType.Log)
        {
            UnityEngine.Debug.Log(mesaj);
        }
        if (debugType == DebugType.Warning)
        {
            UnityEngine.Debug.Log(mesaj);
        }
        if (debugType == DebugType.Error)
        {
            UnityEngine.Debug.Log(mesaj);
        }
    }
}