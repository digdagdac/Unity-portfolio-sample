using System;
using System.Collections;
using UnityEngine;

namespace Muloro.Portfolio
{
    [DisallowMultipleComponent]
    public sealed class PortfolioOfflineBoss : MonoBehaviour
    {
        private const string AnimatorBaseLayer = "Base Layer.";
        private const string IdleStateName = "idle";
        private const string WalkStateName = "walk";
        private const string HitStateName = "hit";
        private const string DeathStateName = "death";
        private const string GroggyStateName = "groggy_loop";
        private const string DashFrontStateName = "dash_front";
        private const string DashBackStateName = "dash_back";
        private const string PunchDashStateName = "punch_dash";
        private const string Punch4ToStarFingerStateName = "punch4_to_starfinger";
        private const string StarFingerStateName = "starfinger";
        private const string StarFingerToIdleStateName = "starfinger_to_idle";
        private const string WalkTriggerName = "WALK";
        private const string IdleTriggerName = "IDLE";

        private const int Phase2BasicAttackWeight = 40;
        private const int Phase2ComboAttackWeight = 60;
        private const int Phase3BasicAttackWeight = 10;
        private const int Phase3SmashComboWeight = 40;
        private const int Phase3PunchStarFingerWeight = 30;
        private const int Phase3PunchDashComboWeight = 20;
        private const float ForwardDashDistanceThreshold = 5f;
        private const float BackDashDistanceThreshold = 1f;
        private const float ForcedBackDashHealthPercent = 0.3f;
        private const float RandomDashChance = 0.7f;
        private const float StarFingerFollowUpDamageDelay = 0.15f;

        private static readonly string[] Phase1BasicAttacks =
        {
            "smash1",
            "punch4",
            StarFingerStateName
        };

        private static readonly string[] SmashCombo =
        {
            "smash1",
            "smash2",
            "smash3"
        };

        private static readonly string[] PunchStarFingerCombo =
        {
            "punch4",
            Punch4ToStarFingerStateName,
            StarFingerStateName,
            StarFingerToIdleStateName
        };

        private static readonly string[] PunchDashCombo =
        {
            PunchDashStateName,
            "punch1",
            "punch2",
            "punch3",
            "punch4"
        };

        [SerializeField]
        private Transform _target;
        [SerializeField]
        private PortfolioOfflinePlayer _player;
        [SerializeField]
        private SpriteRenderer _spriteRenderer;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private string[] _attackAnimationNames =
        {
            "punch1",
            "punch2",
            "punch3",
            "punch4",
            "smash1",
            "smash2",
            "smash3",
            "starfinger"
        };
        [SerializeField]
        private float _attackDamageDelay = 0.35f;
        [SerializeField]
        private float _attackAnimationDuration = 0.95f;
        [SerializeField]
        private float _maxHealth = 300f;
        [SerializeField]
        private float _moveSpeed = 2.2f;
        [SerializeField]
        private float _stopDistance = 2.2f;
        [SerializeField]
        private float _attackRange = 2.55f;
        [SerializeField]
        private float _attackDamage = 8f;
        [SerializeField]
        private float _attackCooldown = 1.15f;
        [SerializeField]
        private float _dashDistance = 3.2f;
        [SerializeField]
        private float _dashDuration = 0.35f;
        [SerializeField]
        private float _randomDashCooldown = 3f;
        [SerializeField]
        private float _forcedBackDashCooldown = 10f;
        [SerializeField]
        private float _phase2HealthPercent = 0.85f;
        [SerializeField]
        private float _phase3HealthPercent = 0.4f;
        [SerializeField]
        private float _groggyDamageThreshold = 90f;
        [SerializeField]
        private float _groggyDuration = 2f;

