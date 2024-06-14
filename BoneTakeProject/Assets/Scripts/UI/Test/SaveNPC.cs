using System;
using System.Collections;
using System.Collections.Generic;
using CleverCrow.Fluid.Dialogues.Graphs;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class SaveNPC : InteractableObject
{
    public DialogueGraph[] firstSaveDialogue;
    public NamePopup namePopup;
    
    private DialoguePlayback playback;
    private string nowMapName;
    private Animator animator;
    private Collider2D collider2D;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        collider2D = GetComponent<Collider2D>();
    }

    private void Start()
    {
        nowMapName = SceneManager.GetActiveScene().name;
    }
    
    public void NpcInteraction(DialoguePlayback _dialoguePlayback)
    {
        if (talkEnable)
        {
            playback = _dialoguePlayback;
            _dialoguePlayback.gameObject.SetActive(true);
            if (string.IsNullOrEmpty(PlayerDataManager.instance.nowPlayer.playerName))
            {
                _dialoguePlayback.PlayDialogue(firstSaveDialogue[0]);
            }
            else
            {
                _dialoguePlayback.PlayDialogue(dialogue[Random.Range(0,3)]);
            }
        }
    }

    public void Save()
    {
        PlayerDataManager.instance.nowPlayer.mapName = nowMapName;
        PlayerDataManager.instance.SaveData();
        
        /*PopupManager popup = GameManager.Instance.GetPopupManager();
        popup.SetPopup("저장하시겠습니까?", false, () => 
            {
                PlayerDataManager.instance.nowPlayer.mapName = nowMapName;
                PlayerDataManager.instance.SaveData();
                popup.ClosePopup();
                popup.SetPopup("저장되었습니다.", true, () =>
                {
                    popup.gameObject.SetActive(false);
                },null);
            },
            () =>
            {
                Debug.Log("취소 버튼을 누름");
            });*/
    }

    public void MakeName()
    {
        namePopup.gameObject.SetActive(true);
        namePopup.confirmButton.onClick.AddListener(() =>
        {
            //인풋필드가 공백인지 확인하고 공백이 아니면 수행
            if (namePopup.NameMakeCheck())
            {
                //플레이어 데이터의 이름을 인풋박스 이름으로 변경
                PlayerDataManager.instance.nowPlayer.playerName = namePopup.inputField.text;
                
                //이름팝업 종료
                namePopup.gameObject.SetActive(false);
                
                //이름 완료 다이얼로그 재생
                playback.gameObject.SetActive(true);
                playback.PlayDialogue(firstSaveDialogue[1]);
            }
        });
    }

    public void Obj_ApplyDamage()
    {
        if (!animator.GetBool("IsDead"))
        {
            CharacterController2D.instance.playerAttack.weaponManager.weaponLife--;
            Destroy(collider2D);
            animator.SetBool("IsDead", true);
        }
    }
}