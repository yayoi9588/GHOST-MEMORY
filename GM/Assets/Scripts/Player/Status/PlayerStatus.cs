using System;
using UnityEngine;

namespace GhostMemory.Player
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-150)]
    public class PlayerStatus : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] float maxHealth = 100f;
        [SerializeField] bool invulnerable;

        [Header("Stamina")]
        [SerializeField] float maxStamina = 100f;

        [Header("Defense")]
        [SerializeField] float armor;

        public float MaxHealth => maxHealth;
        public float Health { get; private set; }
        public float MaxStamina => maxStamina;
        public float Stamina { get; private set; }
        public float Armor => armor;
        public bool Invulnerable
        {
            get => invulnerable;
            set => invulnerable = value;
        }

        public bool IsDead { get; private set; }
        public float HealthNormalized => maxHealth > 0f ? Mathf.Clamp01(Health / maxHealth) : 0f;
        public float StaminaNormalized => maxStamina > 0f ? Mathf.Clamp01(Stamina / maxStamina) : 0f;

        public event Action<PlayerStatId, float, float> StatChanged;
        public event Action<float, float> Damaged;
        public event Action<float, float> Healed;
        public event Action Died;
        public event Action Revived;

        void Awake()
        {
            Health = maxHealth;
            Stamina = maxStamina;
        }

        public float Get(PlayerStatId id)
        {
            switch (id)
            {
                case PlayerStatId.Health: return Health;
                case PlayerStatId.MaxHealth: return maxHealth;
                case PlayerStatId.Stamina: return Stamina;
                case PlayerStatId.MaxStamina: return maxStamina;
                case PlayerStatId.Armor: return armor;
                default: return 0f;
            }
        }

        public void Damage(float amount)
        {
            if (IsDead || invulnerable || amount <= 0f)
                return;

            float applied = Mitigate(amount);
            if (applied <= 0f)
                return;

            SetHealth(Health - applied);
            Damaged?.Invoke(applied, Health);

            if (Health <= 0f && !IsDead)
            {
                IsDead = true;
                Died?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f)
                return;

            float previous = Health;
            SetHealth(Health + amount);
            float applied = Health - previous;
            if (applied > 0f)
                Healed?.Invoke(applied, Health);
        }

        public void Revive(float health)
        {
            IsDead = false;
            SetHealth(Mathf.Max(health, 1f));
            Revived?.Invoke();
        }

        public void Kill()
        {
            if (IsDead)
                return;

            invulnerable = false;
            SetHealth(0f);
            IsDead = true;
            Died?.Invoke();
        }

        public bool TryConsumeStamina(float amount)
        {
            if (amount <= 0f)
                return true;
            if (Stamina < amount)
                return false;

            SetStamina(Stamina - amount);
            return true;
        }

        public void RestoreStamina(float amount)
        {
            if (amount <= 0f)
                return;

            SetStamina(Stamina + amount);
        }

        public void SetMaxHealth(float value, bool scaleCurrent)
        {
            float clamped = Mathf.Max(1f, value);
            float ratio = HealthNormalized;
            Raise(PlayerStatId.MaxHealth, maxHealth, clamped);
            maxHealth = clamped;
            SetHealth(scaleCurrent ? clamped * ratio : Health);
        }

        public void SetMaxStamina(float value, bool scaleCurrent)
        {
            float clamped = Mathf.Max(0f, value);
            float ratio = StaminaNormalized;
            Raise(PlayerStatId.MaxStamina, maxStamina, clamped);
            maxStamina = clamped;
            SetStamina(scaleCurrent ? clamped * ratio : Stamina);
        }

        public void SetArmor(float value)
        {
            float clamped = Mathf.Max(0f, value);
            Raise(PlayerStatId.Armor, armor, clamped);
            armor = clamped;
        }

        void SetHealth(float value)
        {
            float clamped = Mathf.Clamp(value, 0f, maxHealth);
            Raise(PlayerStatId.Health, Health, clamped);
            Health = clamped;
        }

        void SetStamina(float value)
        {
            float clamped = Mathf.Clamp(value, 0f, maxStamina);
            Raise(PlayerStatId.Stamina, Stamina, clamped);
            Stamina = clamped;
        }

        void Raise(PlayerStatId id, float previous, float next)
        {
            if (Mathf.Approximately(previous, next))
                return;

            StatChanged?.Invoke(id, previous, next);
        }

        float Mitigate(float amount)
        {
            if (armor <= 0f)
                return amount;

            return amount * (100f / (100f + armor));
        }
    }
}