        private BossBehaviorTree _behaviorTree;
        private Color _baseColor = Color.white;
        private Coroutine _currentActionCoroutine;
        private float _health;
        private float _nextAttackTime;
        private float _nextRandomDashTime;
        private float _nextForcedBackDashTime;
        private float _flashUntil;
        private float _staggerDamage;
        private float _groggyUntil;
        private int _activePhase;
        private bool _isActing;

        public bool IsDead => _health <= 0f;

        public void SetTarget(Transform target)
        {
            _target = target;
            _player = target != null ? target.GetComponent<PortfolioOfflinePlayer>() : null;
        }

        public void PlaySound(string soundName)
        {
            // Source Belphegor clips keep sound AnimationEvents; this demo has no audio runtime.
        }

        public void TakeDamage(float damage)
        {
            if (IsDead)
            {
                return;
            }

            _health = Mathf.Max(0f, _health - damage);
            _staggerDamage += damage;
            Flash(Color.red, 0.1f);

            if (IsDead)
            {
                CancelCurrentAction();
                PlayAnimation(DeathStateName, force: true);
                return;
            }

            if (_staggerDamage >= _groggyDamageThreshold)
            {
                _staggerDamage = 0f;
                _groggyUntil = Time.time + _groggyDuration;
                CancelCurrentAction();
                PlayAnimation(GroggyStateName, force: true);
                return;
            }

            if (!_isActing)
            {
                PlayAnimation(HitStateName, force: true);
            }
        }

        private void Awake()
        {
            _health = _maxHealth;
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            if (_spriteRenderer != null)
            {
                _baseColor = _spriteRenderer.color;
            }

            if (_attackAnimationNames == null || _attackAnimationNames.Length == 0)
            {
                _attackAnimationNames = Phase1BasicAttacks;
            }

            _activePhase = GetCurrentPhase();
            _behaviorTree = BuildBehaviorTree();
            PlayAnimation(IdleStateName, force: true);
        }

        private void Update()
        {
            RestoreFlashColor();
            _behaviorTree?.Tick();
        }

        private BossBehaviorTree BuildBehaviorTree()
        {
            IBossBehaviorNode emergencySelector = new SelectorNode(
                new SequenceNode(
                    new ConditionNode(() => IsDead),
                    new ActionNode(ExecuteDeath)),
                new SequenceNode(
                    new ConditionNode(IsGroggy),
                    new ActionNode(ExecuteGroggy)),
                new SequenceNode(
                    new ConditionNode(ShouldChangePhase),
                    new ActionNode(ExecutePhaseTransition)));

            IBossBehaviorNode combatSelector = new SelectorNode(
                new ActionNode(TickActiveAction),
                new SequenceNode(
                    new ConditionNode(IsPhase3),
                    BuildPhase3Action()),
                new SequenceNode(
                    new ConditionNode(IsPhase2),
                    BuildPhase2Action()),
                new SequenceNode(
                    new ConditionNode(IsPhase1),
                    BuildPhase1Action()));

            return new BossBehaviorTree(
                new SelectorNode(
                    emergencySelector,
                    new SequenceNode(
                        new ConditionNode(HasTarget),
                        combatSelector),
                    new ActionNode(ExecuteIdle)));
        }

        private IBossBehaviorNode BuildPhase1Action()
        {
            return new SelectorNode(
                BuildDashSequence(),
                BuildAttackSequence(SelectPhase1Attack),
                new ActionNode(ExecuteMovement));
        }

        private IBossBehaviorNode BuildPhase2Action()
        {
            return new SelectorNode(
                BuildDashSequence(),
                BuildAttackSequence(SelectPhase2Attack),
                new ActionNode(ExecuteMovement));
        }

        private IBossBehaviorNode BuildPhase3Action()
        {
            return new SelectorNode(
                BuildDashSequence(),
                BuildAttackSequence(SelectPhase3Attack),
                new ActionNode(ExecuteMovement));
        }

