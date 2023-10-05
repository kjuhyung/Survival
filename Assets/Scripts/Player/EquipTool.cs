using System;
using UnityEngine;

public class EquipTool : Equip
{
    public float attackRate;
    private bool attacking;
    public float attackDistance;

    [Header("# Resource Gathering")]
    public bool doseGatherResources;

    [Header("# Combat")]
    public bool doesDealDamage;
    public int damage;

    private Animator animator;
    private Camera camera;

    private void Awake()
    {
        camera = Camera.main;
        animator = GetComponent<Animator>();
    }

    public override void OnAttackInput()
    {
        if (!attacking)
        {
            attacking = true;
            animator.SetTrigger("Attack");
            Invoke(nameof(OnCanAttack), attackRate);
        }
    }

    void OnCanAttack()
    {
        attacking = false;
    }

    public void OnHit()
    {
        Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        // 화면 중앙에 Ray 를 쏜다        

        if(Physics.Raycast(ray, out hit, attackDistance)) 
        {
            // 맞은 것이 있다면 hit 에 할당

            if(doseGatherResources && hit.collider.TryGetComponent(out Resource resource))
            {
                resource.Gather(hit.point, hit.normal);
            }
            
            if(doesDealDamage && hit.collider.TryGetComponent(out IDamagable damageable))
            {
                damageable.TakePhysicalDamage(damage);
            }
        }
    }
}
