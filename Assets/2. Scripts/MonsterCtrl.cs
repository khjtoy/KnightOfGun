//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DefineCS;
using UnityEngine.AI;
using UnityEngine.UI;

public class MonsterCtrl : Monster
{
    //���� ��������

    public enum State
    {
        IDLE,
        ATTACK,
        SLAM,
        DIE,
    }

    [SerializeField]
    private Transform firePos;
    // ������ ���� ����
    public State state = State.IDLE;
    // ���Ÿ� ���� �����Ÿ�
    public float longDistnaceAttackDist = 10.0f;
    // �ٰŸ� ���� �����Ÿ�
    public float attackDist = 2.0f;
    // ���� ��� ����
    public bool isDie = false;

    // ������Ʈ ĳ��
    private Transform monsterTransform;
    private Transform targetTransform;
    private Animator anim;

    // �ؽ� ���̺� �� ��������
    private readonly int hashAttack = Animator.StringToHash("IsAttack");
    private readonly int hashShoot = Animator.StringToHash("IsShoot");
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashBDie = Animator.StringToHash("IsDie");
    private readonly int hashDie = Animator.StringToHash("Die");
    // ���� ȿ�� ������
    private GameObject bloodEffect;

    [SerializeField]
    private GameObject thorn;
    [SerializeField]
    private HealthBarUI healthBarUI;
    [SerializeField]
    private OpaqueItem opaqueItem;

    // ������ ���� �ʱⰪ
    private readonly int iniHp = 100;
    private int currHp;

    private Rigidbody rigidbody;

    private void Awake()
    {
        Debug.Log("zz");
        currHp = iniHp;
        monsterTransform = GetComponent<Transform>();
        targetTransform = GameObject.FindWithTag("PLAYER").GetComponent<Transform>();

        anim = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        state = State.IDLE;

        currHp = iniHp;
        isDie = false;

        GetComponent<CapsuleCollider>().enabled = true;

        //������ ���¸� üũ�ϴ� �ڷ�ƾ
        StartCoroutine(CheckMonsterState());

        //���¿� ���� ���� �ൿ ���� �ڷ�ƾ
        StartCoroutine(MonsterAction());
    }

    private void Update()
    {
        if(state != State.DIE)
            transform.LookAt(targetTransform);
    }

    private IEnumerator CheckMonsterState()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.3f);


            // ���� ���� ���½� �ڷ�ƾ ����
            if (state == State.DIE)
            {
                yield break;
            }

            // ������ ĳ���� ������ �Ÿ� ����
            float distance = Vector3.Distance(monsterTransform.position, targetTransform.position);


            if(opaqueItem.isOpaque)
            {
                state = State.IDLE;
            }
            else if (distance <= attackDist)
            {
                state = State.SLAM;
            }
            else if (distance <= longDistnaceAttackDist)
            {
                state = State.ATTACK;
            }
            else
            {
                state = State.IDLE;
            }
        }
    }
    private IEnumerator MonsterAction()
    {
        while (!isDie)
        {
            switch (state)
            {
                case State.IDLE:
                    anim.SetBool(hashAttack, false);
                    anim.SetBool(hashShoot, false);
                    break;
                case State.SLAM:
                    anim.SetBool(hashAttack, true);
                    anim.SetBool(hashShoot, false);
                    break;
                case State.ATTACK:
                    anim.SetBool(hashAttack, false);
                    anim.SetBool(hashShoot, true);
                    break;
                case State.DIE:
                    isDie = true;
                    //GetComponent<CapsuleCollider>().enabled = false;
                    healthBarUI.gameObject.SetActive(false);
                    anim.SetBool(hashBDie,isDie);
                    anim.SetTrigger(hashDie);
                    rigidbody.isKinematic = false;
                    break;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void ShootThorn()
    {
        Debug.Log("Shoot");

        Vector3 moveDir = (targetTransform.position - firePos.position).normalized;

        Quaternion quaternion = Quaternion.LookRotation(moveDir,Vector3.up);

        GameObject thorn = ObjectPoolMgr.Instance.GetPooledObject((int)PooledIndex.THORNS);
        thorn.transform.position = firePos.transform.position;
        thorn.transform.rotation = Quaternion.Euler(0f, quaternion.eulerAngles.y, 0f);
        thorn.SetActive(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        /*
        if(collision.collider.CompareTag("BULLET"))
        {
            ObjectPoolMgr.Instance.Despawn(collision.gameObject);
            anim.SetTrigger(hashHit);
            currHp -= 10;
            healthBarUI.ChangeHP(currHp, iniHp);

            if(currHp <= 0)
            {
                state = State.DIE;
            }
        }
        */
    }

    private void OnDrawGizmos()
    {
        // ���� �����Ÿ�
        if (state == State.SLAM)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(monsterTransform.position, attackDist);
        }

        
        // ���� �����Ÿ�
        if (state == State.ATTACK)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(monsterTransform.position, longDistnaceAttackDist);
        }
        
    }

    public override void MonsterHit()
    {
        anim.SetTrigger(hashHit);
        currHp -= 10;
        healthBarUI.ChangeHP(currHp, iniHp);

        if (currHp <= 0)
        {
            state = State.DIE;
        }
    }
}