        private IBossBehaviorNode BuildDashSequence()
        {
            return new SelectorNode(
                new SequenceNode(
                    new ConditionNode(IsForcedBackDashReady),
                    new ActionNode(() => StartDash(DashBackStateName, awayFromTarget: true, forced: true))),
                new SequenceNode(
                    new ConditionNode(IsForwardDashReady),
                    new ConditionNode(ConsumeRandomDashChance),
                    new ActionNode(() => StartDash(DashFrontStateName, awayFromTarget: false, forced: false))),
                new SequenceNode(
                    new ConditionNode(IsBackDashReady),
                    new ConditionNode(ConsumeRandomDashChance),
                    new ActionNode(() => StartDash(DashBackStateName, awayFromTarget: true, forced: false))));
        }

        private IBossBehaviorNode BuildAttackSequence(Func<string[]> attackSelector)
        {
            return new SequenceNode(
                new ConditionNode(IsAttackRange),
                new ConditionNode(IsAttackCooldownReady),
                new ActionNode(() => StartAttack(attackSelector())));
        }

        private bool HasTarget()
        {
            return _target != null && (_player == null || !_player.IsDead);
        }

        private bool IsGroggy()
        {
            return !IsDead && Time.time < _groggyUntil;
        }

        private bool ShouldChangePhase()
        {
            return !IsDead && GetCurrentPhase() != _activePhase;
        }

        private BossBehaviorStatus ExecutePhaseTransition()
        {
            _activePhase = GetCurrentPhase();
            PlayAnimation(HitStateName, force: true);
            return BossBehaviorStatus.Success;
        }

        private bool IsPhase1()
        {
            return GetHealthPercent() > _phase2HealthPercent;
        }

        private bool IsPhase2()
        {
            float healthPercent = GetHealthPercent();
            return healthPercent <= _phase2HealthPercent && healthPercent > _phase3HealthPercent;
        }

        private bool IsPhase3()
        {
            float healthPercent = GetHealthPercent();
            return healthPercent <= _phase3HealthPercent && healthPercent > 0f;
        }

        private int GetCurrentPhase()
        {
            if (IsPhase3())
            {
                return 3;
            }

            return IsPhase2() ? 2 : 1;
        }

        private float GetHealthPercent()
        {
            if (_maxHealth <= 0f)
            {
                return 0f;
            }

            return _health / _maxHealth;
        }

        private BossBehaviorStatus ExecuteDeath()
        {
            if (_isActing)
            {
                CancelCurrentAction();
            }

            PlayAnimation(DeathStateName);
            return BossBehaviorStatus.Running;
        }

        private BossBehaviorStatus ExecuteGroggy()
        {
            if (_isActing)
            {
                CancelCurrentAction();
            }

            PlayAnimation(GroggyStateName);
            return BossBehaviorStatus.Running;
        }

        private BossBehaviorStatus TickActiveAction()
        {
            if (!_isActing)
            {
                return BossBehaviorStatus.Failure;
            }

            FaceTarget(GetToTarget());
            return BossBehaviorStatus.Running;
        }

        private BossBehaviorStatus ExecuteIdle()
        {
            PlayAnimation(IdleStateName);
            return BossBehaviorStatus.Success;
        }

        private BossBehaviorStatus ExecuteMovement()
        {
            Vector3 toTarget = GetToTarget();
            FaceTarget(toTarget);
            bool isMoving = MoveTowardTarget(toTarget);
            PlayLocomotion(isMoving);
            return BossBehaviorStatus.Success;
        }

        private bool IsForwardDashReady()
        {
            return HasTarget()
                && Time.time >= _nextRandomDashTime
                && GetHorizontalDistanceToTarget() > ForwardDashDistanceThreshold;
        }

        private bool IsBackDashReady()
        {
            return HasTarget()
                && Time.time >= _nextRandomDashTime
                && GetHorizontalDistanceToTarget() < BackDashDistanceThreshold;
        }

        private bool IsForcedBackDashReady()
        {
            return HasTarget()
                && Time.time >= _nextForcedBackDashTime
                && GetHealthPercent() < ForcedBackDashHealthPercent;
        }

