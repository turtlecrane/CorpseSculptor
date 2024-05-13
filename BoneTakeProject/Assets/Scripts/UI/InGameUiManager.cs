using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class InGameUiManager : MonoBehaviour
{
    public Image weaponIcon;
    public Transform hpPosition;
    public GameObject lifePointPrefab;
    public CinemachineVirtualCamera m_playerFollowCamera;
    public CinemachineVirtualCamera m_cursorFollowCamera;
    
    private CharacterController2D charCon2D;
    private PlayerGameData playerGameData;
    private WeaponManager weaponManager;
    private WeaponData weaponData;

    private float blinkTimer = 0f;
    private bool isRed = false;
    private float blinkInterval = 0.5f; // 번갈아가며 변경될 시간 간격
    public List<GameObject> lifePoints = new List<GameObject>();
    private int lastRecordedMaxHp;
    private int lastRecordedHp;
    
    private void Start()
    {
        playerGameData = GameManager.Instance.GetPlayerGameData();
        charCon2D = GameManager.Instance.GetCharacterController2D();
        weaponManager = charCon2D.playerAttack.weaponManager;
        weaponData = GameManager.Instance.GetWeaponData();
        
        lastRecordedMaxHp = playerGameData.playerData.playerMaxHP;
        lastRecordedHp = playerGameData.playerData.playerHP;
        CreateLifePoints(lastRecordedHp);
    }

    void Update()
    {
        WeaponUISystem();
        LifePointUISystem();
        CursorCheckState();
    }

    private void WeaponUISystem()
    {
        // 무기 HP의 백분율 계산
        float hpPercentage = (float)weaponManager.weaponLife / weaponData.GetName_WeaponLifeCount(charCon2D.playerAttack.weapon_name);
        Weapon_Type weaponType = charCon2D.playerAttack.weapon_type;
        Weapon_Name weaponName = charCon2D.playerAttack.weapon_name;
        
        // 원래 HP의 35% 이상인 경우
        if (hpPercentage > 0.35f)
        {
            SetWeaponIconState(Color.white, weaponData.weaponGFXSource.freshIcon[weaponData.GetName_WeaponID(weaponName)]);
            ResetBlinkTimer();
        }
        // 원래 HP의 30 ~ 12.6% 경우
        else if (hpPercentage > 0.126f && weaponType != Weapon_Type.Basic)
        {
            SetWeaponIconState(Color.white, weaponData.weaponGFXSource.rottenIcon[weaponData.GetName_WeaponID(weaponName)]);
            ResetBlinkTimer();
        }
        // HP가 매우 낮을 때 (12.6% 이하)
        else if (weaponType != Weapon_Type.Basic)
        {
            BlinkWeaponIcon();
        }
    }
    
    private void ResetBlinkTimer()
    {
        blinkTimer = 0f;
        isRed = false;
    }

    private void SetWeaponIconState(Color color, Sprite sprite)
    {
        weaponIcon.color = color;
        weaponIcon.sprite = sprite;
    }

    private void BlinkWeaponIcon()
    {
        blinkTimer += Time.deltaTime;

        if (blinkTimer >= blinkInterval)
        {
            blinkTimer = 0f;
            weaponIcon.color = isRed ? Color.white : Color.red;
            isRed = !isRed;
        }
    }

    private void LifePointUISystem()
    {
        int currentMaxHp = playerGameData.playerData.playerMaxHP;
        int currentHp = playerGameData.playerData.playerHP;

        // 최대 체력의 증감 확인 및 처리
        if (lastRecordedMaxHp != currentMaxHp)
        {
            HandleMaxHpChange(currentMaxHp);
        }

        // 현재 체력의 변화 확인 및 처리
        if (lastRecordedHp != currentHp)
        {
            UpdateLifePoints();
            lastRecordedHp = currentHp;
        }
    }
    
    // 최대 체력의 변화를 처리
    void HandleMaxHpChange(int currentMaxHp)
    {
        if (lastRecordedMaxHp < currentMaxHp)
        {
            Debug.Log("최대체력 증가");
            CreateLifePoints(currentMaxHp - lastRecordedMaxHp);
        }
        else
        {
            Debug.Log("최대체력 감소");
            DestroyLifePoints(lastRecordedMaxHp - currentMaxHp);
        }
        lastRecordedMaxHp = currentMaxHp;
    }
    
    /// <summary>
    /// 원하는만큼 라이프포인트 만들기 (UI상으로 왼쪽부터 만들어짐)
    /// </summary>
    /// <param name="hpValue">만들 라이프포인트 수</param>
    private void CreateLifePoints(int hpValue)
    {
        for (int i = 0; i < hpValue; i++)
        {
            GameObject lifePoint = Instantiate(lifePointPrefab, hpPosition);
            lifePoints.Add(lifePoint);
        }
    }
    
    /// <summary>
    /// 원하는만큼 라이프포인트 제거하기 (UI상으로 오른쪽부터 제거됨)
    /// </summary>
    /// <param name="hpValue">제거할 라이프포인트 수</param>
    private void DestroyLifePoints(int hpValue)
    {
        // hpValue만큼 반복하되, 리스트에 오브젝트가 남아 있는 동안만 실행
        for (int i = 0; i < hpValue && lifePoints.Count > 0; i++)
        {
            // 리스트의 첫 번째 오브젝트를 찾아서 파괴
            GameObject objectToDestroy = lifePoints[0];
            Destroy(objectToDestroy);

            // 리스트에서 해당 오브젝트 제거
            lifePoints.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// 라이프포인트의 변화 처리하기
    /// </summary>
    private void UpdateLifePoints()
    {
        for (int i = 0; i < lifePoints.Count; i++)
        {
            LifePointState hpScript = lifePoints[i].GetComponent<LifePointState>();
            if (i < lifePoints.Count - playerGameData.playerData.playerHP)
            {
                lifePoints[i].GetComponent<Image>().color = Color.grey;
                hpScript.isDisable = true;
            }
            else
            {
                lifePoints[i].GetComponent<Image>().color = Color.red; //이미지라면 White로 Red는 Test용.
                hpScript.isDisable = false;
            }
        }
    }
    
    void CursorCheckState()
    {
        if (charCon2D.playerAttack.isAiming)
        {
            if (!charCon2D.playerInteraction.isInteractiveCamera)
            {
                m_playerFollowCamera.gameObject.SetActive(false);
                m_cursorFollowCamera.gameObject.SetActive(true);
            }
            // 마우스 포인터 보이게 하기
            Cursor.visible = true;
            // 마우스 포인터 고정 해제
            Cursor.lockState = CursorLockMode.None;
            Vector2 hotSpot = new Vector2(charCon2D.playerAttack.weaponManager.aimingCursor.width / 2, charCon2D.playerAttack.weaponManager.aimingCursor.height / 2);
            Cursor.SetCursor(charCon2D.playerAttack.weaponManager.aimingCursor, hotSpot, CursorMode.Auto);
        }
        else
        {
            if (!charCon2D.playerInteraction.isInteractiveCamera)
            {
                m_playerFollowCamera.gameObject.SetActive(true);
                m_cursorFollowCamera.gameObject.SetActive(false);
            }
            // 마우스 포인터 숨기기
            Cursor.visible = false;
            // 마우스 포인터 화면 중앙에 고정
            Cursor.lockState = CursorLockMode.Locked;
            // 기본 마우스 포인터로 변경
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
