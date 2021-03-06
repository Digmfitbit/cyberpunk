﻿using UnityEngine;
using System.Collections;

public class Fighter : MonoBehaviour {

	protected float strength;
	protected float strengthMax;
	protected float stamina;
	protected float staminaMax;
	public float health;
	protected float healthMax;
	protected float recovery;
	protected float recoveryMax;
	protected float damage;
	protected float damageMax;

	protected float probabilityOfMissing = 20f;
	
	protected bool selected = false;
	public bool alive = true;
	protected bool isAttacking = false;

	public GameObject HealthBar;
	private GameObject healthBar;
	public GameObject StaminaBar;
	private GameObject staminaBar;

    // Fighter's stats

	//Components
	protected GameObject battleManager;
	protected BattleManager battleManagerScript;
	protected SpriteRenderer spriteRenderer;
	protected TextMesh textUnderFighter;
	protected Animator animationController;
	protected LineRenderer lineRenderer;
	protected Fighter enemy;

	protected Vector3 homePos;
	protected Vector3 newPos;
	protected float attackSpeed = 20f;

    public AudioSource attackSFX;
    public AudioSource hurtSFX;

	protected virtual void Awake () 
	{
		//Get components
		battleManager = GameObject.Find ("BattleManager");
		battleManagerScript = battleManager.GetComponent<BattleManager>();

		spriteRenderer = gameObject.GetComponent<SpriteRenderer> ();
		spriteRenderer.material.color = new Color (1f,1f,1f,0.5f);		//start sprite with half the opacity for testing

		animationController = gameObject.GetComponent<Animator> ();

		//initialize variables
		damage = 20;
		homePos = transform.position;

		//make health and stamina bar
		healthBar = Instantiate(HealthBar ,transform.position + new Vector3(0f, -0.1f, 0f) , Quaternion.identity) as GameObject;
		healthBar.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
		healthBar.transform.SetParent (transform);

        staminaBar = Instantiate(StaminaBar, transform.position + new Vector3(0f, -0.2f, 0f), Quaternion.identity) as GameObject;
		staminaBar.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
		staminaBar.transform.SetParent (transform);
	}


	protected virtual void Update ()
	{
        ShowSelection ();

		//Fight Code
		if (!isAttacking)
		{
			//Bring Fighter back to home positon
			newPos = Vector3.Slerp (transform.position, homePos, Time.deltaTime * attackSpeed);
			transform.position = newPos;
		}
	}

	public virtual void StartAttack (GameObject _enemy)
	{
		if (alive) 
		{
            if (_enemy.GetComponent<Fighter>())
                enemy = _enemy.GetComponent<Fighter>();
            else
                Debug.Log("enemy being set is not a Fighter");

			attack();
		}
	}


	public virtual void attack()
	{
        isAttacking = true;
        StartCoroutine(FighterAttack());
	}

    protected virtual IEnumerator FighterAttack()
    {
        if (attackSFX != null)
            attackSFX.Play();

        //Attacking Animation - Move the fighter to the opponent. If the fighter is close enough he hits him by calling the hit function on the opponent. 
        while (Vector3.Distance(transform.position, enemy.transform.position) > 0.2)
        {
            newPos = Vector3.Slerp(transform.position, enemy.transform.position, Time.deltaTime * attackSpeed);
            transform.position = newPos;
            yield return null;
        }

        isAttacking = false;

        //how high is the propability of a failed attack, call hit function on selected opponent
        if (probabilityOfMissing < Random.Range(0, 100))
        {
            enemy.SendMessage("Hit", damage);
        }
        else
        {
            Debug.Log("Shit I missed!!!");
        }
    }


	protected virtual void Hit(float _damageIn)
	{
        if (hurtSFX != null)
            hurtSFX.Play();

		health -= _damageIn;

		//if health is to low call dead function
		if (health <= 0) 
		{
			Dead ();
		}
		//display health under fighter
		healthBar.SendMessage ("UpdateStatusBar", new Vector2(health, healthMax));
	}

	
	protected virtual void Dead()
	{
		alive = false;

		battleManagerScript.someoneDied (gameObject);

		//trigger dead animation
		animationController.SetBool ("isDead", true);
	}


	public virtual void WinFight()
	{

	}


	protected virtual void OnMouseDown()
	{
		if(alive)
		{
			//Tell the GameManager that sombody click on you, snitch!
			battleManagerScript.setSelection (gameObject);
		}
	}


	public virtual void SetSelected(bool _selected)
	{
		selected = _selected;
	}


	protected virtual void ShowSelection()
	{
		if (selected) {	
			spriteRenderer.material.color = new Color (1f, 1f, 1f, 1f);
		}else {
            spriteRenderer.material.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
		}
	}
}