        private bool ConsumeRandomDashChance()
        {
            if (UnityEngine.Random.value <= RandomDashChance)
            {
                return true;
            }

            _nextRandomDashTime = Time.time + _randomDashCooldown;
            return false;
        }

        private BossBehaviorStatus StartDash(string animationName, bool awayFromTarget, bool forced)
        {
            if (_isActing)
            {
                return BossBehaviorStatus.Running;
            }

            if (forced)
            {
                _nextForcedBackDashTime = Time.time + _forcedBackDashCooldown;
            }
            else
            {
                _nextRandomDashTime = Time.time + _randomDashCooldown;
            }

            StartAction(DashRoutine(animationName, awayFromTarget));
            return BossBehaviorStatus.Running;
        }

        private bool IsAttackRange()
        {
            return HasTarget() && GetToTarget().sqrMagnitude <= _attackRange * _attackRange;
        }

        private bool IsAttackCooldownReady()
        {
            return Time.time >= _nextAttackTime;
        }

        private BossBehaviorStatus StartAttack(string[] animationNames)
        {
            if (_isActing)
            {
                return BossBehaviorStatus.Running;
            }

            _nextAttackTime = Time.time + _attackCooldown;
            StartAction(AttackPatternRoutine(animationNames));
            return BossBehaviorStatus.Running;
        }

        private string[] SelectPhase1Attack()
        {
            string attackName = Phase1BasicAttacks[UnityEngine.Random.Range(0, Phase1BasicAttacks.Length)];
            if (attackName == StarFingerStateName)
            {
                return new[] { StarFingerStateName, StarFingerToIdleStateName };
            }

            return new[] { attackName };
        }

        private string[] SelectPhase2Attack()
        {
            int roll = UnityEngine.Random.Range(0, Phase2BasicAttackWeight + Phase2ComboAttackWeight);
            if (roll < Phase2BasicAttackWeight)
            {
                return SelectPhase1Attack();
            }

            return SelectEqualComboAttack();
        }

        private string[] SelectPhase3Attack()
        {
            int roll = UnityEngine.Random.Range(
                0,
                Phase3BasicAttackWeight
                + Phase3SmashComboWeight
                + Phase3PunchStarFingerWeight
                + Phase3PunchDashComboWeight);

            if (roll < Phase3BasicAttackWeight)
            {
                return SelectPhase1Attack();
            }

            roll -= Phase3BasicAttackWeight;
            if (roll < Phase3SmashComboWeight)
            {
                return SmashCombo;
            }

            roll -= Phase3SmashComboWeight;
            return roll < Phase3PunchStarFingerWeight ? PunchStarFingerCombo : PunchDashCombo;
        }

        private string[] SelectEqualComboAttack()
        {
            int roll = UnityEngine.Random.Range(0, 3);
            if (roll == 0)
            {
                return SmashCombo;
            }

            return roll == 1 ? PunchStarFingerCombo : PunchDashCombo;
        }

        private void StartAction(IEnumerator actionRoutine)
        {
            CancelCurrentAction();
            _isActing = true;
            _currentActionCoroutine = StartCoroutine(RunAction(actionRoutine));
        }

        private IEnumerator RunAction(IEnumerator actionRoutine)
        {
            yield return actionRoutine;
            _isActing = false;
            _currentActionCoroutine = null;

            if (!IsDead && !IsGroggy())
            {
                PlayAnimation(IdleStateName, force: true);
            }
        }

        private void CancelCurrentAction()
        {
            if (_currentActionCoroutine != null)
            {
                StopCoroutine(_currentActionCoroutine);
                _currentActionCoroutine = null;
            }

            _isActing = false;
        }

