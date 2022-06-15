using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class BossCtrl : MonoBehaviour
{
    public enum State
    {
        IDLE,
        TRACE,
        ATTACK,
        RUN_ATTACK,
        DIE
    }

    // ������ ���� ����
    public State state = State.IDLE;
    // ���� �����Ÿ�
    public float traceDist = 10.0f;
    //�� ���� �����Ÿ�
    public float longDist = 20.0f;
    // ���� �����Ÿ�
    public float attackDist = 2.0f;
    // ���� ��� ����
    public bool isDie = false;

    // ������Ʈ ĳ��
    private Transform monsterTransform;
    private Transform targetTransform;
    private NavMeshAgent agent;
    private Animator anim;

    // �ؽ� ���̺� �� ��������
    private readonly int hashTrace = Animator.StringToHash("IsTrace");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashLAttack = Animator.StringToHash("LAttack");
    private readonly int hashSDIndex = Animator.StringToHash("SDIndex");
    private readonly int hashDie = Animator.StringToHash("Die");

    // ������ ���� �ʱⰪ
    private readonly int iniHp = 100;
    private int currHp;

    private float timer = 0;
    [SerializeField]
    private float setTime = 2f;
    private bool isChange = false;
    private bool isAttack = false;
    public bool isRot = false;

    private Rigidbody rigidbody;

    [SerializeField]
    private ParticleSystem slashParticle;

    Quaternion TargetRot;
    private void Awake()
    {
        Debug.Log("^^");
        currHp = iniHp;
        monsterTransform = GetComponent<Transform>();
        targetTransform = GameObject.FindWithTag("PLAYER").GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();

        //�ڵ�ȸ�� ��� ��Ȱ��ȭ
        agent.updateRotation = false;

        anim = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        state = State.IDLE;

        currHp = iniHp;
        isDie = false;

        
        GetComponent<CapsuleCollider>().enabled = true;
        /*
        SphereCollider[] spheres = GetComponentsInChildren<SphereCollider>();
        foreach (SphereCollider sphere in spheres)
        {
            sphere.enabled = true;
        }
        */
        

        //������ ���¸� üũ�ϴ� �ڷ�ƾ
        StartCoroutine(CheckMonsterState());

        //���¿� ���� ���� �ൿ ���� �ڷ�ƾ
        StartCoroutine(MonsterAction());
    }


    private void Update()
    {

        if(isChange)
        {
            timer += Time.deltaTime;
            if(timer >= setTime)
            {
                isChange = false;
                isAttack = false;
                timer = 0;
            }
        }


        if(!isRot)
        {

            // ���������� ���� �Ÿ��� ȸ�� ���� �Ǵ�
            if (agent.remainingDistance >= 6.0f)
            {
                Vector3 l_vector = targetTransform.position - monsterTransform.position;

                // ȸ�� ���� ����
                Quaternion rotation = Quaternion.LookRotation(-l_vector);

                // ���� �������� �Լ��� �ε巯�� ȸ�� ó��
                monsterTransform.rotation = Quaternion.Slerp(monsterTransform.rotation, rotation, Time.deltaTime * 10.0f);
            }
        }
    }

    private void RotateToPlayer()
    {
        if (TargetRot == monsterTransform.rotation) return;
        Vector3 l_vector = targetTransform.position - monsterTransform.position;
        TargetRot = Quaternion.LookRotation(-l_vector).normalized;
        isRot = true;
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

            if (!isChange)
            {
                // ������ ĳ���� ������ �Ÿ� ����
                float distance = Vector3.Distance(monsterTransform.position, targetTransform.position);

                if (distance <= attackDist)
                {
                    state = State.ATTACK;
                }
                else if (distance <= traceDist)
                {
                    state = State.TRACE;
                }
                else if (distance <= longDist)
                {
                    state = State.RUN_ATTACK;
                }
                else
                {
                    state = State.IDLE;
                }
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
                    // ���� ����
                    agent.isStopped = true;
                    anim.SetBool(hashTrace, false);
                    //anim.SetBool(hashAttack, false);
                    break;
                case State.TRACE:
                    // ���� ��� ��ǥ�� �̵�
                    agent.SetDestination(targetTransform.position);
                    agent.speed = 10f;
                    agent.isStopped = false;
                    anim.SetBool(hashTrace, true);
                    //anim.SetBool(hashAttack, false);
                    break;
                case State.ATTACK:
                    if (!isAttack)
                    {
                        isAttack = true;
                        agent.isStopped = true;
                        isRot = true;

                        
                        monsterTransform.DORotate(targetTransform.rotation.eulerAngles, 2f).OnComplete(() =>
                        {
                            Debug.Log("�̻�������?");
                            isRot = false;
                            slashParticle.gameObject.SetActive(true);
                            anim.ResetTrigger(hashLAttack);
                            int index = Random.Range(0, 2);
                            anim.SetFloat(hashSDIndex, index);
                            anim.SetTrigger(hashAttack);
                        });
                    }
                    break;
                case State.RUN_ATTACK:
                    if(!isAttack)
                    {
                        isAttack = true;
                        agent.isStopped = true;
                        slashParticle.gameObject.SetActive(true);
                        anim.ResetTrigger(hashAttack);
                        monsterTransform.DOMove(targetTransform.position, 1.5f);
                        anim.SetTrigger(hashLAttack);
                    }
                    break;
                case State.DIE:
                    isDie = true;
                    agent.isStopped = true;
                    anim.SetTrigger(hashDie);
                    rigidbody.isKinematic = false;
                    break;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void Wait()
    {
        Debug.Log(transform.name);
        isChange = true;
        slashParticle.gameObject.SetActive(false);
        state = State.IDLE;
    }

    /*
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("BULLET") && currHp > 0)
        {
            // �Ѿ� ����
            Destroy(collision.gameObject);
            // �ǰ� �ִϸ��̼� ����
            anim.SetTrigger(hashHit);
            // �浹 ����
            Vector3 pos = collision.GetContact(0).point;
            // �Ѿ��� ���� ������ ���� ����
            Quaternion rot = Quaternion.LookRotation(-collision.GetContact(0).normal);
            // ���� ȿ�� ����
            ShowBloodEffect(pos, rot);

            // ���� HP ����
            currHp -= 10;
            if (currHp <= 0)
            {
                state = State.DIE;

                GameManager.GetInstance().DisPlayScore(50);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("PUNCH Collision");
    }
    
    private void ShowBloodEffect(Vector3 pos, Quaternion rot)
    {
        // ���� ȿ�� ����
        GameObject blood = Instantiate<GameObject>(bloodEffect, pos, rot, monsterTransform);
        Destroy(blood, 1.0f);
    }

    void OnPlayerDie()
    {
        state = State.PLAYERDIE;
    }
    */

    private void OnDrawGizmos()
    {
        
        // ���� �����Ÿ�
        if (state == State.TRACE)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(monsterTransform.position, traceDist);
        }
        
        // ���� �����Ÿ�
        if (state == State.ATTACK)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(monsterTransform.position, attackDist);
        }

        if(state == State.RUN_ATTACK)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(monsterTransform.position, longDist);
        }
    }

    /*
    public override void MonsterHit()
    {
        if (currHp > 0)
        {
            // �ǰ� �ִϸ��̼� ����
            anim.SetTrigger(hashHit);

            // ���� HP ����
            currHp -= 10;
            healthBarUI.ChangeHP(currHp, iniHp);
            if (currHp <= 0)
            {
                anim.SetBool(hashTrace, false);
                anim.SetBool(hashAttack, false);
                anim.SetBool(hashPatrol, false);
                state = State.DIE;
            }
        }
    }
    */
}
