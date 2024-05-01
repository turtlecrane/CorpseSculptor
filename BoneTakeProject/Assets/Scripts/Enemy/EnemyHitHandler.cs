using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyHitHandler : MonoBehaviour
{
    public EnemyAI enemyAIScript;
    public Animator animator;
    public float life; //현재 남은 HP
    public float knockbackBasicForce; //피격시 넉백의 강도
    public bool isCorpseState; //시체상태인지
    public bool isExtracted; //발골완료된 상태인지
    
    private Rigidbody2D rb;
    private bool isInvincible = false; //무적상태인지
    private bool isFading = false; // fade 중인지 상태 확인
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (life <= 0)
        {
            StartCoroutine(EnemyKnockdown());
        }

        //...TEST CODE
        if (isCorpseState)
        {
            var b = this.GetComponentInChildren<TextMeshPro>();
            b.text = "[시체 상태]";
        }

        //발골됐으면
        if (isExtracted && !isFading)
        {
            StartCoroutine(FadeOutAndDestroy(1f));
            isFading = true;
        }
    }
    
    /// <summary>
    /// PlayerEventKey스크립트에서 SendMessage로 호출됨
    /// </summary>
    /// <param name="damage"></param>
    public void Enemy_ApplyDamage(float damage) {
        if (!isInvincible)
        {
            CharacterController2D charCon2D = GameManager.Instance.GetCharacterController2D();
            //피격 (Hit) 애니메이션 트리거 설정
            animator.SetTrigger("Hit");

            life -= damage; // 라이프 차감
            if (charCon2D.playerAttack.weapon_type != Weapon_Type.Basic 
                || charCon2D.playerAttack.weapon_type != Weapon_Type.etc)
            {
                charCon2D.playerAttack.weaponManager.weaponLife -=  1; //무기 HP를 1 줄임
            }
            rb.velocity = Vector2.zero; // 현재 속도를 0으로 초기화

            // 넉백 방향 결정 (캐릭터가 오른쪽을 바라보고 있으면 오른쪽으로, 그렇지 않으면 왼쪽으로 넉백)
            bool isFacingRight = GameManager.Instance.GetCharacterController2D().m_FacingRight;
            //knockbackBasicForce = 7000f;
            float knockbackForce = isFacingRight ? knockbackBasicForce : -knockbackBasicForce;

            rb.AddForce(new Vector2(knockbackForce, 0)); // 넉백 적용

            // 히트 효과 코루틴 실행
            StartCoroutine(HitTime());
        }
    }
    
    IEnumerator HitTime()
    {
        isInvincible = true;
        yield return new WaitForSeconds(0.1f); //0.1초동안 무적상태
        isInvincible = false;
    }
    
    IEnumerator EnemyKnockdown()
    {
        //Enemy 상태 초기화
        enemyAIScript.canMove = false;
        enemyAIScript.canRotation = false;
        enemyAIScript.canTracking = false;
        enemyAIScript.canAttack = false;
        enemyAIScript.isRunning = false;
        
        //공중에서 사망한경우
        if (!enemyAIScript.isGrounded)
        {
            // enemyAIScript.isGrounded가 true가 될 때까지 기다리기
            yield return new WaitUntil(() => enemyAIScript.isGrounded == true);
        }
        
        animator.SetBool("Dead", true);
        isInvincible = true;
        
        Collider2D[] colliders = GetComponents<Collider2D>();//몬스터에게 붙어있는 콜라이더 컴포넌트 모두 가져오기
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
        
        yield return new WaitForSeconds(0.25f);
        foreach (var collider in colliders) // 모든 Collider2D 컴포넌트의 isTrigger를 true로 설정
        {
            collider.isTrigger = true;
        }
        
        yield return new WaitForSeconds(1f);
        
        isCorpseState = true; //시체 파밍 상태로 전환
    }
    
    // 알파값을 천천히 0으로 감소시키고 오브젝트 제거
    IEnumerator FadeOutAndDestroy(float duration)
    {
        yield return new WaitForSeconds(2f);//1초 기다리기
        
        SpriteRenderer spriteRenderer = animator.GetComponent<SpriteRenderer>();

        float counter = 0;
        Color spriteColor = spriteRenderer.color;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, counter / duration);
            spriteRenderer.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
