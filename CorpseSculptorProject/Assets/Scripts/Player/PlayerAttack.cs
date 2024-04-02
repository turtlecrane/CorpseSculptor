using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public enum Weapon_Type
{
    Basic = 2,
    etc = 0
}

public class PlayerAttack : MonoBehaviour
{
    public Weapon_Type weapon_type;
    public bool canAttack = true;
    public bool isAbleMultipleAttack; //다중타수가 가능한 상태인지 판별
    public bool isAttacking;
    public int count = 0;
    public float multiAtk_maxTime;
    
    private float AbleMultipleAttack_Time;
    private float comparisonTimer;
    private bool previousIsAttacking = false; // isAttacking의 이전 상태를 추적하기 위한 변수
    private CharacterController2D playerCharacterController2D; 
    void Start()
    {
        playerCharacterController2D = GetComponent<CharacterController2D>();
        
        //플레이어 캐릭터 컨트롤러에서 현재 플레이어가 착용중인 무기 정보를 가져옴
        weapon_type = playerCharacterController2D.playerdata.weaponType; 
    }

    void Update()
    {
        isAttacking = IsCurrentAnimationTag("attack");
        //TODO << 이쪽에서 2타 공격시 움직임이 작동되지 않음
        //playerCharacterController2D.canMove = !isAttacking;
        if (isAttacking) playerCharacterController2D.m_Rigidbody2D.velocity = Vector2.zero; 
        
        // 좌클릭 감지
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            #region ...HOLD CODE [보류중]
            //---------------------------
            if(!playerCharacterController2D.m_Grounded) return; //점프공격
            if(playerCharacterController2D.isDashing) return; //대쉬공격
            if(playerCharacterController2D.isClimbing) return; //벽타기때 공격하는경우
            //---------------------------
            #endregion
            
            //타격 시 카운팅 시작
            count = Mathf.Min(count + 1, (int)weapon_type); // count를 1 증가시키되, 무기 최대 타수를 초과하지 않도록 함
            
            isAbleMultipleAttack = true;
            
            if (weapon_type == Weapon_Type.Basic)//착용중인 무기가 기본 무기인경우 (손톱)
            {
                Player_BasicAttack();
            }
            else
            {
                //TODO 이곳에 다른 무기도 추가
                Debug.Log("제작중인 무기 혹은 존재하지 않는 무기 종류입니다.");
            }
            
            canAttack = false;
            StartCoroutine(AttackCooldown());
        }
        
        // 다중 공격 가능 상태 업데이트
        UpdateMultiAttackState(ref isAbleMultipleAttack, ref AbleMultipleAttack_Time, multiAtk_maxTime);
    }
    
    /// <summary>
    /// 공격 상태를 업데이트하는 함수
    /// </summary>
    /// <param name="extraDuration">연속 공격 가능 상태에 대해 추가 시간</param>
    void UpdateMultiAttackState(ref bool stateFlag, ref float timer, float extraDuration = 0)
    {
        //이동중이면 다중 공격이 아닌 첫타로 구분 
        if (playerCharacterController2D.playerMovement.horizontalMove != 0)
        {
            InitMultiAttackState(ref stateFlag, ref timer);
            return;
        }

        //공격중이며 이전에 공격한적이 없다면 타이머 시작(트리거)
        if (isAttacking && !previousIsAttacking)
        {
            timer = 0;
            stateFlag = true;
            previousIsAttacking = true;
        }
        
        //다중공격 가능시간으로 다중공격이 가능한 상태인지 판단
        if (stateFlag)
        {
            comparisonTimer += Time.deltaTime;
            if (IsCurrentAnimationTag("attack"))
            {
                timer += Time.deltaTime;
            }
            else
            {
                //각 무기에 해당하는 마지막타수의 경우에는 추가시간을 기다리지 않고 바로 다중 공격 상태 종료
                if ((count == (int)weapon_type && comparisonTimer > timer) ||
                    (count != (int)weapon_type && comparisonTimer > timer + extraDuration))
                {
                    InitMultiAttackState(ref stateFlag, ref timer);
                }
            }
        }
    }
    
    /// <summary>
    /// 다중 타수를 판단하는 속성들 초기화
    /// </summary>
    void InitMultiAttackState(ref bool _stateFlag, ref float _timer)
    {
        _stateFlag = false;
        previousIsAttacking = false;
        _timer = 0;
        comparisonTimer = 0;
        count = 0;
    }
    
    /// <summary>
    /// 플레이어 공격 - 기본 무기(손톱)
    /// </summary>
    public void Player_BasicAttack()
    {
        // 기본 공격 모션으로 전환
        playerCharacterController2D.animator.SetBool("IsBasicAttacking", true);
    
        // count가 2 이상일 때만 Num of Hits 설정
        if (count == (int)weapon_type)
        {
            playerCharacterController2D.animator.SetInteger("Num of Hits", count);
        }
    }
    
    /// <summary>
    /// Animator에서 현재 재생 중인 애니메이션의 길이를 가져오기
    /// </summary>
    /// <returns>현재 애니메이션의 길이</returns>
    float GetCurrentAnimationLength()
    {
        //(인자는 레이어의 번호)
        AnimatorStateInfo stateInfo = playerCharacterController2D.animator.GetCurrentAnimatorStateInfo(0);
        // 애니메이션의 길이(시간) 반환
        float animationLength = stateInfo.length;
        return animationLength;
    }
    
    /// <summary>
    /// Animator에서 현재 재생 중인 애니메이션의 태그를 판단
    /// </summary>
    /// <returns></returns>
    bool IsCurrentAnimationTag(string tag)
    {
        AnimatorStateInfo stateInfo = playerCharacterController2D.animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsTag(tag);
    }
    
    /// <summary>
    /// 플레이어 공격의 쿨타임을 결정
    /// </summary>
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(0.25f);
        canAttack = true;
    }
    
    
    
    
    //<---------애니메이션에서 호출 (Key Event)--------->
    
    /// <summary>
    /// 애니메이션의 Event호출로 호출되는 플레이어 공격 함수
    /// </summary>
    public void Player_DoBasicDamege()
    {
        float xOffset = playerCharacterController2D.m_FacingRight ? 2.125f : -2.125f;
        Collider2D[] basicHitBox = Physics2D.OverlapBoxAll(new Vector2(transform.position.x + xOffset, transform.position.y), new Vector2(2.25f, 2f), 0f);
        
        for (int i = 0; i < basicHitBox.Length; i++)
        {
            if (basicHitBox[i].gameObject != null && basicHitBox[i].CompareTag("Enemy"))
            {
                //해당 오브젝트의 상태 스크립트에 접근해서 HP를 깎아야함.
                //HP를 줄이는건 0+데이터ATK로 깎는다.
                //0인이유는 기본공격이라서. 다른 무기들은 도끼) 3+ATK 이런식이다
                basicHitBox[i].gameObject.SendMessage("ApplyDamage", 0+playerCharacterController2D.playerdata.playerATK);
            }
        }
    }
    
    public void EnablePleyerMovement()
    {
        playerCharacterController2D.canMove = true;
    }

    public void DisablePleyerMovement()
    {
        playerCharacterController2D.canMove = false;
        playerCharacterController2D.m_Rigidbody2D.velocity = Vector2.zero;
    }

}