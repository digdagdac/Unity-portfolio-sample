using UnityEngine;
using UnityEngine.InputSystem;

namespace Muloro.Portfolio
{
    [DisallowMultipleComponent]
    public sealed class PortfolioOfflinePlayer : MonoBehaviour
    {
        [SerializeField]
        private PortfolioOfflineBoss _boss;
        [SerializeField]
        private SpriteRenderer _spriteRenderer;
        [SerializeField]
        private float _moveSpeed = 6f;
        [SerializeField]
        private float _attackRange = 2.35f;
        [SerializeField]
        private float _attackDamage = 30f;
        [SerializeField]
        private float _attackCooldown = 0.35f;
        [SerializeField]
        private float _maxHealth = 100f;

        private Color _baseColor = Color.white;
        private float _health;
        private float _nextAttackTime;
        private float _flashUntil;

        public bool IsDead => _health <= 0f;

        public void SetBoss(PortfolioOfflineBoss boss)
        {
            _boss = boss;
        }

        public void TakeDamage(float damage)
        {
            if (IsDead)
            {
                return;
            }

            _health = Mathf.Max(0f, _health - damage);
            Flash(Color.red, 0.12f);

            if (IsDead && _spriteRenderer != null)
            {
                _spriteRenderer.color = new Color(0.35f, 0.35f, 0.35f, 0.9f);
            }
        }

        private void Awake()
        {
            _health = _maxHealth;
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (_spriteRenderer != null)
            {
                _baseColor = _spriteRenderer.color;
            }
        }

        private void Update()
        {
            if (IsDead)
            {
                return;
            }

            Vector2 movement = ReadMovement();
            Move(movement);
            FaceCombatDirection(movement);

            if (AttackPressed())
            {
                TryAttack();
            }

            RestoreFlashColor();
        }

        private static Vector2 ReadMovement()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            var movement = Vector2.zero;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                movement.x -= 1f;
            }
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                movement.x += 1f;
            }
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                movement.y -= 1f;
            }
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                movement.y += 1f;
            }

            return movement.sqrMagnitude > 1f ? movement.normalized : movement;
        }

        private static bool AttackPressed()
        {
            bool keyboardAttack = Keyboard.current?.spaceKey.wasPressedThisFrame == true;
            bool mouseAttack = Mouse.current?.leftButton.wasPressedThisFrame == true;
            return keyboardAttack || mouseAttack;
        }

        private void Move(Vector2 movement)
        {
            if (movement.sqrMagnitude <= 0f)
            {
                return;
            }

            transform.position += new Vector3(movement.x, 0f, movement.y) * (_moveSpeed * Time.deltaTime);
        }

        private void FaceCombatDirection(Vector2 movement)
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            if (Mathf.Abs(movement.x) > 0.01f)
            {
                _spriteRenderer.flipX = movement.x < 0f;
                return;
            }

            if (_boss != null && !_boss.IsDead)
            {
                _spriteRenderer.flipX = _boss.transform.position.x < transform.position.x;
            }
        }

        private void TryAttack()
        {
            if (Time.time < _nextAttackTime)
            {
                return;
            }

            _nextAttackTime = Time.time + _attackCooldown;
            Flash(Color.white, 0.07f);

            if (_boss == null || _boss.IsDead)
            {
                return;
            }

            float sqrDistance = (_boss.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance <= _attackRange * _attackRange)
            {
                _boss.TakeDamage(_attackDamage);
            }
        }

        private void Flash(Color color, float duration)
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            _spriteRenderer.color = color;
            _flashUntil = Time.time + duration;
        }

        private void RestoreFlashColor()
        {
            if (_spriteRenderer != null && _flashUntil > 0f && Time.time >= _flashUntil)
            {
                _spriteRenderer.color = _baseColor;
                _flashUntil = 0f;
            }
        }
    }
}
