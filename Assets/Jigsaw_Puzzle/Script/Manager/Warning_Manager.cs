using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class Messages
{
    [HideInInspector] public TextMeshProUGUI warningtext;
    [HideInInspector] public float warningTime;
    [HideInInspector] public float warningTimeNext;
    public Messages(TextMeshProUGUI warning)
    {
        warningtext = warning;
    }
    public bool ShowWarning()
    {
        warningTimeNext += Time.deltaTime;
        if (warningTimeNext > warningTime)
        {
            warningtext.gameObject.SetActive(false);
            warningTimeNext = 0;
            return true;
        }
        return false;
    }
}
public class Warning_Manager : Singletion<Warning_Manager>
{
    [SerializeField] private Transform mesajKutusu;
    private List<Messages> warnings = new List<Messages>();
    private List<Messages> allWarnings = new List<Messages>();
    private bool warningVar;
    private bool warning;
    public override void OnAwake()
    {
        for (int e = 0; e < mesajKutusu.childCount; e++)
        {
            warnings.Add(new Messages(mesajKutusu.GetChild(e).GetComponent<TextMeshProUGUI>()));
        }
    }
    public void ShowMessage(string msg, float duration = 1)
    {
        warningVar = true;
        mesajKutusu.gameObject.SetActive(true);
        bool findWarning = false;
        int bulunanText = mesajKutusu.childCount - 1;
        for (int e = warnings.Count - 1; e >= 0 && !findWarning; e--)
        {
            if (!warnings[e].warningtext.gameObject.activeSelf)
            {
                bulunanText = e;
                findWarning = true;
                warnings[bulunanText].warningtext.gameObject.SetActive(true);
            }
        }
        if (!findWarning)
        {
            bulunanText = warnings.Count;
            warnings.Add(new Messages(
                Instantiate(warnings[0].warningtext, mesajKutusu).GetComponent<TextMeshProUGUI>()));
        }
        warnings[bulunanText].warningTime = duration;
        warnings[bulunanText].warningtext.text = msg;
        warnings[bulunanText].warningtext.transform.SetAsFirstSibling();
        allWarnings.Add(warnings[bulunanText]);
    }
    private void Update()
    {
        if (warningVar)
        {
            warning = false;
            for (int e = allWarnings.Count - 1; e >= 0; e--)
            {
                if (allWarnings[e].ShowWarning())
                {
                    allWarnings.RemoveAt(e);
                }
                else
                {
                    warning = true;
                }
            }
            warningVar = warning;
            if (!warning)
            {
                mesajKutusu.gameObject.SetActive(false);
            }
        }
    }
}