using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 애니메이션에서 호출되는 이벤트 함수 모음
/// </summary>
public class PlayerEventKey : MonoBehaviour
{
    /// <summary>
    /// 기본 공격의 데미지를 주는 함수
    /// </summary>
    public void Player_DoBasicDamege()
    {
        CharacterController2D charCon2D = CharacterController2D.instance;
        charCon2D.playerAttack.attackParticle.Play();
        float xOffset = charCon2D.m_FacingRight ? 1 : -1;
        Collider2D[] basicHitBox = Physics2D.OverlapBoxAll(new Vector2(transform.position.x + (xOffset * charCon2D.playerAttack.playerOffset_X), transform.position.y + 1 + charCon2D.playerAttack.playerOffset_Y), charCon2D.playerAttack.hitBoxSize, 0f);
        
        for (int i = 0; i < basicHitBox.Length; i++)
        {
            if (basicHitBox[i].gameObject != null && (basicHitBox[i].CompareTag("Enemy") || basicHitBox[i].CompareTag("Boss")))
            {
                string methodName = "Enemy_ApplyDamage";
                float damage = 0 + charCon2D.playerdata.playerATK;
                
                if (basicHitBox[i].CompareTag("Enemy"))
                {
                    basicHitBox[i].gameObject.SendMessage(methodName, damage);
                }
                else if(basicHitBox[i].CompareTag("Boss"))
                {
                    basicHitBox[i].gameObject.GetComponentInParent<BossHitHandler>().gameObject.SendMessage(methodName, damage);
                }
            }
        }
    }
    
    public void Player_DoKnifeDamage()
    {
        CharacterController2D charCon2D = CharacterController2D.instance;
        WeaponData weaponDataScript = WeaponData.instance;
        charCon2D.playerAttack.attackParticle.Play();
        float xOffset = charCon2D.m_FacingRight ? 1 : -1;
        Collider2D[] basicHitBox = Physics2D.OverlapBoxAll(new Vector2(transform.position.x + (xOffset * charCon2D.playerAttack.weaponManager.playerOffset_X), transform.position.y + 1 + charCon2D.playerAttack.weaponManager.playerOffset_Y), charCon2D.playerAttack.weaponManager.hitBoxSize, 0f);
        
        for (int i = 0; i < basicHitBox.Length; i++)
        {
            if (basicHitBox[i].gameObject != null && (basicHitBox[i].CompareTag("Enemy") || basicHitBox[i].CompareTag("Boss")))
            {
                string methodName = "Enemy_ApplyDamage";
                float damage = weaponDataScript.GetName_DamageCount(charCon2D.playerAttack.weapon_name) + charCon2D.playerdata.playerATK;
                
                if (basicHitBox[i].CompareTag("Enemy"))
                {
                    basicHitBox[i].gameObject.SendMessage(methodName, damage);
                }
                else if(basicHitBox[i].CompareTag("Boss"))
                {
                    basicHitBox[i].gameObject.GetComponentInParent<BossHitHandler>().gameObject.SendMessage(methodName, damage);
                }
            }
        }
    }
    
    /// <summary>
    /// 플레이어가 이동 가능하도록 함
    /// </summary>
    public void EnablePlayerMovement()
    {
        CharacterController2D.instance.canMove = true;
    }
    
    /// <summary>
    /// 플레이어를 조작불가 상태로 만듬
    /// </summary>
    public void DisablePlayerMovement()
    {
        CharacterController2D.instance.canMove = false;
        CharacterController2D.instance.m_Rigidbody2D.velocity = Vector2.zero;
    }
    
    //착지했을때 착지애니메이션 이 종료되면 착지상태를 false 상태로 만듬
    public void ExitPlayerLanding()
    {
        CharacterController2D.instance.isLanding = false;
        CharacterController2D.instance.isBigLanding = false;
    }
}