        private IEnumerator DashRoutine(string animationName, bool awayFromTarget)
        {
            Vector3 toTarget = GetToTarget();
            Vector3 direction = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : transform.forward;
            if (awayFromTarget)
            {
                direction *= -1f;
            }

            FaceTarget(toTarget);
            ClearLocomotionTriggers();
            PlayAnimation(animationName, force: true);

            Vector3 start = transform.position;
            Vector3 end = start + direction * _dashDistance;
            float elapsed = 0f;
            while (elapsed < _dashDuration)
            {
                elapsed += Time.deltaTime;
                float ratio = _dashDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / _dashDuration);
                transform.position = Vector3.Lerp(start, end, ratio);
                yield return null;
            }
        }

        private IEnumerator AttackPatternRoutine(string[] animationNames)
        {
            if (animationNames == null || animationNames.Length == 0)
            {
                animationNames = Phase1BasicAttacks;
            }

            foreach (string animationName in animationNames)
            {
                ClearLocomotionTriggers();
                PlayAnimation(animationName, force: true);

                bool appliesDamage = ShouldApplyAttackDamage(animationName);
                float damageDelay = appliesDamage ? GetAttackDamageDelay(animationName) : 0f;
                if (appliesDamage)
                {
                    yield return new WaitForSeconds(damageDelay);
                    TryApplyAttackDamage();
                }

                float remainingDuration = appliesDamage
                    ? Mathf.Max(0f, _attackAnimationDuration - damageDelay)
                    : _attackAnimationDuration;
                if (remainingDuration > 0f)
                {
                    yield return new WaitForSeconds(remainingDuration);
                }
            }
        }

        private static bool ShouldApplyAttackDamage(string animationName)
        {
            return animationName != StarFingerStateName
                && animationName != Punch4ToStarFingerStateName;
        }

        private float GetAttackDamageDelay(string animationName)
        {
            return animationName == StarFingerToIdleStateName
                ? StarFingerFollowUpDamageDelay
                : _attackDamageDelay;
        }

        private void TryApplyAttackDamage()
        {
            if (IsDead || _player == null || _player.IsDead || _target == null)
            {
                return;
            }

            Vector3 toTarget = GetToTarget();
            if (toTarget.sqrMagnitude <= _attackRange * _attackRange)
            {
                _player.TakeDamage(_attackDamage);
            }
        }

        private void FaceTarget(Vector3 toTarget)
        {
            if (_spriteRenderer != null && Mathf.Abs(toTarget.x) > 0.01f)
            {
                _spriteRenderer.flipX = toTarget.x > 0f;
            }
        }

        private bool MoveTowardTarget(Vector3 toTarget)
        {
            if (toTarget.sqrMagnitude <= _stopDistance * _stopDistance)
            {
                return false;
            }

            transform.position += toTarget.normalized * (_moveSpeed * Time.deltaTime);
            return true;
        }

        private float GetHorizontalDistanceToTarget()
        {
            return GetToTarget().magnitude;
        }

        private Vector3 GetToTarget()
        {
            if (_target == null)
            {
                return Vector3.zero;
            }

            Vector3 toTarget = _target.position - transform.position;
            toTarget.y = 0f;
            return toTarget;
        }

        private void PlayAnimation(string stateName, float transitionDuration = 0.05f, bool force = false)
        {
            if (_animator == null || _animator.runtimeAnimatorController == null || string.IsNullOrWhiteSpace(stateName))
            {
                return;
            }

            int shortStateHash = Animator.StringToHash(stateName);
            int fullPathHash = Animator.StringToHash(AnimatorBaseLayer + stateName);
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
            if (!force && (currentState.shortNameHash == shortStateHash || currentState.fullPathHash == fullPathHash))
            {
                return;
            }

            int stateHash = _animator.HasState(0, fullPathHash) ? fullPathHash : shortStateHash;
            if (!_animator.HasState(0, stateHash))
            {
                return;
            }

            _animator.CrossFadeInFixedTime(stateHash, transitionDuration, 0);
        }

