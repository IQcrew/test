using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    System.Random rrr = new System.Random();
    [Header("KeyBinds")]
    [SerializeField] public KeyCode Right = KeyCode.RightArrow;
    [SerializeField] public KeyCode Left = KeyCode.LeftArrow;
    [SerializeField] public KeyCode Up = KeyCode.UpArrow;
    [SerializeField] public KeyCode Down = KeyCode.DownArrow;
    [SerializeField] public KeyCode hit = KeyCode.N;
    [SerializeField] public KeyCode fire = KeyCode.M;
    [SerializeField] public KeyCode slot = KeyCode.K;
    [Header("Health & Saturation")]
    [SerializeField] public string PlayerName;
    [SerializeField] public int MaxHealth = 200;
    [SerializeField] public int Saturation = 100;

    public bool PlayerRotationRight = true;
    private bool PlayerLastRotationRight;
    private float Health;
    // movement
    private bool GoRight;
    private bool GoLeft;
    private bool GoUp;
    private bool GoDown;
    public bool readyToTeleport = true;

    //jumping & falling
    private bool isGrounded;
    private float jumpTime = 0f;
    private bool isFalling = false;
    private float maxFallSpeed = 0f;
    private bool knockedOut = false;
    private float fallingTime;

    //crouching,ladder,onewayplatform variables
    private bool isCrouching = false;
    private bool isInThrow = false;
    private bool inRoll = false;
    private bool isOnLadder;
    public bool isInPlatform = false;
    private float lastExplosion = -10;

    // walking variables
    private float LastKeyRight = -5f;
    private float LastKeyLeft = -5f;

    //sprinting variables
    private bool sprinting = false;
    private float LastSprintRight = -5f;
    private float LastSprintLeft = -5f;
    [Header("LayerMasks")]
    [SerializeField] public LayerMask platformLayerMask;
    [SerializeField] public LayerMask oneWayPlatformLayerMask;
    [SerializeField] public LayerMask playerLayerMask;
    [Header("Shooting")]
    [SerializeField] public Transform FirePoint;
    public Gun PlayerGun;
    [SerializeField] public bool shooting = false;
    private double LastTimeShoot = -5f;
    private bool ReadyToFire = false;
    private int BulletsToShot = 0;
    private Quaternion TempQuaternion = Quaternion.Euler(0, 0, 0);
    private Laser MyLaser;
    private float lastHit = 0f;
    public bool granadePos = false;
    private bool iSlot = false;

    [Header("Fighting")]
    private float lastHitMelee = 0f;
    [SerializeField] private float kickForce;
    private bool kicked = false;
    private bool iHit = false;
    private bool hasHit = false;
    private MeleeWeapon PlayerWeapon;
    [System.NonSerialized] public int gunIndex = 0;
    [System.NonSerialized] public int weaponIndex = 0;
    [System.NonSerialized] public int granadeIndex = 0;
    private bool iFire;

    [Header("Components")]
    [SerializeField] private Rigidbody2D PlayerBody;
    [SerializeField] private BoxCollider2D PlayerHitBox;
    [SerializeField] private BoxCollider2D OpponentHitBox;
    [SerializeField] private Animator PlayerAnimator;
    [SerializeField] private Animator ShootingAnimator;
    [SerializeField] private SpriteRenderer PlayerRenderer;
    [SerializeField] private OneWayPlatform platformScript;
    [SerializeField] private GameObject LaserPoint;
    [SerializeField] private AudioSource PlayerAudio;
    [SerializeField] private Transform EmptyPoint;
    [SerializeField] private Transform BulletPoint;
    private FirePoint FP;
    private granadePack PlayerGranade;
    private bool readyToThrowGranade = false;
    private sceneManager SM;


    private float WalkForce = 5f;
    private float SprintForce = 8f;
    private float JumpForce = 10f;
    private float LadderVertical = 5f;
    private float LadderHorizontal = 3f;
    private float DoubleTapTime = 0.2f;
    private float TimeInRoll = 0.5f;
    private float timeBetweenRoll = 0.2f;
    private float ThrowJump = 5f;
    private float ThrowJumpGravity = 2f;
    private float NormalGravity = 2.5f;
    private float FallGravity = 4.5f;
    private float fallSpeed = 15f;
    private float fallAcceleration = 20f;
    private float fallDmg = 2f;
    private AudioClip Jump;
    private AudioClip Walk;
    private AudioClip Run;
    private AudioClip Reload;
    private AudioClip emptySound;
    private AudioClip getHit;
    private AudioClip deathSound;
    private SpriteRenderer PlayerSkin;

    void Start()
    {
        SM = GameObject.Find("LevelManager").GetComponent<sceneManager>();
        SM.appendPlayer(this.gameObject);
        FP = FirePoint.GetComponent<FirePoint>();
        playerTemplate PT = GameObject.Find("LevelManager").GetComponent<playerTemplate>();
        GunManager GM = GameObject.Find("LevelManager").GetComponent<GunManager>();
        PlayerWeapon = GM.AllMeleeWeapons[0];
        //setup keybinds
        if (gameObject.name == "Player_1"){
            Right = dataManager.settingsData.getKeyBind("P1 right");
            Left = dataManager.settingsData.getKeyBind("P1 left");
            Up = dataManager.settingsData.getKeyBind("P1 up");
            Down = dataManager.settingsData.getKeyBind("P1 down");
            hit = dataManager.settingsData.getKeyBind("P1 hit");
            fire = dataManager.settingsData.getKeyBind("P1 fire");
            slot = dataManager.settingsData.getKeyBind("P1 slot");
            AnimatorOverrideController[] controller = SM.getSkin(dataManager.gameData.SkinP1, gameObject.name);
            PlayerAnimator.runtimeAnimatorController = controller[0];
            ShootingAnimator.runtimeAnimatorController = controller[1];
            gameObject.transform.Find("ShootingTextureDown").GetComponent<SpriteRenderer>().sprite = SM.getTexture(dataManager.gameData.SkinP1, gameObject.name);
        }
        else{
            Right = dataManager.settingsData.getKeyBind("P2 right");
            Left = dataManager.settingsData.getKeyBind("P2 left");
            Up = dataManager.settingsData.getKeyBind("P2 up");
            Down = dataManager.settingsData.getKeyBind("P2 down");
            hit = dataManager.settingsData.getKeyBind("P2 hit");
            fire = dataManager.settingsData.getKeyBind("P2 fire");
            slot = dataManager.settingsData.getKeyBind("P2 slot");
            AnimatorOverrideController[] controller = SM.getSkin(dataManager.gameData.SkinP2, gameObject.name);
            PlayerAnimator.runtimeAnimatorController = controller[0];
            ShootingAnimator.runtimeAnimatorController = controller[1];
            gameObject.transform.Find("ShootingTextureDown").GetComponent<SpriteRenderer>().sprite = SM.getTexture(dataManager.gameData.SkinP2, gameObject.name);
        }
        //setup values 
        WalkForce = PT.WalkForce;
        SprintForce = PT.SprintForce;
        JumpForce = PT.JumpForce;
        LadderVertical = PT.LadderVertical;
        LadderHorizontal = PT.LadderHorizontal;
        DoubleTapTime = PT.DoubleTapTime;
        TimeInRoll = PT.TimeInRoll;
        ThrowJump = PT.ThrowJump;
        ThrowJumpGravity = PT.ThrowJumpGravity;
        NormalGravity = PT.NormalGravity;
        FallGravity = PT.FallGravity;
        fallSpeed = PT.fallSpeed;
        fallAcceleration = PT.fallAcceleration;
        fallDmg = PT.fallDmg;
        Jump = PT.Jump;
        Walk = PT.Walk;
        Run = PT.Run;
        Reload = PT.Reload;
        emptySound = PT.emptySound;
        deathSound = PT.deathSound;
        getHit = PT.getHit;
        PlayerSkin = this.gameObject.GetComponent<SpriteRenderer>();
        Physics2D.IgnoreCollision(PlayerHitBox, OpponentHitBox);
        MyLaser = LaserPoint.GetComponent<Laser>();
        Health = MaxHealth;
        PlayerLastRotationRight = PlayerRotationRight;
        if (!PlayerRotationRight) { transform.Rotate(0f, 180f, 0F); }
        PlayerGun = GetGun("Pistol");
        PlayerGranade = GetGranade("Explosive");
        MyLaser.ShootLaser(false);
        FP.exitFP();
    }

    void Update()
    {
        //SM.SkinUpdate(PlayerName, PlayerSkin.sprite, gameObject.transform.rotation);
        if (SM.Paused) { return; }
        isGrounded = GroundCheck();
        iFire = Input.GetKey(fire); // bool variables (to save power)
        iHit = Input.GetKey(hit);
        iSlot = Input.GetKey(slot);
        if (Input.GetKey(Right) && !Input.GetKey(Left)) { GoRight = true; GoLeft = false; } // Setting inputs - Left & Right ...
        else if ((!Input.GetKey(Right)) && Input.GetKey(Left)) { GoRight = false; GoLeft = true; }
        else { GoRight = false; GoLeft = false; }
        if (Input.GetKey(Up) && !Input.GetKey(Down)) { GoUp = true; GoDown = false; } // ... Up & Down ...
        else if ((!Input.GetKey(Up)) && Input.GetKey(Down)) { GoUp = false; GoDown = true; }
        else { GoUp = false; GoDown = false; }
        if (isGrounded) { kicked = false; } // reset kick premennej
        if (Time.time - lastHitMelee < PlayerWeapon.hitSpeed) { return; }
        if (knockedOut) { PlayerBody.velocity = Vector2.zero; PlayerAudio.clip = null; return; } // Stun
        if (isCrouching) { crouch(); PlayerAudio.clip = null; } // Crouch continue
        else if (PlayerBody.velocity.y <= -fallSpeed || isFalling) { fall(); } // Fall
        else if (isOnLadder && (!isGrounded)) { Ladder(); PlayerAudio.clip = null; } // Ladder
        else if (iHit) { meleeAttack(); } // Melee
        else if (granadePos) { GranadePosition(); } 
        else if (shooting) { ShootPosition(); } // Shooting
        else
        {
            BulletsToShot = 0;
            if (GoDown && isGrounded) { crouch(); PlayerAudio.clip = null; } // Crouch first
            else // movement
            {
                if (isGrounded && PlayerGun.name != "None" && iFire){
                    PlayerAudio.clip = null;
                    shooting = true; LastTimeShoot = Time.time;
                    PlayerRenderer.enabled = false;
                    PlayerBody.velocity = new Vector2(0, PlayerBody.velocity.y);
                }
                else if (isGrounded && iSlot){// && PlayerGranade.name != "None"
                    granadePos = true; LastTimeShoot = Time.time;
                    PlayerAudio.clip = null; PlayerRenderer.enabled = false;
                    PlayerBody.velocity = new Vector2(0, PlayerBody.velocity.y);
                }
                else { jump(); walk(); }
                if (PlayerBody.velocity.y < 0) { PlayerBody.gravityScale = FallGravity; }
                else { PlayerBody.gravityScale = NormalGravity; }
            }
            //Fliping Player
            if (PlayerRotationRight && !PlayerLastRotationRight) { transform.Rotate(0f, 180f, 0F); }
            else if (!PlayerRotationRight && PlayerLastRotationRight) { transform.Rotate(0f, 180f, 0F); }
            PlayerLastRotationRight = PlayerRotationRight;
        }
        if (Health < MaxHealth && Time.time - lastHit > 5) {
            Health += 20f * Time.deltaTime;
            if (Health > MaxHealth) { Health = MaxHealth; }
        }
        AnimationSetter(); // AnimationSetter
    }
    private void fall()
    {
        if (PlayerBody.velocity.y < maxFallSpeed) { maxFallSpeed = PlayerBody.velocity.y; }
        if (!isFalling) { isFalling = true; fallingTime = Time.time; HitBoxChanger(0.5f, 0.5f, 0f, -0.575f, false); }
        else if (isGrounded && !(lastExplosion + 0.1 > Time.time))
        {
            isFalling = false;
            TakeDamage((int)(fallDmg * Math.Abs(maxFallSpeed - (Time.time - fallingTime) * fallAcceleration)));
            PlayerBody.velocity = Vector2.zero;
            StartCoroutine(knockedOff());
        }
    }
    public void giveExplosion(int dmg)
    {
        TakeDamage(dmg);
        lastExplosion = Time.time;
        isFalling = true;
        fallingTime = Time.time;
    }
    private IEnumerator knockedOff()
    {
        knockedOut = true;
        HitBoxChanger(1f, 1f, 0f, 0f, false);
        yield return new WaitForSeconds(1f);
        knockedOut = false;
        HitBoxChanger(1.2f, 2.2f, 0f, -0.075f, false);
        maxFallSpeed = 0f;
    }
    private void meleeAttack()
    {
        if (isGrounded)
        {
            PlayerBody.velocity = Vector2.zero;
            PlayerAudio.clip = null;
            RaycastHit2D[] rayCastHits;
            if (PlayerRotationRight) { rayCastHits = Physics2D.RaycastAll(new Vector2(PlayerHitBox.bounds.center.x + PlayerHitBox.bounds.extents.x, PlayerHitBox.bounds.center.y), Vector2.right, PlayerWeapon.range, playerLayerMask); }
            else { rayCastHits = Physics2D.RaycastAll(new Vector2(PlayerHitBox.bounds.center.x - PlayerHitBox.bounds.extents.x, PlayerHitBox.bounds.center.y), Vector2.left, PlayerWeapon.range, playerLayerMask); }
            PlayerAnimator.SetBool("isFighting", true);
            foreach (RaycastHit2D raycast in rayCastHits)
                if (raycast.collider != PlayerHitBox)
                {
                    raycast.collider.GetComponent<Player>().TakeDamage(PlayerWeapon.damage);
                    lastHitMelee = Time.time;
                }

        }
        else
        {
            RaycastHit2D[] rayCastHits;
            if (PlayerRotationRight) { rayCastHits = Physics2D.RaycastAll(new Vector2(PlayerHitBox.bounds.center.x + PlayerHitBox.bounds.extents.x, PlayerHitBox.bounds.center.y - PlayerHitBox.bounds.extents.y), Vector2.right, PlayerHitBox.bounds.size.x, playerLayerMask); }
            else { rayCastHits = Physics2D.RaycastAll(new Vector2(PlayerHitBox.bounds.center.x - PlayerHitBox.bounds.extents.x, PlayerHitBox.bounds.center.y - PlayerHitBox.bounds.extents.y), Vector2.left, PlayerHitBox.bounds.size.x, playerLayerMask); }
            if (!kicked)
            {
                PlayerAnimator.SetBool("isFighting", true);
                foreach (RaycastHit2D raycast in rayCastHits)
                    if (raycast.collider != PlayerHitBox)
                    {
                        raycast.collider.GetComponent<Player>().giveExplosion(20);
                        if (PlayerRotationRight) { raycast.collider.GetComponent<Rigidbody2D>().AddForce(new Vector2(kickForce, 0f)); }
                        else { raycast.collider.GetComponent<Rigidbody2D>().AddForce(new Vector2(-kickForce, 0f)); }
                        kicked = true;
                    }
            }
        }
    }
    private void walk()
    {
        if (sprinting)
        {
            sprint();
            PlayerAudio.clip = Run;
            if (!PlayerAudio.isPlaying) { PlayerAudio.Play(); }
            return;
        }
        if (PlayerAudio.clip == Run) { PlayerAudio.clip = null; }
        PlayerAudio.clip = Walk;
        if (GoRight) //walk right
        {   //sprint check
            if (Input.GetKeyDown(Right) && Time.time - LastKeyRight < DoubleTapTime) { sprinting = true; }
            LastKeyRight = Time.time; PlayerRotationRight = true;
            PlayerBody.velocity = new Vector2(+WalkForce, PlayerBody.velocity.y);
            if (!PlayerAudio.isPlaying) { PlayerAudio.Play(); }
        }
        else if (GoLeft)     // walk left
        {   //sprint check
            if (Input.GetKeyDown(Left) && Time.time - LastKeyLeft < DoubleTapTime) { sprinting = true; }
            LastKeyLeft = Time.time; PlayerRotationRight = false;
            PlayerBody.velocity = new Vector2(-WalkForce, PlayerBody.velocity.y);
            if (!PlayerAudio.isPlaying) { PlayerAudio.Play(); }
        }
        else { PlayerBody.velocity = new Vector2(0, PlayerBody.velocity.y); PlayerAudio.clip = null; }   //stay at position
    }
    private void sprint()
    {
        if (GoRight) //sprint right
        {   //check double tap right to walk
            if (Input.GetKeyDown(Right) && Time.time - LastSprintRight < DoubleTapTime) { sprinting = false; }
            LastSprintRight = Time.time; PlayerRotationRight = true;
            PlayerBody.velocity = new Vector2(+SprintForce, PlayerBody.velocity.y);
        }
        else if (GoLeft)  //sprint left
        {   //check double tap left to walk
            if (Input.GetKeyDown(Left) && Time.time - LastSprintLeft < DoubleTapTime) { sprinting = false; }
            LastSprintLeft = Time.time; PlayerRotationRight = false;
            PlayerBody.velocity = new Vector2(-SprintForce, PlayerBody.velocity.y);
        }
        else
        {   //if you  stay more than "DoubleTapTime" it will remove sprint
            if (Time.time - LastSprintLeft > DoubleTapTime && Time.time - LastSprintRight > DoubleTapTime) { sprinting = false; }
            PlayerBody.velocity = new Vector2(0, PlayerBody.velocity.y);
        }
    }
    private void jump()
    {
        if (GoUp && isGrounded && (!isCrouching))
        {
            if (Time.time - jumpTime > 0.1f)
            {
                PlayerBody.velocity = new Vector2(PlayerBody.velocity.x, JumpForce);
                PlayerAudio.PlayOneShot(Jump);
                jumpTime = Time.time;
            }
        }
    }
    private void Ladder()
    {
        PlayerBody.gravityScale = 0f;
        // pohyb vpravo a vlavo
        if (GoRight)
        { PlayerBody.velocity = new Vector2(+LadderHorizontal, PlayerBody.velocity.y); PlayerRotationRight = true; }
        else if (GoLeft)
        { PlayerBody.velocity = new Vector2(-LadderHorizontal, PlayerBody.velocity.y); PlayerRotationRight = false; }
        else { PlayerBody.velocity = new Vector2(0f, PlayerBody.velocity.y); }
        // pohyb hore a dole
        if (GoUp)
        { PlayerBody.velocity = new Vector2(PlayerBody.velocity.x, +LadderVertical); }
        else if (GoDown)
        { PlayerBody.velocity = new Vector2(PlayerBody.velocity.x, -LadderVertical); }
        else { PlayerBody.velocity = new Vector2(PlayerBody.velocity.x, 0f); }
    }
    private void crouch()
    {

        if (Math.Abs(PlayerBody.velocity.x) <= WalkForce - 1f)
        {
            PlayerBody.velocity = Vector2.zero;
            if (inRoll)
            {
                inRoll = false;
                StopCoroutine(Roll());
                HitBoxChanger(1.2f, 2.2f, 0f, -0.075f, false);
            }
            else if (isInThrow)
            {
                isInThrow = false;
                PlayerBody.gravityScale = NormalGravity;
                HitBoxChanger(1.2f, 2.2f, 0f, -0.075f, false);
            }
            else if (!isCrouching) { HitBoxChanger(1.2f, 1.7f, 0f, -0.323f, true); }
            else if (!GoDown) { HitBoxChanger(1.2f, 2.2f, 0f, -0.075f, false); }
        }
        else if (Math.Abs(PlayerBody.velocity.x) <= WalkForce + 1f || inRoll) 
        {
            if (!inRoll)
                StartCoroutine(Roll());
            PlayerBody.velocity = PlayerRotationRight ? new Vector2(WalkForce, PlayerBody.velocity.y) : new Vector2(-WalkForce, PlayerBody.velocity.y);
        }
        else if (Math.Abs(PlayerBody.velocity.x) >= WalkForce+1f && (!inRoll))
        {
            if (!isCrouching)
            {
                isInThrow = true;
                HitBoxChanger(2f, 1f, 0f, 0f, true);
                PlayerBody.gravityScale = ThrowJumpGravity;
                PlayerBody.velocity = PlayerRotationRight ? new Vector2(SprintForce, ThrowJump) : new Vector2(-SprintForce, ThrowJump);
                PlayerAudio.PlayOneShot(Jump);
            }
            else if (isGrounded)
            {
                isInThrow = false;
                PlayerBody.velocity = PlayerRotationRight ? new Vector2(WalkForce, PlayerBody.velocity.y) : new Vector2(-WalkForce, PlayerBody.velocity.y);
                sprinting = false;
                PlayerBody.gravityScale = NormalGravity;
                StartCoroutine(Roll());
            }
        }
    }
    private IEnumerator Roll()
    {
        inRoll = true;
        HitBoxChanger(1.2f, 1.2f, 0f, -0.575f, true);
        yield return new WaitForSeconds(TimeInRoll);
        HitBoxChanger(1.2f, 2.2f, 0f, -0.075f, false);
        yield return new WaitForSeconds(timeBetweenRoll);
        inRoll = false;
    }
    private void HitBoxChanger(float sizeX, float sizeY, float offsetX, float offsetY, bool isCrouching)
    {
        PlayerHitBox.size = new Vector2(sizeX, sizeY);
        PlayerHitBox.offset = new Vector2(offsetX, offsetY);
        this.isCrouching = isCrouching;
    }
    public float getHealt
    {
        get { return Health; }
        set { }
    }
    private void ShootPosition()
    {
        SM.updateAmmoGun(PlayerName, PlayerGun.ammo);
        PlayerBody.velocity = Vector2.zero;
        ShootingAnimator.SetBool("Shot", false);
        if (LastTimeShoot + 0.5 < Time.time || !iFire && (GoRight || GoLeft || iHit || iSlot))
        {
            shooting = false; LastTimeShoot = -5;
            MyLaser.ShootLaser(false); 
            PlayerRenderer.enabled = true; 
            FP.exitFP(); 
            return;
        }
        if (GoRight) { PlayerRotationRight = true; }
        else if (GoLeft) { PlayerRotationRight = false; }
        if (PlayerRotationRight && !PlayerLastRotationRight) { transform.Rotate(0f, 180f, 0F); }
        else if (!PlayerRotationRight && PlayerLastRotationRight) { transform.Rotate(0f, 180f, 0F); }
        PlayerLastRotationRight = PlayerRotationRight;
        FP.FUpdate();
        if (BulletsToShot > 0) {
            switch (PlayerGun.name)
            {
                case "Mac-10":
                    if (Time.time > LastTimeShoot + 0.08f) {
                        TempQuaternion = Quaternion.Euler(0, 0, (((float)rrr.NextDouble()) * 10) - 5);
                        shootingBullet(QuaternionDifference(TempQuaternion, BulletPoint.rotation));
                        BulletsToShot -= 1; PlayerGun.ammo -= 1; LastTimeShoot = Time.time; PlayerAudio.PlayOneShot(PlayerGun.Sound, PlayerGun.fireVolume);
                    }
                    break;
                case "AssalutRifle":
                    if (Time.time > LastTimeShoot + 0.1f) {

                        shootingBullet(BulletPoint.rotation);
                        BulletsToShot -= 1; PlayerGun.ammo -= 1; LastTimeShoot = Time.time; PlayerAudio.PlayOneShot(PlayerGun.Sound, PlayerGun.fireVolume);
                    }
                    break;
                case "MiniGun":
                    if (Time.time > LastTimeShoot + PlayerGun.FireSpeed)
                    {
                        LastTimeShoot = Time.time;
                        TempQuaternion = Quaternion.Euler(0, 0, (((float)rrr.NextDouble()) * 6) - 3);
                        shootingBullet(QuaternionDifference(TempQuaternion, BulletPoint.rotation));
                        BulletsToShot -= 1; PlayerGun.ammo -= 1; LastTimeShoot = Time.time; PlayerAudio.PlayOneShot(PlayerGun.Sound, PlayerGun.fireVolume);
                    }
                    break;
            }
            return;
        }
        if (PlayerGun.name is "SniperRifle") { MyLaser.ShootLaser(true); }
        else { MyLaser.ShootLaser(false); }
        if (iFire) { ReadyToFire = true; LastTimeShoot = Time.time; }
        else if (ReadyToFire && PlayerGun.name != "" && PlayerGun.name != "None")
        {
            ReadyToFire = false;
            if (PlayerGun.fire()) {
                ShootingAnimator.SetBool("Shot", true);
                switch (PlayerGun.name){
                    case "Shotgun":
                        for (int i = 0; i < 4; i++)
                        {
                            TempQuaternion = Quaternion.Euler(0, 0, (((float)rrr.NextDouble()) * 14) - 7);
                            GameObject TVP = Instantiate(PlayerGun.Bullet, BulletPoint.position, QuaternionDifference(TempQuaternion, BulletPoint.rotation));
                            TVP.GetComponent<bullet>().shooter_name = PlayerName;
                            TVP.GetComponent<bullet>().damage = PlayerGun.damage;
                            TVP.GetComponent<bullet>().speed = PlayerGun.speed;
                        }
                        Instantiate(PlayerGun.EmptyBullet, EmptyPoint.position, EmptyPoint.rotation);
                        PlayerGun.ammo -= 1;
                        PlayerAudio.PlayOneShot(PlayerGun.Sound, PlayerGun.fireVolume);
                        PlayerAudio.PlayOneShot(Reload, PlayerGun.fireVolume);
                        break;
                    case "Mac-10":
                    case "AssalutRifle":
                        BulletsToShot = PlayerGun.ammo >= PlayerGun.BulletsOnShoot ? PlayerGun.BulletsOnShoot : PlayerGun.ammo;
                        break;
                    case "MiniGun":
                        BulletsToShot = PlayerGun.ammo;
                        break;
                    case "RocketLauncher":
                        PlayerAudio.PlayOneShot(PlayerGun.Sound, PlayerGun.fireVolume);
                        PlayerGun.ammo -= 1;
                        GameObject RT = Instantiate(PlayerGun.Bullet, BulletPoint.position, BulletPoint.rotation);
                        RT.GetComponent<Rocket>().shooterName = PlayerName;
                        break;
                    default:
                        shootingBullet(BulletPoint.rotation);
                        PlayerAudio.PlayOneShot(PlayerGun.Sound, PlayerGun.fireVolume);
                        PlayerGun.ammo -= 1;
                        if (PlayerGun.name == "SniperRifle") { PlayerAudio.PlayOneShot(Reload, PlayerGun.fireVolume); }
                        break;
                }
            }
        }
        if (PlayerGun.ammo <= 0) { PlayerAudio.PlayOneShot(emptySound); PlayerGun = GetGun("None"); LastTimeShoot = -5f; }
    }
    private void GranadePosition()
    {
        ShootingAnimator.SetBool("Thrown", false);
        if (iSlot) { LastTimeShoot = Time.time; readyToThrowGranade = true; }
        if (LastTimeShoot + 0.5 < Time.time || !iSlot && (GoRight || GoLeft || iHit || iFire))
        {
            granadePos = false; LastTimeShoot = -5f;
            PlayerRenderer.enabled = true;
            FP.exitFP();
            return;
        }
        if(readyToThrowGranade && !iSlot)
        {
            ShootingAnimator.SetBool("Thrown", true);
            readyToThrowGranade =false; PlayerGranade.coutInPack -= 1;
            GameObject tempG = Instantiate(PlayerGranade.granade, FirePoint.position, FirePoint.rotation);
            tempG.GetComponent<Granade>().setVelocity(20);
            if(PlayerGranade.coutInPack <= 0) { PlayerGranade = GetGranade("None"); }
        }
        FP.FUpdate();
    }
    private void shootingBullet(Quaternion rotation)
    {
        GameObject TVP = Instantiate(PlayerGun.Bullet, BulletPoint.position, rotation);
        TVP.GetComponent<bullet>().shooter_name = PlayerName;
        TVP.GetComponent<bullet>().damage = PlayerGun.damage;
        TVP.GetComponent<bullet>().speed = PlayerGun.speed;
        Instantiate(PlayerGun.EmptyBullet, EmptyPoint.position, EmptyPoint.rotation);

    }
    private bool GroundCheck()
    {
        if (isInPlatform) { return false; }
        else if (RayCast(PlayerHitBox.bounds.center, PlayerHitBox.bounds.extents.y - 0.2f, oneWayPlatformLayerMask, true))
        {
            platformScript.currentPlatform = null;
            StartCoroutine(IsInPlatform());
            return false;
        }
        else {
            if (RayCast(new Vector2(PlayerHitBox.bounds.center.x, PlayerHitBox.bounds.center.y - PlayerHitBox.bounds.extents.y), 0.1f, platformLayerMask, false)) { return true; }
            else { return false; } 
        }
    }
    private bool RayCast(Vector2 origin, float distance, LayerMask layerMask, bool oneWay)
    {
        RaycastHit2D[] rayCasts = new RaycastHit2D[] {
        Physics2D.Raycast(origin, Vector2.down, distance, layerMask),
        Physics2D.Raycast(new Vector2(origin.x + PlayerHitBox.bounds.extents.x, origin.y), Vector2.down, distance, layerMask),
        Physics2D.Raycast(new Vector2(origin.x - PlayerHitBox.bounds.extents.x, origin.y), Vector2.down, distance, layerMask) };
        if (oneWay) { return rayCasts[0].collider != null || rayCasts[1].collider != null || rayCasts[2].collider != null; }
        foreach (RaycastHit2D item in rayCasts)
            if (item.collider != null)
                if (item.collider.gameObject.layer == LayerMask.NameToLayer("OneWayPlatform")) { platformScript.currentPlatform = item.collider.gameObject; return true; }
                else { platformScript.currentPlatform = null; }
        return rayCasts[0].collider != null || rayCasts[1].collider != null || rayCasts[2].collider != null;
    }
    private IEnumerator IsInPlatform()
    {
        isInPlatform = true;
        yield return new WaitForSeconds(0.4f);
        isInPlatform = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            if (!isCrouching) { PlayerBody.gravityScale = 0f; }
            isOnLadder = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            if (!isCrouching) { PlayerBody.gravityScale = 2.5f; }
            isOnLadder = false;
        }
    }
    public void TakeDamage(int damage)
    {
        lastHit = Time.time;
        Health -= damage;
        if (Health <= 0f) { death(); }
        PlayerAudio.PlayOneShot(getHit);
    }
    private void AnimationSetter()
    {
        if ((isGrounded && GoUp) || !isGrounded || isInPlatform) { PlayerAnimator.SetBool("isGrounded", false); }
        else { PlayerAnimator.SetBool("isGrounded", true); }
        if (PlayerBody.velocity != Vector2.zero) { PlayerAnimator.SetBool("isLadderMoving", true); }
        else { PlayerAnimator.SetBool("isLadderMoving", false); }
        if (!iHit) { PlayerAnimator.SetBool("isFighting", false); }
        PlayerAnimator.SetBool("isCrouching", isCrouching);
        PlayerAnimator.SetBool("isOnLadder", isOnLadder);
        PlayerAnimator.SetFloat("PlayerSpeed", Math.Abs(PlayerBody.velocity.x));
        PlayerAnimator.SetInteger("WeaponID", weaponIndex);
        ShootingAnimator.SetInteger("GunID", gunIndex);
        ShootingAnimator.SetBool("isThrowing", granadePos);
        ShootingAnimator.SetBool("isShooting", shooting);
        ShootingAnimator.SetInteger("GranadeID", granadeIndex);
    }
    private Quaternion QuaternionDifference(Quaternion origin, Quaternion target)
    {
        Quaternion identityOrigin = Quaternion.identity * Quaternion.Inverse(origin);
        Quaternion identityTarget = Quaternion.identity * Quaternion.Inverse(target);
        return identityOrigin * Quaternion.Inverse(identityTarget);
    }
    private Gun GetGun(string name)
    {
        GunManager GunM = GameObject.Find("LevelManager").GetComponent<GunManager>();
        for (int i = 0; i < GunM.AllGuns.Count; i++){if (name == GunM.AllGuns[i].name) { gunIndex = i; SM.setTexture(PlayerName, "Gun", GunM.AllGuns[i].weaponTexture);
        BulletPoint.position = GunM.AllGuns[i].offSet + FirePoint.position; SM.updateAmmoGun(PlayerName, GunM.AllGuns[i].ammo); return GunM.AllGuns[i].Clone(); }}
        gunIndex = 0; BulletPoint.position = GunM.AllGuns[0].offSet+FirePoint.position; SM.updateAmmoGun(PlayerName, GunM.AllGuns[0].ammo); return GunM.AllGuns[0];
    }
    private granadePack GetGranade(string name)
    {
        GunManager GunM = GameObject.Find("LevelManager").GetComponent<GunManager>();
        for(int i = 0; i < GunM.AllGranades.Count; i++){if (name == GunM.AllGranades[i].name) { return GunM.AllGranades[i].Clone(); }}
        return GunM.AllGranades[0];
    }
    private MeleeWeapon GetMelee(string name)
    {
        GunManager GunM = GameObject.Find("LevelManager").GetComponent<GunManager>();
        for (int i = 0; i < GunM.AllMeleeWeapons.Count; i++) { if (name == GunM.AllMeleeWeapons[i].name) { weaponIndex = i; return GunM.AllMeleeWeapons[i].Clone(); } }
        weaponIndex = 0; return GunM.AllMeleeWeapons[0];
    }
    public bool PickUpWeapon(string WeaponName, string type)
    {
        switch (type)
        {
            case "Gun":
                if (WeaponName == "MedicKit") { Health = MaxHealth; return true; }
                if (PlayerGun.name == "None") { PlayerGun = GetGun(WeaponName); return true; }
                else if (isCrouching && iHit) { PlayerGun = GetGun(WeaponName); return true; }
                return false;
            case "Melee":
                if (PlayerWeapon.name == "Hand") { PlayerWeapon = GetMelee(WeaponName); return true; }
                else if (isCrouching && iHit) { PlayerWeapon = GetMelee(WeaponName); return true; }
                return false;
            case "Granade":
                if (PlayerGranade.name == "None") { PlayerGranade = GetGranade(WeaponName); return true; }
                else if (isCrouching && iHit) { PlayerGranade = GetGranade(WeaponName); return true; }
                return false;
            default:
                return false;
        }
    }
    private void death()
    {
        GameObject.Find("LevelManager").GetComponent<AudioSource>().PlayOneShot(deathSound);
        SM.PlayerDeath(this.gameObject);
        Destroy(gameObject);
    }
    public void startPlayer()
    {
        GameObject.Find("LevelManager").GetComponent<sceneManager>().appendPlayer(gameObject);
    }
}
