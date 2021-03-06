//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DefineCS;
using UnityEngine.AI;
using UnityEngine.UI;

public class MonsterCtrl : Monster
{
    //몬스터 상태정보

    public enum State
    {
        IDLE,
        ATTACK,
        SLAM,
        DIE,
    }

    [SerializeField]
    private Transform firePos;
    // 몬스터의 현재 상태
    public State state = State.IDLE;
    // 원거리 공격 사정거리
    public float longDistnaceAttackDist = 10.0f;
    // 근거리 공격 사정거리
    public float attackDist = 2.0f;
    // 몬스터 사망 여부
    public bool isDie = false;

    // 컴포넌트 캐싱
    private Transform monsterTransform;
    private Transform targetTransform;
    private Animator anim;

    // 해시 테이블 값 가져오기
    private readonly int hashAttack = Animator.StringToHash("IsAttack");
    private readonly int hashShoot = Animator.StringToHash("IsShoot");
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashBDie = Animator.StringToHash("IsDie");
    private readonly int hashDie = Animator.StringToHash("Die");
    // 혈흔 효과 프리팹
    private GameObject bloodEffect;

    [SerializeField]
    private GameObject thorn;
    [SerializeField]
    private HealthBarUI healthBarUI;
    [SerializeField]
    private OpaqueItem opaqueItem;

    // 몬스터의 생명 초기값
    private readonly int iniHp = 100;
    private int currHp;

    private Rigidbody rigidbody;
    SphereCollider[] spheres;

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
        spheres = GetComponentsInChildren<SphereCollider>();
        foreach (SphereCollider sphere in spheres)
        {
            sphere.enabled = false;
        }

        //몬스터의 상태를 체크하는 코루틴
        StartCoroutine(CheckMonsterState());

        //상태에 따라 몬스터 행동 수행 코루틴
        StartCoroutine(MonsterAction());
    }

    private void Update()
    {
        if(state != State.DIE && !opaqueItem.isOpaque)
            transform.LookAt(targetTransform);
    }

    private IEnumerator CheckMonsterState()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.3f);


            // 몬스터 죽음 상태시 코루틴 멈춤
            if (state == State.DIE)
            {
                yield break;
            }

            // 몬스터의 캐릭터 사이의 거리 측정
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
                    foreach (SphereCollider sphere in spheres)
                    {
                        sphere.enabled = false;
                    }
                    anim.SetBool(hashAttack, false);
                    anim.SetBool(hashShoot, false);
                    break;
                case State.SLAM:
                    foreach (SphereCollider sphere in spheres)
                    {
                        sphere.enabled = true;
                    }
                    anim.SetBool(hashAttack, true);
                    anim.SetBool(hashShoot, false);
                    break;
                case State.ATTACK:
                    foreach (SphereCollider sphere in spheres)
                    {
                        sphere.enabled = false;
                    }
                    anim.SetBool(hashAttack, false);
                    anim.SetBool(hashShoot, true);
                    break;
                case State.DIE:
                    foreach (SphereCollider sphere in spheres)
                    {
                        sphere.enabled = false;
                    }
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GUN"))
        {
            MonsterHit(monsterTransform.position, monsterTransform.rotation.eulerAngles, 40);
        }
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
        // 추적 사정거리
        if (state == State.SLAM)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(monsterTransform.position, attackDist);
        }

        
        // 공격 사정거리
        if (state == State.ATTACK)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(monsterTransform.position, longDistnaceAttackDist);
        }
        
    }

    public override void MonsterHit(Vector3 bloodPos, Vector3 bloodRot, int damage)
    {
        anim.SetTrigger(hashHit);
        currHp -= damage;
        healthBarUI.ChangeHP(currHp, iniHp);

        if (currHp <= 0)
        {
            state = State.DIE;


            GetComponent<CapsuleCollider>().enabled = false;

            int random = Random.Range(0, 2);

            if(random == 0)
            {
                GameObject bottle = ObjectPoolMgr.Instance.GetPooledObject((int)PooledIndex.WATER_BOTTLE);
                bottle.transform.position = transform.localPosition;
                bottle.SetActive(true);
            }
        }
    }
}
