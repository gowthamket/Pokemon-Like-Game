using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : EffectData
{
    public AbilityID Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public Action<Dictionary<Stat, int>, Pokemon, Pokemon> OnBoost { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnBasePower { get; set; }
    public Action<float, Pokemon, Pokemon, Move> OnDamagingHit { get; set; }

    public Func<float, Pokemon, Pokemon, Move, float> OnModifyAtk { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifyDef { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifySpAtk { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifySpDef { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifySpd { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifyAcc { get; set; }

    public Func<ConditionID, Pokemon, EffectData, bool> OnTrySetVolatile { get; set; }
    public Func<ConditionID, Pokemon, EffectData, bool> OnTrySetStatus { get; set; }
}
