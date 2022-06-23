using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField]
    private RectTransform rect;

    [Header("1.��, 2.��ź")]
    [SerializeField]
    private Sprite[] weaponImage;

    public int weaponIndex { get; private set; }

    private PlayerAttack playerAttack;

    [SerializeField]
    private GameObject gun;

    private EventParam eventParam;

    private void Start()
    {
        playerAttack = GetComponent<PlayerAttack>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !playerAttack.IsAttack)
            ChangePanel();
    }

    public void ChangePanel()
    {
        rect.DOKill();
        weaponIndex++;
        if (weaponIndex >= 2) weaponIndex = 0;

        if (weaponIndex == 0) gun.SetActive(true);
        else gun.SetActive(false);

        rect.anchoredPosition = new Vector3(265, 31, 0);
        rect.transform.GetChild(0).GetComponent<Image>().sprite = weaponImage[weaponIndex];
        rect.DOAnchorPosX(3, 1f);
    }


    public void MaxBullet()
    {
        EventManager.TriggerEvent("MAX", eventParam);
    }
}
