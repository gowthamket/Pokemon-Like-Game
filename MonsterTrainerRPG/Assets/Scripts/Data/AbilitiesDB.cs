using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitiesDB
{
    public static void Init()
    {
        foreach (var kvp in Abilities)
        {
            var abilityId = kvp.Key;
            var ability = kvp.Value;

            ability.Id = abilityId;
            ability.Source = EffectSource.Ability;
            ability.SourceId = (int)abilityId;
        }
    }

    public static Dictionary<AbilityID, Ability> Abilities { get; set; } = new Dictionary<AbilityID, Ability>()
    {
        // 1. Abilities that modify stats
        {
            AbilityID.overgrow,
            new Ability()
            {
                Name = "Overgrow",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Grass && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                },
                OnModifySpAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Grass && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                }
            }
        },
        {
            AbilityID.blaze,
            new Ability()
            {
                Name = "Blaze",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Fire && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                },
                OnModifySpAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Fire && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                }
            }
        },
        {
            AbilityID.torrent,
            new Ability()
            {
                Name = "Torrent",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Water && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                },
                OnModifySpAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Water && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                }
            }
        },
        {
            AbilityID.swarm,
            new Ability()
            {
                Name = "Swarm",
                OnModifyAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Bug && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                },
                OnModifySpAtk = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.Type == PokemonType.Bug && attacker.HP <= attacker.MaxHp / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                }
            }
        },
        {
            AbilityID.compoundeyes,
            new Ability()
            {
                Name = "Compound Eyes",
                OnModifyAcc = (float acc, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    acc = acc * 1.3f;

                    return acc;
                }
            }
        },

        // 2. Abilities that prevent stat boost
        {
            AbilityID.keeneye,
            new Ability()
            {
                Name = "Keen Eye",
                OnBoost = (Dictionary<Stat, int> boosts, Pokemon target, Pokemon source) =>
                {
                    // If it's self boost then return
                    if (source != null && target == source) return;

                    if (boosts.ContainsKey(Stat.Accuracy) && boosts[Stat.Accuracy] < 0)
                    {
                        boosts.Remove(Stat.Accuracy);

                        target.StatusChanges.Enqueue($"{target.Base.Name}'s accuracy cannot be decreased due to it's keen eye");
                    }
                }
            }
        },
        {
            AbilityID.hypercutter,
            new Ability()
            {
                Name = "Hyper Cutter",
                OnBoost = (Dictionary<Stat, int> boosts, Pokemon target, Pokemon source) =>
                {
                    // If it's self boost then return
                    if (source != null && target == source) return;

                    if (boosts.ContainsKey(Stat.Attack) && boosts[Stat.Attack] < 0)
                    {
                        boosts.Remove(Stat.Attack);

                        target.StatusChanges.Enqueue($"{target.Base.Name}'s attack cannot be decreased");
                    }
                }
            }
        },
        {
            AbilityID.clearbody,
            new Ability()
            {
                Name = "Clear Body",
                OnBoost = (Dictionary<Stat, int> boosts, Pokemon target, Pokemon source) =>
                {
                    // If it's self boost then return
                    if (source != null && target == source) return;

                    bool showMsg = false;

                    foreach (var stat in boosts.Keys) {
                        if (boosts[stat] < 0)
                        {
                            showMsg = true;
                            boosts.Remove(Stat.Accuracy);
                        }
                    }

                    if (showMsg)
                    {
                        target.StatusChanges.Enqueue($"{target.Base.Name}'s clear body prevents stat loss");
                    }
                }
            }
        },

        // 3. Abilities that prevent inflicting status conditions
        {
            AbilityID.limber,
            new Ability()
            {
                Name = "Limber",
                OnTrySetStatus = (ConditionID statusId, Pokemon pokemon, EffectData effect) =>
                {
                    if (statusId != ConditionID.par)
                        return true;

                    if (effect != null && effect.Source == EffectSource.Move)
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s immune to paralysis");

                    return false;
                }
            }
        },
        {
            AbilityID.vitalspirit,
            new Ability()
            {
                Name = "Vital Spirit",
                OnTrySetStatus = (ConditionID statusId, Pokemon pokemon, EffectData effect) =>
                {
                    if (statusId != ConditionID.slp)
                        return true;

                    if (effect != null && effect.Source == EffectSource.Move)
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s immune to sleep");

                    return false;
                }
            }
        },
        {
            AbilityID.immunity,
            new Ability()
            {
                Name = "Immunity",
                OnTrySetStatus = (ConditionID statusId, Pokemon pokemon, EffectData effect) =>
                {
                    if (statusId != ConditionID.psn)
                        return true;

                    if (effect != null && effect.Source == EffectSource.Move)
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s immune to poison");

                    return false;
                }
            }
        },
        {
            AbilityID.waterveil,
            new Ability()
            {
                Name = "Water Veil",
                OnTrySetStatus = (ConditionID statusId, Pokemon pokemon, EffectData effect) =>
                {
                    if (statusId != ConditionID.brn)
                        return true;

                    if (effect != null && effect.Source == EffectSource.Move)
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s immune to burn");

                    return false;
                }
            }
        },
        {
            AbilityID.insomnia,
            new Ability()
            {
                Name = "Insomnia",
                OnTrySetStatus = (ConditionID statusId, Pokemon pokemon, EffectData effect) =>
                {
                    if (statusId != ConditionID.slp)
                        return true;

                    if (effect != null && effect.Source == EffectSource.Move)
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s immune to sleep");

                    return false;
                }
            }
        },
        {
            AbilityID.owntempo,
            new Ability()
            {
                Name = "Own Tempo",
                OnTrySetVolatile = (ConditionID statusId, Pokemon pokemon, EffectData effect) =>
                {
                    if (statusId == ConditionID.confusion) 
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s immune to confusion");
                        return false;
                    }

                    return true;
                }
            }
        },

        // 4. Abilities that modify base power of moves
        {
            AbilityID.ironfist,
            new Ability()
            {
                Name = "Iron Fist",
                OnBasePower = (float basePower, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.HasFlag(MoveFlag.Punch))
                    {
                        basePower = basePower * 1.2f;
                    }

                    return basePower;
                }
            }
        },
        {
            AbilityID.strongjaw,
            new Ability()
            {
                Name = "Strong Jaw",
                OnBasePower = (float basePower, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.HasFlag(MoveFlag.Bite))
                    {
                        basePower = basePower * 1.5f;
                    }

                    return basePower;
                }
            }
        },
        {
            AbilityID.toughclaws,
            new Ability()
            {
                Name = "Tough Claws",
                OnBasePower = (float basePower, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.HasFlag(MoveFlag.Contact))
                    {
                        Debug.Log("Tough Claw Boost");
                        basePower = basePower * 1.3f;
                    }

                    return basePower;
                }
            }
        },
        {
            AbilityID.megalauncher,
            new Ability()
            {
                Name = "Mega Launcher",
                OnBasePower = (float basePower, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if (move.Base.HasFlag(MoveFlag.Pulse))
                    {
                        basePower = basePower * 1.5f;
                    }

                    return basePower;
                }
            }
        },

        // 5. Abilities that have effect during damaging hits
        {
            AbilityID.Static,
            new Ability()
            {
                Name = "Static",
                OnDamagingHit = (float damage, Pokemon target, Pokemon attacker, Move move) =>
                {
                    // 30% chance to cause paralyze when hit with contact moves
                    var flags = move.Base.Flags;
                    if (flags != null && flags.Contains(MoveFlag.Contact) && Random.Range(1, 4) == 1)
                    {
                        attacker.SetStatus(ConditionID.par);
                    }
                }
            }
        },
        {
            AbilityID.poisonpoint,
            new Ability()
            {
                Name = "Poison Point",
                OnDamagingHit = (float damage, Pokemon target, Pokemon attacker, Move move) =>
                {
                    // 30% chance to cause poison when hit with contact moves
                    var flags = move.Base.Flags;
                    if (flags != null && flags.Contains(MoveFlag.Contact) && Random.Range(1, 4) == 1)
                    {
                        Debug.Log("Poison Point causes Poison");
                        attacker.SetStatus(ConditionID.psn);
                    }
                }
            }
        },
        {
            AbilityID.flamebody,
            new Ability()
            {
                Name = "Flame Body",
                OnDamagingHit = (float damage, Pokemon target, Pokemon attacker, Move move) =>
                {
                    // 30% chance to cause burn when hit with contact moves
                    var flags = move.Base.Flags;
                    if (flags != null && flags.Contains(MoveFlag.Contact) && Random.Range(1, 4) == 1)
                    {
                        Debug.Log("Flamebody causes paralyze");
                        attacker.SetStatus(ConditionID.brn);
                    }
                }
            }
        },
    };
}

public enum AbilityID
{
    overgrow, blaze, torrent, swarm, compoundeyes, keeneye, hypercutter, clearbody,
    limber, vitalspirit, immunity, waterveil, insomnia, owntempo, ironfist, strongjaw, toughclaws, megalauncher,
    Static, poisonpoint, flamebody
}