        private void PlayLocomotion(bool isMoving)
        {
            string stateName = isMoving ? WalkStateName : IdleStateName;
            string triggerName = isMoving ? WalkTriggerName : IdleTriggerName;

            if (!TrySetAnimatorTrigger(triggerName, stateName))
            {
                PlayAnimation(stateName);
            }
        }

        private bool TrySetAnimatorTrigger(string triggerName, string stateName)
        {
            if (_animator == null || _animator.runtimeAnimatorController == null || !HasAnimatorTrigger(triggerName))
            {
                return false;
            }

            int shortStateHash = Animator.StringToHash(stateName);
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
            if (currentState.shortNameHash == shortStateHash)
            {
                return true;
            }

            if (_animator.IsInTransition(0))
            {
                AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(0);
                if (nextState.shortNameHash == shortStateHash)
                {
                    return true;
                }
            }

            string otherTriggerName = triggerName == WalkTriggerName ? IdleTriggerName : WalkTriggerName;
            if (HasAnimatorTrigger(otherTriggerName))
            {
                _animator.ResetTrigger(otherTriggerName);
            }

            _animator.SetTrigger(triggerName);
            return true;
        }

        private void ClearLocomotionTriggers()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null)
            {
                return;
            }

            if (HasAnimatorTrigger(WalkTriggerName))
            {
                _animator.ResetTrigger(WalkTriggerName);
            }

            if (HasAnimatorTrigger(IdleTriggerName))
            {
                _animator.ResetTrigger(IdleTriggerName);
            }
        }

        private bool HasAnimatorTrigger(string triggerName)
        {
            if (_animator == null)
            {
                return false;
            }

            foreach (AnimatorControllerParameter parameter in _animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
                {
                    return true;
                }
            }

            return false;
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

        private enum BossBehaviorStatus
        {
            Success,
            Failure,
            Running
        }

        private interface IBossBehaviorNode
        {
            BossBehaviorStatus Tick();
        }

        private sealed class BossBehaviorTree
        {
            private readonly IBossBehaviorNode _root;

            public BossBehaviorTree(IBossBehaviorNode root)
            {
                _root = root;
            }

            public BossBehaviorStatus Tick()
            {
                return _root.Tick();
            }
        }

        private sealed class SelectorNode : IBossBehaviorNode
        {
            private readonly IBossBehaviorNode[] _children;

            public SelectorNode(params IBossBehaviorNode[] children)
            {
                _children = children;
            }

            public BossBehaviorStatus Tick()
            {
                foreach (IBossBehaviorNode child in _children)
                {
                    BossBehaviorStatus status = child.Tick();
                    if (status != BossBehaviorStatus.Failure)
                    {
                        return status;
                    }
                }

                return BossBehaviorStatus.Failure;
            }
        }

        private sealed class SequenceNode : IBossBehaviorNode
        {
            private readonly IBossBehaviorNode[] _children;

            public SequenceNode(params IBossBehaviorNode[] children)
            {
                _children = children;
            }

            public BossBehaviorStatus Tick()
            {
                foreach (IBossBehaviorNode child in _children)
                {
                    BossBehaviorStatus status = child.Tick();
                    if (status != BossBehaviorStatus.Success)
                    {
                        return status;
                    }
                }

                return BossBehaviorStatus.Success;
            }
        }

        private sealed class ConditionNode : IBossBehaviorNode
        {
            private readonly Func<bool> _condition;

            public ConditionNode(Func<bool> condition)
            {
                _condition = condition;
            }

            public BossBehaviorStatus Tick()
            {
                return _condition() ? BossBehaviorStatus.Success : BossBehaviorStatus.Failure;
            }
        }

        private sealed class ActionNode : IBossBehaviorNode
        {
            private readonly Func<BossBehaviorStatus> _action;

            public ActionNode(Func<BossBehaviorStatus> action)
            {
                _action = action;
            }

            public BossBehaviorStatus Tick()
            {
                return _action();
            }
        }
    }
}
