using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image     imageFill;
    [SerializeField] private Transform targetTank;
    [SerializeField] private Vector3   offset = new Vector3(0, 1f, 0);

    float hp;
    float maxHP;

    // Update is called once per frame
    void Update()
    {
        imageFill.fillAmount = Mathf.Lerp(imageFill.fillAmount, hp / maxHP, Time.deltaTime * 5f);

        transform.position = targetTank.position + offset;
        transform.rotation = Quaternion.identity;
    }

    public void OnInit(float maxHP, Transform tankTransform)
    {
        this.maxHP           = maxHP;
        hp                   = maxHP;
        targetTank           = tankTransform;
        imageFill.fillAmount = 1;
    }

    public void SetNewHp(float hp)
    {
        this.hp = hp;
        imageFill.fillAmount = hp / maxHP;
    }
}