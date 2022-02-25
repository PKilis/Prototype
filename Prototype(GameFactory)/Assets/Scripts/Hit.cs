using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Hit : MonoBehaviour
{
    private Transform owner;
    private int damage;
    private Collider hitCollider;
    private Rigidbody rb;
    private Animator anim;

    private void Awake()
    {
        owner = transform.root;
        hitCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        hitCollider.isTrigger = true;
        rb.useGravity = false;
        rb.isKinematic = true;
        hitCollider.enabled = false;
    }
    void Start()
    {
        if (owner.tag == "Player")
        {
            damage = owner.GetComponent<AttackControl>().GetDamage();
            anim = GetComponentInParent<Transform>().GetComponentInParent<Animator>();
        }
        else if (owner.tag == "Enemy")
        {
            damage = owner.GetComponent<Enemy>().GetDamage();
            anim = GetComponentInParent<Animator>();
        }
        else
        {
            enabled = false;
        }
    }
    private void Update()
    {
        if (!anim.IsInTransition(0) && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") 
            && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= .5f
            && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < .55f)
        {
            ControlTheCollider(true);
        }
        else
        {
            ControlTheCollider(false);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Health health = other.GetComponent<Health>();
        if (health != null && health.gameObject != owner.gameObject)
        {
            health.GiveDamage(damage);
        }
    }
    private void ControlTheCollider(bool open)
    {
        hitCollider.enabled = open;
    }
}
