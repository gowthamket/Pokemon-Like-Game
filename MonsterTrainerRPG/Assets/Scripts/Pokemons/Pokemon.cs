using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }

    public PokemonBase Base { 
        get {
            return _base;
        }
    }
    public int Level { 
        get {
            return level;
        }
    }

    public int Exp { get; set; }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }

    public Ability Ability { get; private set; }

    public Queue<string> StatusChanges { get; private set; }
    public bool HpChanged { get; set; }
    public event System.Action OnStatusChanged;

    public void Init()
    {
        // Generate Moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= PokemonBase.MaxNumOfMoves)
                break;
        }

        Exp = Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHp;

        StatusChanges = new Queue<string>();
        ResetStatBoost();

        Ability = AbilitiesDB.Abilities[Base.AbilityID];
        Status = null;
        VolatileStatus = null;
    }

    public Pokemon(PokemonSaveData saveData)
    {
        _base = PokemonDB.GetPokemonByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;

        if (saveData.statusId != null)
            Status = ConditionsDB.Conditions[saveData.statusId.Value];
        else
            Status = null;

        Moves = saveData.moves.Select(s => new Move(s)).ToList();

        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoost();
        VolatileStatus = null;
    }

    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData()
        {
            name = Base.Name,
            hp = HP,
            level = Level,
            exp = Exp,
            statusId = Status?.Id,
            moves = Moves.Select(m => m.GetSaveData()).ToList()
        };

        return saveData;
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHp = Mathf.FloorToInt((Base.Speed * Level) / 100f) + 10 + Level;
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0},
        };
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        // Apply stat boost
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);

        return statVal;
    }

    public void ApplyBoosts(Dictionary<Stat, int> boosts, Pokemon source)
    {
        OnBoost(boosts, source);

        foreach (var kvp in boosts)
        {
            var stat = kvp.Key;
            var boost = kvp.Value;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            else
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");

            Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            return true;
        }

        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(LearnableMove moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaxNumOfMoves)
            return;

        Moves.Add(new Move(moveToLearn.Base));
    }

    public int Attack {
        get { return GetStat(Stat.Attack); }
    }

    public int Defense {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense {
        get { return GetStat(Stat.SpDefense); }
    }

    public int Speed {
        get {
            return GetStat(Stat.Speed);
        }
    }

    public int MaxHp { get; private set; }

    public DamageDetails TakeDamage(Move move, Pokemon attacker, Condition weather)
    {
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
            critical = 2f;

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        float weatherMod = weather?.OnDamageModify?.Invoke(this, attacker, move) ?? 1f;

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float attack;
        float defense;
        if (move.Base.Category == MoveCategory.Special)
        {
            attack = attacker.SpAttack;
            defense = SpDefense;

            // Abilites & Held Items might modify the stats
            attack = attacker.ModifySpAtk(attack, attacker, move);
            defense = ModifySpDef(defense, attacker, move);
        }
        else
        {
            attack = attacker.Attack;
            defense = Defense;

            // Abilites & Held might modify the stats
            attack = attacker.ModifyAtk(attack, attacker, move);
            defense = ModifyDef(defense, attacker, move);
        }

        // Abilities might modify base power
        int basePower = Mathf.FloorToInt(attacker.OnBasePower(move.Base.Power, attacker, this, move));

        float modifiers = Random.Range(0.85f, 1f) * type * critical * weatherMod;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * basePower * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);

        if (damage > 0)
            OnDamagingHit(damage, attacker, move);

        return damageDetails;
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        HpChanged = true;
    }

    public void SetStatus(ConditionID conditionId, EffectData effect=null)
    {
        if (Status != null) return;

        bool canSet = Ability?.OnTrySetStatus?.Invoke(conditionId, this, effect) ?? true;
        if (!canSet) return;

        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionId, EffectData effect = null)
    {
        if (VolatileStatus != null) return;

        bool canSet = Ability?.OnTrySetVolatile?.Invoke(conditionId, this, effect) ?? true;
        if (!canSet) return;

        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
                canPerformMove = false;
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
                canPerformMove = false;
        }

        return canPerformMove;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }

    public void OnDamagingHit(float damage, Pokemon attacker, Move move)
    {
        Ability?.OnDamagingHit?.Invoke(damage, this, attacker, move);
    }

    public void OnBoost(Dictionary<Stat, int> boosts, Pokemon source)
    {
        Ability?.OnBoost?.Invoke(boosts, this, source);
    }

    public float OnBasePower(float basePower, Pokemon attacker, Pokemon defender, Move move)
    {
        if (Ability?.OnBasePower != null)
            basePower = Ability.OnBasePower(basePower, attacker, defender, move);

        return basePower;
    }

    public float ModifyAtk(float atk, Pokemon attacker, Move move)
    {
        if (Ability?.OnModifyAtk != null)
            atk = Ability.OnModifyAtk(atk, attacker, this, move);

        return atk;
    }

    public float ModifySpAtk(float atk, Pokemon attacker, Move move)
    {
        if (Ability?.OnModifySpAtk != null)
            atk = Ability.OnModifySpAtk(atk, attacker, this, move);

        return atk;
    }

    public float ModifyDef(float def, Pokemon attacker, Move move)
    {
        if (Ability?.OnModifyDef != null)
            def = Ability.OnModifyDef(def, attacker, this, move);

        return def;
    }

    public float ModifySpDef(float def, Pokemon attacker, Move move)
    {
        if (Ability?.OnModifySpDef != null)
            def = Ability.OnModifySpDef(def, attacker, this, move);

        return def;
    }

    public float ModifySpd(float spd, Pokemon attacker, Move move)
    {
        if (Ability?.OnModifySpd != null)
            spd = Ability.OnModifySpd(spd, attacker, this, move);

        return spd;
    }

    public float ModifyAcc(float acc, Pokemon attacker, Pokemon defender, Move move)
    {
        if (Ability?.OnModifyAcc != null)
            acc = Ability.OnModifyAcc(acc, attacker, defender, move);

        return acc;
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

[System.Serializable]
public class PokemonSaveData
{
    public string name;
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;
}
