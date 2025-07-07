using ProjectM;
using ProjectM.Gameplay.Scripting;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace VRisingMods.Core.Utilities;

public static class DebugUtil
{

    public static void LogComponentTypesFromQueries(ComponentSystemBase systemBase)
    {
        LogUtil.LogInfo("========================================");
        var queryCount = 0;
        foreach (var query in systemBase.EntityQueries)
        {
            LogUtil.LogInfo($"query#{queryCount}--------------------------------");
            var entities = query.ToEntityArray(Allocator.Temp);
            for (var i = 0; i < entities.Length; i++)
            {
                DebugUtil.LogComponentTypes(entities[i]);
            }
            queryCount++;
        }
    }

    public static void LogComponentTypes(Entity entity)
    {
        LogUtil.LogMessage("-------------------------------------------");
        var componentTypes = WorldUtil.Game.EntityManager.GetComponentTypes(entity);
        foreach (var componentType in componentTypes)
        {
            LogUtil.LogMessage(componentType.ToString());
        }
        LogUtil.LogMessage("-------------------------------------------");
    }

    public static void LogPrefabGuid(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasComponent<PrefabGUID>(entity))
        {
            return;
        }
        var prefabGuid = entityManager.GetComponentData<PrefabGUID>(entity);
        LogUtil.LogInfo($"  PrefabGUID: {LookupPrefabName(prefabGuid)}");
    }

    public static void LogPlayerCharacter(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasComponent<PlayerCharacter>(entity))
        {
            return;
        }
        var pc = entityManager.GetComponentData<PlayerCharacter>(entity);
        LogUtil.LogInfo($"  PlayerCharacter:");
        LogUtil.LogInfo($"    Name: {pc.Name}");
        LogUtil.LogInfo($"    SmartClanName: {pc.SmartClanName}");
        LogUtil.LogInfo($"    UserEntity: {pc.UserEntity}");
    }

    public static void LogVampireSpecificAttributes(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasComponent<VampireSpecificAttributes>(entity))
        {
            return;
        }
        var vsa = entityManager.GetComponentData<VampireSpecificAttributes>(entity);
        LogUtil.LogInfo($"  VampireSpecificAttributes:");
        LogUtil.LogInfo($"    SilverCoinResisteance: {vsa.SilverCoinResistance.Value}");
        LogUtil.LogInfo($"    PvPResilience: {vsa.PvPResilience.Value}");
        LogUtil.LogInfo($"    HolyResistance: {vsa.HolyResistance.Value}");
        LogUtil.LogInfo($"    SilverResistance: {vsa.SilverResistance.Value}");
        LogUtil.LogInfo($"    GarlicResistance: {vsa.GarlicResistance.Value}");
        LogUtil.LogInfo($"    SunResistance: {vsa.SunResistance.Value}");
        LogUtil.LogInfo($"    PhysicalCriticalStrikeChance: {vsa.PhysicalCriticalStrikeChance.Value}");
        LogUtil.LogInfo($"    PhysicalCriticalStrikeDamage: {vsa.PhysicalCriticalStrikeDamage.Value}");
        LogUtil.LogInfo($"    SpellCriticalStrikeChance: {vsa.SpellCriticalStrikeChance.Value}");
        LogUtil.LogInfo($"    SpellCriticalStrikeDamage: {vsa.SpellCriticalStrikeDamage.Value}");
        LogUtil.LogInfo($"    BonusPhysicalPower: {vsa.BonusPhysicalPower.Value}");
        LogUtil.LogInfo($"    BonusSpellPower: {vsa.BonusSpellPower.Value}");
        LogUtil.LogInfo($"    BonusMaxHealth: {vsa.BonusMaxHealth.Value}");
        LogUtil.LogInfo($"    BonusMovementSpeed: {vsa.BonusMovementSpeed.Value}");
        LogUtil.LogInfo($"    BonusShapeshiftMovementSpeed: {vsa.BonusShapeshiftMovementSpeed.Value}");
        LogUtil.LogInfo($"    ResourceYieldModifier: {vsa.ResourceYieldModifier.Value}");
        LogUtil.LogInfo($"    ReducedResourceDurabilityLoss: {vsa.ReducedResourceDurabilityLoss.Value}");
        LogUtil.LogInfo($"    IncreasedShieldEfficiency: {vsa.IncreasedShieldEfficiency.Value}");
        LogUtil.LogInfo($"    UltimateEfficiency: {vsa.UltimateEfficiency.Value}");
        LogUtil.LogInfo($"    WeaponSkillPower: {vsa.WeaponSkillPower.Value}");
        LogUtil.LogInfo($"    SpellFreeCast: {vsa.SpellFreeCast.Value}");
        LogUtil.LogInfo($"    WeaponFreeCast: {vsa.WeaponFreeCast.Value}");
        LogUtil.LogInfo($"    BloodMendHealEfficiency: {vsa.BloodMendHealEfficiency.Value}");
        LogUtil.LogInfo($"    DemountProtection: {vsa.DemountProtection.Value}");
        LogUtil.LogInfo($"    AdditionalInventorySlots: {vsa.AdditionalInventorySlots.Value}");
        LogUtil.LogInfo($"    BonusPhysicalPowerModId: {vsa.BonusPhysicalPowerModId}");
        LogUtil.LogInfo($"    BonusSpellPowerModId: {vsa.BonusSpellPowerModId}");
        LogUtil.LogInfo($"    BonusMaxHealthModId: {vsa.BonusMaxHealthModId}");
        LogUtil.LogInfo($"    BonusMovementSpeedModId: {vsa.BonusMovementSpeedModId}");
        LogUtil.LogInfo($"    BonusShapeshiftMovementSpeedModId: {vsa.BonusShapeshiftMovementSpeedModId}");
        LogUtil.LogInfo($"    PvPProtected: {vsa.PvPProtected.Value}");
        LogUtil.LogInfo($"    PassiveSlots: {vsa.PassiveSlots}");
    }

    public static void LogBuffs(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasBuffer<BuffBuffer>(entity))
        {
            return;
        }
        LogUtil.LogInfo($"  buffs:");
        var attackerBuffs = entityManager.GetBuffer<BuffBuffer>(entity);
        foreach (var buff in attackerBuffs)
        {
            LogUtil.LogInfo($"    {LookupPrefabName(buff.PrefabGuid)}");
        }
    }

    public static void LogSpellModSet(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasComponent<SpellModSetComponent>(entity))
        {
            return;
        }
        LogUtil.LogInfo($"  spell mods:");
        var smsc = entityManager.GetComponentData<SpellModSetComponent>(entity);
        var sm = smsc.SpellMods;
        LogUtil.LogInfo($"    count:{sm.Count}");
        LogUtil.LogInfo($"    0:{LookupPrefabName(sm.Mod0.Id)}");
        LogUtil.LogInfo($"    1:{LookupPrefabName(sm.Mod1.Id)}");
        LogUtil.LogInfo($"    2:{LookupPrefabName(sm.Mod2.Id)}");
        LogUtil.LogInfo($"    3:{LookupPrefabName(sm.Mod3.Id)}");
        LogUtil.LogInfo($"    4:{LookupPrefabName(sm.Mod4.Id)}");
        LogUtil.LogInfo($"    5:{LookupPrefabName(sm.Mod5.Id)}");
        LogUtil.LogInfo($"    6:{LookupPrefabName(sm.Mod6.Id)}");
        LogUtil.LogInfo($"    7:{LookupPrefabName(sm.Mod7.Id)}");
        LogUtil.LogInfo("     ----");
    }

    public static void LogApplyBuffOnGameplayEvent(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasBuffer<ApplyBuffOnGameplayEvent>(entity))
        {
            return;
        }
        LogUtil.LogInfo($"  Buffs to apply on gameplay events:");
        var applyBuffs = entityManager.GetBuffer<ApplyBuffOnGameplayEvent>(entity);
        foreach (var buff in applyBuffs)
        {
            LogUtil.LogInfo($"    stacks:{buff.Stacks}");
            LogUtil.LogInfo($"    0:{LookupPrefabName(buff.Buff0)}");
            LogUtil.LogInfo($"    1:{LookupPrefabName(buff.Buff1)}");
            LogUtil.LogInfo($"    2:{LookupPrefabName(buff.Buff2)}");
            LogUtil.LogInfo($"    3:{LookupPrefabName(buff.Buff3)}");
            LogUtil.LogInfo($"    spellModSource:{buff.CustomAbilitySpellModsSource}");
            LogUtil.LogInfo("     ----");
        }
    }

    public static void LogProjectilDestroyData(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasComponent<ProjectileDestroyData>(entity))
        {
            return;
        }
        var pdd = entityManager.GetComponentData<ProjectileDestroyData>(entity);
        LogUtil.LogInfo($"  ProjectileDestroyData:");
        LogUtil.LogInfo($"    HasHitTarget: {pdd.HasHitTarget}");
    }

    public static void LogHitColliderCast(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasBuffer<HitColliderCast>(entity))
        {
            return;
        }
        LogUtil.LogInfo($"  HitColliderCast [Buffer]:");
        var buffer = entityManager.GetBuffer<HitColliderCast>(entity);
        for (var i = 0; i < buffer.Length; i++)
        {
            var hcc = buffer[i];
            LogUtil.LogInfo($"    HitColliderCast [{i}]");
            LogUtil.LogInfo($"      Shape");
            LogUtil.LogInfo($"        Type: {hcc.Shape.Type}");
            LogUtil.LogInfo($"        RadiusOrWidth: {hcc.Shape.RadiusOrWidth}");
            LogUtil.LogInfo($"        InnerRadiusOrHeight: {hcc.Shape.InnerRadiusOrHeight}");
            LogUtil.LogInfo($"        Length: {hcc.Shape.Length}");
            LogUtil.LogInfo($"        Angle: {hcc.Shape.Angle}");
            LogUtil.LogInfo($"      AfterDuration: {hcc.AfterDuration}");
            LogUtil.LogInfo($"      TerrainColliderModifier: {hcc.TerrainColliderModifier}");
            LogUtil.LogInfo($"      PrioritySettings");
            LogUtil.LogInfo($"        Near_Origin_Factor: {hcc.PrioritySettings.Near_Origin_Factor}");
            LogUtil.LogInfo($"        Near_Origin_Distance: {hcc.PrioritySettings.Near_Origin_Distance}");
            LogUtil.LogInfo($"        Target_Priority_Factor: {hcc.PrioritySettings.Target_Priority_Factor}");
            LogUtil.LogInfo($"        UseMeleeCone: {hcc.PrioritySettings.UseMeleeCone}");
            LogUtil.LogInfo($"        Melee_Cone_Angle {hcc.PrioritySettings.Melee_Cone_Angle}");
            LogUtil.LogInfo($"        UseColliderCenterAsOriginPosition: {hcc.PrioritySettings.UseColliderCenterAsOriginPosition}");
            LogUtil.LogInfo($"      CollisionCheckType: {hcc.CollisionCheckType}");
            LogUtil.LogInfo($"      PrimaryFilterFlags: {hcc.PrimaryFilterFlags}");
            LogUtil.LogInfo($"      PrimaryTargets_Count: {hcc.PrimaryTargets_Count}");
            LogUtil.LogInfo($"      SecondaryTargets_Count: {hcc.SecondaryTargets_Count}");
            LogUtil.LogInfo($"      ContinuousCollision: {hcc.ContinuousCollision}");
            LogUtil.LogInfo($"      IncludeTerrain: {hcc.IncludeTerrain}");
            LogUtil.LogInfo($"      CanHitThroughBlockSpellCollision: {hcc.CanHitThroughBlockSpellCollision}");
            LogUtil.LogInfo($"      IgnoreImmaterial: {hcc.IgnoreImmaterial}");
            LogUtil.LogInfo("    ----");
        }
    }

    public static void LogHitTriggers(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasBuffer<HitTrigger>(entity))
        {
            return;
        }
        LogUtil.LogInfo($"  HitTrigger [Buffer]:");
        var buffer = entityManager.GetBuffer<HitTrigger>(entity);
        for (var i = 0; i < buffer.Length; i++)
        {
            var ht = buffer[i];
            LogUtil.LogInfo($"    HitTrigger [{i}]");
            LogUtil.LogInfo($"      HitTime: {ht.HitTime}");
            LogUtil.LogInfo($"      Target: {ht.Target}");
            LogUtil.LogInfo($"      OriginPosition: {ht.OriginPosition}");
            LogUtil.LogInfo($"      CollisionPosition: {ht.CollisionPosition}");
            LogUtil.LogInfo($"      CollisionRotation: {ht.CollisionRotation}");
            LogUtil.LogInfo($"      Handled: {ht.Handled}");
            LogUtil.LogInfo($"      Ignore: {ht.Ignore}");
            LogUtil.LogInfo($"      HitGroup: {ht.HitGroup}");
            LogUtil.LogInfo($"      CastIndex: {ht.CastIndex}");
            LogUtil.LogInfo("    ----");
        }
    }

    public static void LogTriggerHitConsume(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasBuffer<TriggerHitConsume>(entity))
        {
            return;
        }
        LogUtil.LogInfo($"  TriggerHitConsume [Buffer]:");
        var buffer = entityManager.GetBuffer<TriggerHitConsume>(entity);
        for (var i = 0; i < buffer.Length; i++)
        {
            var thc = buffer[i];
            LogUtil.LogInfo($"    TriggerHitConsume [{i}]");
            LogUtil.LogInfo($"      SpellCategory: {thc.SpellCategory}");
            LogUtil.LogInfo($"      EventIdIndex: {thc.EventIdIndex}");
            LogUtil.LogInfo($"      EventIdCount: {thc.EventIdCount}");
            LogUtil.LogInfo("    ----");
        }
    }

    public static void LogPlayImpactOnGameplayEvent(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasBuffer<PlayImpactOnGameplayEvent>(entity))
        {
            return;
        }
        LogUtil.LogInfo($"  PlayImpactOnGameplayEvent [Buffer]:");
        var buffer = entityManager.GetBuffer<PlayImpactOnGameplayEvent>(entity);
        for (var i = 0; i < buffer.Length; i++)
        {
            var pioge = buffer[i];
            LogUtil.LogInfo($"    PlayImpactOnGameplayEvent [{i}]");
            LogUtil.LogInfo($"      PrimarySequenceGuid: {pioge.PrimarySequenceGuid}");
            LogUtil.LogInfo($"      ImpactMappingGuid: {LookupPrefabName(pioge.ImpactMappingGuid)}");
            LogUtil.LogInfo($"      SkipMaterialSequence: {pioge.SkipMaterialSequence}");
            LogUtil.LogInfo($"      RotationOffsetEulerMin: {pioge.RotationOffsetEulerMin}");
            LogUtil.LogInfo($"      RotationOffsetEulerMax: {pioge.RotationOffsetEulerMax}");
            LogUtil.LogInfo($"      Scale: {pioge.Scale}");
            LogUtil.LogInfo($"      SequenceRotationTarget: {pioge.SequenceRotationTarget}");
            LogUtil.LogInfo("    ----");
        }
    }

    public static void LogDealDamageOnGameplayEvent(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasBuffer<DealDamageOnGameplayEvent>(entity))
        {
            return;
        }
        LogUtil.LogInfo($"  DealDamageOnGameplayEvent [Buffer]:");
        var buffer = entityManager.GetBuffer<DealDamageOnGameplayEvent>(entity);
        for (var i = 0; i < buffer.Length; i++)
        {
            var ddoge = buffer[i];
            LogUtil.LogInfo($"    DealDamageOnGameplayEvent [{i}]");
            LogUtil.LogInfo($"      Parameters");
            LogUtil.LogInfo($"        MaterialModifiers: {ddoge.Parameters.MaterialModifiers}");
            LogUtil.LogInfo($"          Human: {ddoge.Parameters.MaterialModifiers.Human}");
            LogUtil.LogInfo($"          Undead: {ddoge.Parameters.MaterialModifiers.Undead}");
            LogUtil.LogInfo($"          Demon: {ddoge.Parameters.MaterialModifiers.Demon}");
            LogUtil.LogInfo($"          Mechanical: {ddoge.Parameters.MaterialModifiers.Mechanical}");
            LogUtil.LogInfo($"          Beast: {ddoge.Parameters.MaterialModifiers.Beast}");
            LogUtil.LogInfo($"          CastleObject: {ddoge.Parameters.MaterialModifiers.CastleObject}");
            LogUtil.LogInfo($"          PlayerVampire: {ddoge.Parameters.MaterialModifiers.PlayerVampire}");
            LogUtil.LogInfo($"          PvEVampire: {ddoge.Parameters.MaterialModifiers.PvEVampire}");
            LogUtil.LogInfo($"          ShadowVBlood: {ddoge.Parameters.MaterialModifiers.ShadowVBlood}");
            LogUtil.LogInfo($"          BasicStructure: {ddoge.Parameters.MaterialModifiers.BasicStructure}");
            LogUtil.LogInfo($"          ReinforcedStructure: {ddoge.Parameters.MaterialModifiers.ReinforcedStructure}");
            LogUtil.LogInfo($"          FortifiedStructure: {ddoge.Parameters.MaterialModifiers.FortifiedStructure}");
            LogUtil.LogInfo($"          StoneStructure: {ddoge.Parameters.MaterialModifiers.StoneStructure}");
            LogUtil.LogInfo($"          SiegeAltar: {ddoge.Parameters.MaterialModifiers.SiegeAltar}");
            LogUtil.LogInfo($"          Wood: {ddoge.Parameters.MaterialModifiers.Wood}");
            LogUtil.LogInfo($"          Minerals: {ddoge.Parameters.MaterialModifiers.Minerals}");
            LogUtil.LogInfo($"          Vegetation: {ddoge.Parameters.MaterialModifiers.Vegetation}");
            LogUtil.LogInfo($"          LightArmor: {ddoge.Parameters.MaterialModifiers.LightArmor}");
            LogUtil.LogInfo($"          VBlood: {ddoge.Parameters.MaterialModifiers.VBlood}");
            LogUtil.LogInfo($"          Magic: {ddoge.Parameters.MaterialModifiers.Magic}");
            LogUtil.LogInfo($"          Explosives: {ddoge.Parameters.MaterialModifiers.Explosives}");
            LogUtil.LogInfo($"          MassiveResource: {ddoge.Parameters.MaterialModifiers.MassiveResource}");
            LogUtil.LogInfo($"          MonsterGate: {ddoge.Parameters.MaterialModifiers.MonsterGate}");
            LogUtil.LogInfo($"        MainFactor: {ddoge.Parameters.MainFactor}");
            LogUtil.LogInfo($"        ResourceModifier: {ddoge.Parameters.ResourceModifier}");
            LogUtil.LogInfo($"        StaggerFactor: {ddoge.Parameters.StaggerFactor}");
            LogUtil.LogInfo($"        RawDamageValue: {ddoge.Parameters.RawDamageValue}");
            LogUtil.LogInfo($"        RawDamagePercent: {ddoge.Parameters.RawDamagePercent}");
            LogUtil.LogInfo($"        DealDamageFlags: {ddoge.Parameters.DealDamageFlags}");
            LogUtil.LogInfo($"        MainType: {ddoge.Parameters.MainType}");
            LogUtil.LogInfo($"      DamageModifierPerHit: {ddoge.DamageModifierPerHit}");
            LogUtil.LogInfo($"      MultiplyMainFactorWithStacks: {ddoge.MultiplyMainFactorWithStacks}");
            LogUtil.LogInfo("    ----");
        }
    }

    public static void LogGameplayEventListeners(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasBuffer<GameplayEventListeners>(entity))
        {
            return;
        }
        LogUtil.LogInfo($"  GameplayEventListeners [Buffer]:");
        var buffer = entityManager.GetBuffer<GameplayEventListeners>(entity);
        for (var i = 0; i < buffer.Length; i++)
        {
            var gel = buffer[i];
            LogUtil.LogInfo($"    GameplayEventListeners [{i}]");
            LogUtil.LogInfo($"      EventIdIndex: {gel.EventIdIndex}");
            LogUtil.LogInfo($"      EventIndexOfType: {gel.EventIndexOfType}");
            LogUtil.LogInfo($"      GameplayEventType: {gel.GameplayEventType}");
            LogUtil.LogInfo($"      GameplayEventId: {gel.GameplayEventId}");
            LogUtil.LogInfo($"        GameplayEventType: {gel.GameplayEventId.GameplayEventType}");
            LogUtil.LogInfo($"        EventId: {gel.GameplayEventId.EventId}");
            LogUtil.LogInfo("    ----");
        }
    }

    public static void LogCreateGameplayEventsOnHit(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasBuffer<CreateGameplayEventsOnHit>(entity))
        {
            return;
        }
        LogUtil.LogInfo($"  CreateGameplayEventsOnHit [Buffer]:");
        var buffer = entityManager.GetBuffer<CreateGameplayEventsOnHit>(entity);
        for (var i = 0; i < buffer.Length; i++)
        {
            var cgeoh = buffer[i];
            LogUtil.LogInfo($"    CreateGameplayEventsOnHit [{i}]");
            LogUtil.LogInfo($"      EventId: {cgeoh.EventId}");
            LogUtil.LogInfo($"      HitGroup: {cgeoh.HitGroup}");
            LogUtil.LogInfo($"      ColliderCastIndex: {cgeoh.ColliderCastIndex}");
            LogUtil.LogInfo("    ----");
        }
    }

    public static void LogAbilityProjectileFanOnGameplayEvent_DataServer(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasComponent<AbilityProjectileFanOnGameplayEvent_DataServer>(entity))
        {
            return;
        }
        var apfogeds = entityManager.GetComponentData<AbilityProjectileFanOnGameplayEvent_DataServer>(entity);
        LogUtil.LogInfo($"  AbilityProjectileFanOnGameplayEvent_DataServer:");
        LogUtil.LogInfo($"    Angle: {apfogeds.Angle}");
        LogUtil.LogInfo($"    OffsetAngle: {apfogeds.OffsetAngle}");
        LogUtil.LogInfo($"    RandomOffset: {apfogeds.RandomOffset}");
        LogUtil.LogInfo($"    IndividualRandom: {apfogeds.IndividualRandom}");
        LogUtil.LogInfo($"    NewProjectileEntity: {LookupPrefabName(apfogeds.NewProjectileEntity)}");
        LogUtil.LogInfo($"    NewProjectileEntityAlternate: {LookupPrefabName(apfogeds.NewProjectileEntityAlternate)}");
        LogUtil.LogInfo($"    TickCount: {apfogeds.TickCount}");
        LogUtil.LogInfo($"    UseOwnerAimDirection: {apfogeds.UseOwnerAimDirection}");
        LogUtil.LogInfo($"    SetSpellTargetToSelf: {apfogeds.SetSpellTargetToSelf}");
        LogUtil.LogInfo($"    SetSpellTargetToSelfSpellTarget: {apfogeds.SetSpellTargetToSelfSpellTarget}");
        LogUtil.LogInfo($"    SetSpellTargetToBuffTarget: {apfogeds.SetSpellTargetToBuffTarget}");
        LogUtil.LogInfo($"    SetRandomTargetInRadiusToSpellTarget: {apfogeds.SetRandomTargetInRadiusToSpellTarget}");
        LogUtil.LogInfo($"    RandomTargetMaxHeightDiff: {apfogeds.RandomTargetMaxHeightDiff}");
        LogUtil.LogInfo($"    RandomSpellTargetHitFilter: {apfogeds.RandomSpellTargetHitFilter}");
        LogUtil.LogInfo($"    BoostBuffType: {apfogeds.BoostBuffType}");
        LogUtil.LogInfo($"    BoostPerStackCount: {apfogeds.BoostPerStackCount}");
        LogUtil.LogInfo($"    BoostPerStackAngle: {apfogeds.BoostPerStackAngle}");
        LogUtil.LogInfo($"    BoostPerStackPrefab1: {LookupPrefabName(apfogeds.BoostPerStackPrefab1)}");
        LogUtil.LogInfo($"    BoostPerStackPrefab2: {LookupPrefabName(apfogeds.BoostPerStackPrefab2)}");
        LogUtil.LogInfo($"    BoostPerStackPrefab3: {LookupPrefabName(apfogeds.BoostPerStackPrefab3)}");
        LogUtil.LogInfo($"    OverrideProjectileSpeedMin: {apfogeds.OverrideProjectileSpeedMin}");
        LogUtil.LogInfo($"    OverrideProjectileSpeedMax: {apfogeds.OverrideProjectileSpeedMax}");
    }

    public static string LookupPrefabName(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasComponent<PrefabGUID>(entity))
        {
            return "Entity does not have PrefabGUID component";
        }
        var prefabGuid = entityManager.GetComponentData<PrefabGUID>(entity);
        return LookupPrefabName(prefabGuid);
    }

    public static string LookupPrefabName(PrefabGUID prefabGuid)
    {
        var prefabCollectionSystem = WorldUtil.Game.GetExistingSystemManaged<PrefabCollectionSystem>();
        var prefabLookupMap = prefabCollectionSystem._PrefabLookupMap;
        if (prefabLookupMap.GuidToEntityMap.ContainsKey(prefabGuid))
        {
            return $"{prefabLookupMap.GetName(prefabGuid)} PrefabGUID({prefabGuid.GuidHash})";
        }
        return $"GUID Not Found {prefabGuid._Value}";
    }

}
