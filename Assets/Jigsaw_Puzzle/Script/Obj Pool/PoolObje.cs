//using DG.Tweening;
using UnityEngine;

public class PoolObje : MonoBehaviour
{
    [Header("Bu obje icin kullanılacak havuz")]
    public Pooler havuzum;

    public virtual void ObjeHavuzEnter()
    {
        gameObject.SetActive(false);
    }
    public virtual void ObjeHavuzExit()
    {
        gameObject.SetActive(true);
    }
    public void EnterHavuz()
    {
        gameObject.SetActive(false);
        havuzum.ObjeyiHavuzaYerlestir(this);
    }
    public void EnterHavuzInTime(float time)
    {
        //DOVirtual.DelayedCall(time, () =>
        //{
        //    havuzum.ObjeyiHavuzaYerlestir(this);
        //});
    }
}