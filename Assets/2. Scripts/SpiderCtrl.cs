using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderCtrl : MonoBehaviour
{

    public enum State
    {
        IDLE,
        PATROL,
        TRACE,
        ATTACK,
        DIE
    }

    // ������ ���� ����
    public State state = State.IDLE;
    // ���� �����Ÿ�
    public float traceDist = 10.0f;
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
    private readonly int hashPatrol = Animator.StringToHash("IsPatrol");
    private readonly int hashAttack = Animator.StringToHash("IsAttack");
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashDie = Animator.StringToHash("Die");

    // ������ ���� �ʱⰪ
    private readonly int iniHp = 100;
    private int currHp;

    // ����
    [SerializeField]
    private Transform[] waypoints;
    private int waypointIndex;
    private Vector3 target;

    // Score ����
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
    }

    private void OnEnable()
    {
        state = State.IDLE;

        currHp = iniHp;
        isDie = false;

        GetComponent<CapsuleCollider>().enabled = true;
        SphereCollider[] spheres = GetComponentsInChildren<SphereCollider>();
        foreach (SphereCollider sphere in spheres)
        {
            sphere.enabled = true;
        }

        //������ ���¸� üũ�ϴ� �ڷ�ƾ
        StartCoroutine(CheckMonsterState());

        //���¿� ���� ���� �ൿ ���� �ڷ�ƾ
        StartCoroutine(MonsterAction());
    }

    private void Update()
    {
        
        // ���������� ���� �Ÿ��� ȸ�� ���� �Ǵ�
        if (agent.remainingDistance >= 2.0f)
        {
            // ������Ʈ�� ȸ�� ��
            Vector3 direction = agent.desiredVelocity;

            // ȸ�� ���� ����
            Quaternion rotation = Quaternion.LookRotation(direction);

            // ���� �������� �Լ��� �ε巯�� ȸ�� ó��
            monsterTransform.rotation = Quaternion.Slerp(monsterTransform.rotation, rotation, Time.deltaTime * 10.0f);
        }
        
        if(Vector3.Distance(monsterTransform.position, target) < 1)
        {
            IterateWaypointIndex();
            target = waypoints[waypointIndex].position;
        }
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

            if (distance <= attackDist)
            {
                state = State.ATTACK;
            }
            else if (distance <= traceDist)
            {
                state = State.TRACE;
            }
            else
            {
                //state = State.IDLE;
                state = State.PATROL;
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
                    anim.SetBool(hashPatrol, false);
                    break;
                case State.PATROL:
                    agent.SetDestination(target);
                    agent.speed = 7f;
                    agent.isStopped = false;
                    anim.SetBool(hashPatrol, true);
                    anim.SetBool(hashTrace, false);
                    anim.SetBool(hashAttack, false);
                    break;
                case State.TRACE:
                    // ���� ��� ��ǥ�� �̵�
                    agent.SetDestination(targetTransform.position);
                    agent.speed = 10f;
                    agent.isStopped = false;
                    anim.SetBool(hashTrace, true);
                    anim.SetBool(hashAttack, false);
                    anim.SetBool(hashPatrol, false);
                    break;
                case State.ATTACK:
                    anim.SetBool(hashAttack, true);
                    break;
                case State.DIE:
                    isDie = true;
                    agent.isStopped = true;

                    anim.SetTrigger(hashDie);

                    //StopAllCoroutines();

                    GetComponent<CapsuleCollider>().enabled = false;
                    SphereCollider[] spheres = GetComponentsInChildren<SphereCollider>();
                    foreach (SphereCollider sphere in spheres)
                    {
                        sphere.enabled = false;
                    }
                    break;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    void IterateWaypointIndex()
    {
        waypointIndex++;
        if(waypointIndex == waypoints.Length)
        {
            waypointIndex = 0;
        }
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
    }
}
