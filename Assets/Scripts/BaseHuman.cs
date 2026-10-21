using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseHuman : MonoBehaviour {
	// 是否移动
	protected bool isMoving = false;
	// 移动目标点
	private Vector3 targetPosition;
	// 移动速度
	public float speed = 1.2f;
	// 动画组件
	private Animator animator;
	// 描述
	public string desc = "";
	// 是否攻击
	internal bool isAttacking;
	internal float attackTime = float.MinValue;
	

	//攻击动作
	public void Attack()
	{
		isAttacking = true;
		attackTime = Time.time;
		animator.SetBool("isAttacking", true);
	}

	//攻击Update
	public void AttackUpdate()
	{
		if (!isAttacking) return;
		if (Time.time - attackTime < 1.2f) return;
		isAttacking = false;
		animator.SetBool("isAttacking", false);
	}

	//移动到某处
	public void MoveTo(Vector3 pos)
	{
		targetPosition = pos;
		isMoving = true;
		animator.SetBool("isMoving", true);
	}

	// 移动Update
	public void MoveUpdate()
	{
		// 判断是否可动
		if (isMoving == false)
		{
			return;
		}

		Vector3 pos = transform.position;
		// 发生位移
		transform.position = Vector3.MoveTowards(pos, targetPosition, speed * Time.deltaTime);
		// 改变朝向
		transform.LookAt(targetPosition);
		// 停止位移
		if (Vector3.Distance(pos, targetPosition) < 0.5f)
		{
			isMoving = false;
			animator.SetBool("isMoving", false);
		}
	}

	// Use this for initialization
	protected void Start()
	{
		animator = GetComponent<Animator>();
	}

	// Upadate is called once per frame
	protected void Update()
	{
		//移动
		MoveUpdate();

		//攻击
		AttackUpdate();
	}
}
