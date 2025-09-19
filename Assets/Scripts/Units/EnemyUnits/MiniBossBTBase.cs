using UnityEngine;
using System.Collections.Generic;

namespace BehaviorTree
{
    public class MiniBossBlackboard : EnemyBlackboard
    {
        public float buffRadius = 5f;
        public float buffMoveSpeedPercent = 0.2f;
        public float buffAttackSpeedPercent = 0.2f;
        public float buffArmor = 10f;
        public float buffMagicDamage = 10f;
        public bool hasBuffedAllies;
        public float lastBuffTime;
        public float buffCooldown = 10f;

        public int minReinforcements = 2;
        public int maxReinforcements = 4;
        public int reinforcementThreshold = 3;
        public float lastSummonTime;
        public float summonCooldown = 15f;
        public int summonWave = 0;
        // Add more fields for tactical command, etc.
    }

    public abstract class MiniBossBTBase : EnemyBTBase
    {
        public new MiniBossBlackboard blackboard;

        protected override void Awake()
        {
            blackboard = new MiniBossBlackboard();
            base.blackboard = blackboard;
        }

        // Command behavior: Buff Allies, Summon Reinforcements, Tactical Command
        public virtual void Command()
        {
            BuffAllies();
            SummonReinforcements();
            TacticalCommand();
        }
        public virtual void BuffAllies()
        {
            if (Time.time < blackboard.lastBuffTime + blackboard.buffCooldown) return;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, blackboard.buffRadius);
            bool buffed = false;
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy") && hit.gameObject != this.gameObject)
                {
                    EnemyBlackboard allyBB = hit.GetComponent<EnemyBTBase>()?.blackboard;
                    if (allyBB != null)
                    {
                        allyBB.moveSpeed += allyBB.moveSpeed * blackboard.buffMoveSpeedPercent;
                        allyBB.Set("buffedByMiniBoss", true);
                        buffed = true;
                        // TODO: Add visual FX here (rune circle, glow, etc.)
                    }
                }
            }
            if (buffed)
            {
                blackboard.hasBuffedAllies = true;
                blackboard.lastBuffTime = Time.time;
            }
        }
        public virtual void SummonReinforcements()
        {
            if (Time.time < blackboard.lastSummonTime + blackboard.summonCooldown) return;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, blackboard.buffRadius);
            int undeadCount = 0;
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy") && hit.gameObject != this.gameObject)
                {
                    undeadCount++;
                }
            }
            if (undeadCount < blackboard.reinforcementThreshold)
            {
                int summonCount = Mathf.Clamp(Random.Range(blackboard.minReinforcements, blackboard.maxReinforcements + 1), 1, 10);
                for (int i = 0; i < summonCount; i++)
                {
                    GameObject prefab = GetSummonPrefabForWave(blackboard.summonWave);
                    Vector3 spawnPos = transform.position + new Vector3(Random.Range(-2f, 2f), 0, 0);
                    GameObject summoned = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
                }
                blackboard.lastSummonTime = Time.time;
                blackboard.summonWave++;
            }
        }

        public virtual void TacticalCommand()
        {
            // Trigger: Player in detection range
            if (blackboard.targetPlayer == null) return;
            float distToPlayer = Vector2.Distance(transform.position, blackboard.targetPlayer.position);
            if (distToPlayer > blackboard.detectionRange) return;

            // Action: All nearby undead focus on player
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, blackboard.buffRadius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy") && hit.gameObject != this.gameObject)
                {
                    EnemyBTBase ally = hit.GetComponent<EnemyBTBase>();
                    if (ally != null && ally.blackboard != null)
                    {
                        ally.blackboard.targetPlayer = blackboard.targetPlayer;
                        ally.blackboard.commandIssued = true;
                        // Optionally: set a flag to focus attack on player
                    }
                }
            }
            // Optionally: Debuff player (e.g., curse)
            var player = blackboard.targetPlayer.GetComponent<PlayerController>();
            if (player != null)
            {
                // Example: player.ApplyDebuff("Curse", duration, effectValue);
                // TODO: Implement debuff logic if needed
            }
            // TODO: Play animation (point at player), FX, etc.
        }

        // Attack behavior: Ranged spells, keep distance, debuff
        public virtual void Attack()
        {
            if (blackboard.targetPlayer == null) return;
            float distToPlayer = Vector2.Distance(transform.position, blackboard.targetPlayer.position);
            if (distToPlayer > blackboard.attackRange) {
                // Move closer if out of range
                MoveTowardsPlayer(blackboard.attackRange - 1f);
                return;
            }
            if (distToPlayer < blackboard.attackRange * 0.5f) {
                // Move away if too close
                MoveAwayFromPlayer(blackboard.attackRange * 0.8f);
            }
            // Cast Dark Bolt
            CastDarkBolt();
            // Optionally: Cast Curse of Weakness
            if (Random.value < 0.3f) CastCurseOfWeakness();
        }

        // Engage behavior: AoE support, buff allies, pressure
        public virtual void Engage()
        {
            if (blackboard.targetPlayer == null) return;
            // Cast AoE spell (Poison Cloud, Bone Spike)
            CastBoneSpikeAoE();
            // Buff allies for pressure
            BuffAllies();
            // TODO: Play engage animation, FX
        }

        // Frenzy behavior: spam skills, summon, nova, sacrifice HP
        public virtual void Frenzy()
        {
            if (blackboard.hp > blackboard.hpMax * 0.5f && Time.time < 30f) return;
            // Ignore keep distance, spam skills
            CastDarkNova();
            SummonReinforcements();
            if (Random.value < 0.2f) SacrificeHPBuffAllies();
            // TODO: Play frenzy animation, FX
        }

        // Retreat behavior: escape, leave traps, fear
        public virtual void Retreat()
        {
            if (blackboard.hp > blackboard.hpMax * 0.2f && blackboard.nearbyAllies.Count > 0) return;
            // Move to spawn/teleport
            RetreatToSpawn();
            // Leave Undead Bomb
            if (Random.value < 0.5f) LeaveUndeadBomb();
            // Cast Fear if possible
            if (HasMana()) CastFear();
            // TODO: Play retreat animation, FX
        }

        // --- Helper methods for actions (stubs, to be implemented in derived class or here) ---
        protected virtual void MoveTowardsPlayer(float stopDist) { /* Move towards player until stopDist */ }
        protected virtual void MoveAwayFromPlayer(float safeDist) { /* Move away from player until safeDist */ }
        protected virtual void CastDarkBolt() { /* Instantiate projectile, play FX */ }
        protected virtual void CastCurseOfWeakness() { /* Debuff player, play FX */ }
        protected virtual void CastBoneSpikeAoE() { /* AoE attack, play FX */ }
        protected virtual void CastDarkNova() { /* AoE nova, play FX */ }
        protected virtual void SacrificeHPBuffAllies() { /* Lose HP, buff all allies */ }
        protected virtual void RetreatToSpawn() { /* Move or teleport to spawn */ }
        protected virtual void LeaveUndeadBomb() { /* Spawn bomb/trap */ }
        protected virtual bool HasMana() { return true; }
        protected virtual void CastFear() { /* Debuff/fear player */ }

        // Helper to select prefab based on wave/upgrade logic
        protected virtual GameObject GetSummonPrefabForWave(int wave)
        {
            // TODO: Replace with your prefab references
            // Example: 0 = skeleton, 1 = skeleton archer, 2+ = skeleton knight
            if (wave == 0) return Resources.Load<GameObject>("Skeleton");
            if (wave == 1) return Resources.Load<GameObject>("SkeletonArcher");
            return Resources.Load<GameObject>("SkeletonKnight");
        }
    }
}
