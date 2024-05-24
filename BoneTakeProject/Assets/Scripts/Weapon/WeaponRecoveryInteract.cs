using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRecoveryInteract : MonoBehaviour
{
    public bool isStone;
    private bool isRecovered = false;

    public void NpcInteraction()
    {
        int maxWeaponHp = 0;
        int nowWeaponHp = 0;
        int recoveryValue = 0;
        if (!isRecovered)
        {
            if (isStone)
            {
                //착용중인 무기의 최대 무기HP 가져오기
                maxWeaponHp = WeaponData.instance.GetName_WeaponLifeCount(PlayerDataManager.instance.nowPlayer.weaponName);
            
                //현재 착용중인 무기의 현재 무기HP 가져오기
                nowWeaponHp = PlayerDataManager.instance.nowPlayer.weaponHP;
            
                recoveryValue = Mathf.FloorToInt((maxWeaponHp - nowWeaponHp) / 2.0f);
                Debug.Log("maxWeaponHp : " + maxWeaponHp + ", nowWeaponHp : " + nowWeaponHp + ", recoveryValue : " + recoveryValue );
            
                CharacterController2D.instance.playerAttack.weaponManager.weaponLife += recoveryValue;
            }
            else //방부업자 로직
            {
                recoveryValue = WeaponData.instance.GetName_WeaponLifeCount(PlayerDataManager.instance.nowPlayer.weaponName);
                CharacterController2D.instance.playerAttack.weaponManager.weaponLife = recoveryValue;
            }
            isRecovered = true;
            
            //TestCode...
            gameObject.GetComponent<SpriteRenderer>().color = Color.black;
        }
        else
        {
            Debug.Log("1회만 회복 가능합니다.");
        }
    }
}