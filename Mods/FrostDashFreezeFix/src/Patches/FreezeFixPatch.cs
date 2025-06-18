using System.Collections.Generic;
using HarmonyLib;
using HookDOTS.API.Attributes;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Scripting;
using ProjectM.Shared;
using ProjectM.WeaponCoating;
using Stunlock.Core;
using Stunlock.Network;
using Unity.Collections;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace FrostDashFreezeFix.Patches;

[HarmonyPatch]
internal static class FreezeFixPatch
{
    static PrefabGUID AB_Vampire_VeilOfFrost_Buff = new PrefabGUID(-879243806);
    static PrefabGUID AB_Frost_Shared_SpellMod_FrostWeapon_Buff = new PrefabGUID(-1510452092);
    static PrefabGUID Frost_Vampire_Buff_Chill = new PrefabGUID(27300215);
    static PrefabGUID NullPrefabGUID = new PrefabGUID(0);


    // during StatChangeMutationSystem prefix: attacker has the AB_Vampire_VeilOfFrost_Buff
    // I suspect DealDamageSystem is the ideal target, but it's unmanaged so can't hook
    // prefixing StatChangeSystem: frost weapon damage appears a frame after the auto attack appears
    // postfixing CreateGameplayEventsOnDamageTakenSystem: frost weapon damage appears a frame after the auto attack appears
    // actually, it's all in the same frame, but each system runs multiple times inside the RecursiveGroup until there's nothing left to "chain"
    // the damage taken event first appears after StatChangeSystem
    // note: disabling Apply_BuffModificationsSystem_Server blocks freezes from happening in general.
    [EcsSystemUpdatePostfix(typeof(StatChangeSystem))]
    public static void OnUpdate()
    {
        FreezeFixUtil.CurrentTick_CallCount++;

        var entityManager = WorldUtil.Server.EntityManager;
        var query = entityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<DamageTakenEvent>(),
            },
        });

        var damageTakenEvents = query.ToEntityArray(Allocator.Temp);
        foreach (var eventEntity in damageTakenEvents)
        {
            //DebugDamageTakenEvent(eventEntity);
            //DoFix(entityManager, eventEntity);
        }
    }

    static void DoFix(EntityManager entityManager, Entity eventEntity)
    {
        var damageTaken = entityManager.GetComponentData<DamageTakenEvent>(eventEntity);
        var attackerEntity = entityManager.GetComponentData<EntityOwner>(damageTaken.Source);
        if (!entityManager.HasComponent<PlayerCharacter>(attackerEntity))
        {
            return;
        }

        // record frost dash attackers seen in this frame
        var attackerBuffs = entityManager.GetBuffer<BuffBuffer>(attackerEntity);
        foreach (var buff in attackerBuffs)
        {
            if (buff.PrefabGuid.Equals(AB_Vampire_VeilOfFrost_Buff))
            {
                FreezeFixUtil.FrostDashAttackersThisTick.Add(attackerEntity);
            }
        }

        // don't apply redundant out-of-order chill (the frost dash will apply it if appropriate, after the consume check)
        var sourcePrefabGuid = entityManager.GetComponentData<PrefabGUID>(damageTaken.Source);
        if (sourcePrefabGuid.Equals(AB_Frost_Shared_SpellMod_FrostWeapon_Buff))
        {
            // todo: only if they frost dash attacked in this tick

            if (entityManager.HasBuffer<ApplyBuffOnGameplayEvent>(damageTaken.Source))
            {
                LogUtil.LogError("will be clearing buffs");
                var buffsToApply = entityManager.GetBuffer<ApplyBuffOnGameplayEvent>(damageTaken.Source);

                for (var i = 0; i < buffsToApply.Length; i++)
                {
                    var buff = buffsToApply[i];
                    if (buff.Buff0.Equals(Frost_Vampire_Buff_Chill))
                    {
                        buff.Buff0 = NullPrefabGUID;
                    }
                    if (buff.Buff1.Equals(Frost_Vampire_Buff_Chill))
                    {
                        buff.Buff1 = NullPrefabGUID;
                    }
                    if (buff.Buff2.Equals(Frost_Vampire_Buff_Chill))
                    {
                        buff.Buff2 = NullPrefabGUID;
                    }
                    if (buff.Buff3.Equals(Frost_Vampire_Buff_Chill))
                    {
                        buff.Buff3 = NullPrefabGUID;
                    }
                    buffsToApply[i] = buff;
                }

                DebugDamageTakenEvent(eventEntity);
            }
        }

    }


    static void DebugDamageTakenEvent(Entity eventEntity)
    {
        var entityManager = WorldUtil.Server.EntityManager;

        //DebugUtil.LogComponentTypes(eventEntity);

        var damageTaken = entityManager.GetComponentData<DamageTakenEvent>(eventEntity);
        //DebugUtil.LogComponentTypes(damageTaken.Source);

        var attackerEntity = entityManager.GetComponentData<EntityOwner>(damageTaken.Source);
        //DebugUtil.LogComponentTypes(entityOwner.Owner);
        if (!entityManager.HasComponent<PlayerCharacter>(attackerEntity))
        {
            return;
        }
        //DebugUtil.LogComponentTypes(attackerEntity);

        LogUtil.LogInfo($"DamageTakenEvent from inspection#{FreezeFixUtil.TickCount}-{FreezeFixUtil.CurrentTick_CallCount} ---------------------------------------");

        var sourcePrefabGuid = entityManager.GetComponentData<PrefabGUID>(damageTaken.Source);
        LogUtil.LogInfo($"damage taken from source with prefab guid: {LookupPrefabName(sourcePrefabGuid)}");

        if (entityManager.HasBuffer<CreateGameplayEventsOnHit>(damageTaken.Source))
        {
            LogUtil.LogInfo($"  gameplay events to create on hit:");
            var gameplayEvents = entityManager.GetBuffer<CreateGameplayEventsOnHit>(damageTaken.Source);
            foreach (var ge in gameplayEvents)
            {
                LogUtil.LogInfo($"    EventId:{ge.EventId.EventId}, {ge.EventId.GameplayEventType}");
            }
        }

        if (entityManager.HasBuffer<ApplyBuffOnGameplayEvent>(damageTaken.Source))
        {
            LogUtil.LogInfo($"  Buffs to apply on gameplay events (damageTaken.Source):");
            var applyBuffs = entityManager.GetBuffer<ApplyBuffOnGameplayEvent>(damageTaken.Source);
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

        if (entityManager.HasBuffer<ApplyBuffOnGameplayEvent>(attackerEntity))
        {
            LogUtil.LogInfo($"  Buffs to apply on gameplay events (attacker):");
            var applyBuffs = entityManager.GetBuffer<ApplyBuffOnGameplayEvent>(attackerEntity);
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

        if (entityManager.HasBuffer<BuffBuffer>(attackerEntity))
        {
            LogUtil.LogInfo($"  attacker's buffs:");
            var attackerBuffs = entityManager.GetBuffer<BuffBuffer>(attackerEntity);
            foreach (var buff in attackerBuffs)
            {
                LogUtil.LogInfo($"    {LookupPrefabName(buff.PrefabGuid)}");
            }
        }

        if (entityManager.HasBuffer<SpellModSetComponent>(damageTaken.Source))
        {
            LogUtil.LogInfo($"  spell mods:");
            var smsc = entityManager.GetComponentData<SpellModSetComponent>(damageTaken.Source);
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

        LogUtil.LogInfo("---------------------------------------");

        #region debug info from triggering cold snap
        // COLD SNAP TRIGGERED (with arctic energy jewel):
        // damage taken from source with prefab guid: AB_Frost_ColdSnap_Area PrefabGuid(-1314524417)
        //   gameplay events to create on hit:
        //     EventId:-801788231, Local
        //   Buffs to apply on gameplay events (damageTaken.Source):
        //     stacks:1
        //     0:AB_Frost_ColdSnap_Shield PrefabGuid(-557933420)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     spellModSource:PrefabGuid(0)
        //      ----
        //     stacks:1
        //     0:AB_Frost_ColdSnap_ImmaterialBuff PrefabGuid(2082543045)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     spellModSource:PrefabGuid(0)
        //      ----
        //     stacks:1
        //     0:Frost_Vampire_Buff_Freeze PrefabGuid(612319955)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     spellModSource:PrefabGuid(0)
        //      ----
        //     stacks:1
        //     0:Frost_Vampire_Buff_Freeze PrefabGuid(612319955)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     spellModSource:PrefabGuid(0)
        //      ----
        //     stacks:1
        //     0:AB_Frost_Shared_SpellMod_FrostWeapon_Buff PrefabGuid(-1510452092)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     spellModSource:PrefabGuid(0)
        //      ----
        //     stacks:1
        //     0:Frost_Vampire_Buff_NoFreeze_Shared_DamageTrigger PrefabGuid(312176353)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     spellModSource:PrefabGuid(0)
        //      ----
        //   Buffs to apply on gameplay events (attacker):
        //     stacks:1
        //     0:Buff_CombatStance PrefabGuid(-952067173)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     spellModSource:PrefabGuid(0)
        //      ----
        //     stacks:1
        //     0:Buff_OutOfCombat PrefabGuid(897325455)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     spellModSource:PrefabGuid(0)
        //      ----
        //   attacker's buffs:
        //     AscendancyPassive_Illusion_T03_IllusionMastery PrefabGuid(522943404)
        //     SetBonus_Veil_PhysCrit_T09 PrefabGuid(-45464851)
        //     Buff_VBlood_Ability_Replace PrefabGuid(1171608023)
        //     Buff_VBlood_Ability_Replace PrefabGuid(1171608023)
        //     SpellPassive_Blood_T01_BloodSpray PrefabGuid(-1027845865)
        //     SpellPassive_Blood_T03_VBloodSlayer PrefabGuid(896859617)
        //     SpellPassive_Storm_T04_TurbulentVelocity PrefabGuid(-1148833103)
        //     SpellPassive_Illusion_T01_SpiritualInfusion PrefabGuid(-204224143)
        //     SpellPassive_Illusion_T02_FlowingSorcery PrefabGuid(-1979168975)
        //     Buff_VBlood_Ability_Replace PrefabGuid(1171608023)
        //     AscendancyPassive_Storm_T03_StormMastery PrefabGuid(-655574135)
        //     EquipBuff_Chest_Base PrefabGuid(1872694456)
        //     EquipBuff_Weapon_Reaper_Ability03 PrefabGuid(-244154805)
        //     AscendancyPassive_Chaos_T03_ChaosMastery PrefabGuid(-974227542)
        //     EquipBuff_Boots_Base PrefabGuid(-1465458722)
        //     EquipBuff_Legs_Base PrefabGuid(1971020070)
        //     EquipBuff_Gloves_Base PrefabGuid(541298575)
        //     EquipBuff_Bag_Base PrefabGuid(-783958722)
        //     SetBonus_MovementSpeed_T09 PrefabGuid(-252942383)
        //     SetBonus_VeilCooldownRecovery_T09 PrefabGuid(-1689852255)
        //     AscendancyPassive_Chaos_T02_UltimatePower PrefabGuid(-463084646)
        //     AscendancyPassive_Chaos_T01_VeilCooldown PrefabGuid(-1314793423)
        //     AscendancyPassive_Unholy_T02_FeedCooldown PrefabGuid(1178142107)
        //     AscendancyPassive_Storm_T02_MountSpeed PrefabGuid(399289260)
        //     AscendancyPassive_Storm_T01_AttackSpeed PrefabGuid(1805361793)
        //     AscendancyPassive_Unholy_T03_SkeletonMastery PrefabGuid(-392743276)
        //     AscendancyPassive_Illusion_T02_ShapeshiftSpeed PrefabGuid(-849582362)
        //     AscendancyPassive_Illusion_T01_SpellCooldown PrefabGuid(1380208342)
        //     AscendancyPassive_Frost_T01_ShieldEfficiency PrefabGuid(1401357351)
        //     AscendancyPassive_Frost_T02_AllResist PrefabGuid(1943795419)
        //     AscendancyPassive_Frost_T03_FrostMastery PrefabGuid(-1877359740)
        //     AscendancyPassive_Blood_T01_BloodMend PrefabGuid(254413498)
        //     AscendancyPassive_Blood_T02_BloodDrain PrefabGuid(762152014)
        //     AscendancyPassive_Blood_T03_LeechMastery PrefabGuid(980613835)
        //     AscendancyPassive_Unholy_T01_HealthRegen PrefabGuid(-2030466757)
        //     Buff_General_Haste_TurbulentVelocity PrefabGuid(415557996)
        //     Buff_CombatStance PrefabGuid(-952067173)
        //     Buff_InCombat PrefabGuid(581443919)
        //     AB_Frost_ColdSnap_ImmaterialBuff PrefabGuid(2082543045)
        //     AB_Frost_ColdSnap_Shield PrefabGuid(-557933420)
        //     AB_Frost_Shared_SpellMod_FrostWeapon_Buff PrefabGuid(-1510452092)
        //   spell mods:
        //     count:1
        //     0:SpellMod_Shared_FrostWeapon PrefabGuid(1222918506)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     4:GUID Not Found 0
        //     5:GUID Not Found 0
        //     6:GUID Not Found 0
        //     7:GUID Not Found 0
        #endregion


        #region debug info from hitting a frost dash (with reaper auto) after cold snap
        // interestingly, different attacker buffs appear when inspecting the reaper auto event, vs inspecting the frost weapon event
        // It appears that the frost weapon damage event happens a frame after the reaper auto event?

        // DamageTakenEvent from inspection#55 ---------------------------------------
        // damage taken from source with prefab guid: AB_Vampire_Reaper_Primary_MeleeAttack_Hit01 PrefabGuid(-1766314654)
        //   gameplay events to create on hit:
        //     EventId:-1763838605, Local
        //     EventId:-1328798647, Local
        //   Buffs to apply on gameplay events (attacker):
        //     stacks:1
        //     0:Buff_CombatStance PrefabGuid(-952067173)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     spellModSource:PrefabGuid(0)
        //      ----
        //     stacks:1
        //     0:Buff_OutOfCombat PrefabGuid(897325455)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     spellModSource:PrefabGuid(0)
        //      ----
        //   attacker's buffs:
        //     AscendancyPassive_Illusion_T03_IllusionMastery PrefabGuid(522943404)
        //     SetBonus_Veil_PhysCrit_T09 PrefabGuid(-45464851)
        //     Buff_VBlood_Ability_Replace PrefabGuid(1171608023)
        //     Buff_VBlood_Ability_Replace PrefabGuid(1171608023)
        //     SpellPassive_Blood_T01_BloodSpray PrefabGuid(-1027845865)
        //     SpellPassive_Blood_T03_VBloodSlayer PrefabGuid(896859617)
        //     SpellPassive_Storm_T04_TurbulentVelocity PrefabGuid(-1148833103)
        //     SpellPassive_Illusion_T01_SpiritualInfusion PrefabGuid(-204224143)
        //     SpellPassive_Illusion_T02_FlowingSorcery PrefabGuid(-1979168975)
        //     Buff_VBlood_Ability_Replace PrefabGuid(1171608023)
        //     AscendancyPassive_Storm_T03_StormMastery PrefabGuid(-655574135)
        //     EquipBuff_Chest_Base PrefabGuid(1872694456)
        //     EquipBuff_Weapon_Reaper_Ability03 PrefabGuid(-244154805)
        //     AscendancyPassive_Chaos_T03_ChaosMastery PrefabGuid(-974227542)
        //     EquipBuff_Boots_Base PrefabGuid(-1465458722)
        //     EquipBuff_Legs_Base PrefabGuid(1971020070)
        //     EquipBuff_Gloves_Base PrefabGuid(541298575)
        //     EquipBuff_Bag_Base PrefabGuid(-783958722)
        //     SetBonus_MovementSpeed_T09 PrefabGuid(-252942383)
        //     SetBonus_VeilCooldownRecovery_T09 PrefabGuid(-1689852255)
        //     AscendancyPassive_Chaos_T02_UltimatePower PrefabGuid(-463084646)
        //     AscendancyPassive_Chaos_T01_VeilCooldown PrefabGuid(-1314793423)
        //     AscendancyPassive_Unholy_T02_FeedCooldown PrefabGuid(1178142107)
        //     AscendancyPassive_Storm_T02_MountSpeed PrefabGuid(399289260)
        //     AscendancyPassive_Storm_T01_AttackSpeed PrefabGuid(1805361793)
        //     AscendancyPassive_Unholy_T03_SkeletonMastery PrefabGuid(-392743276)
        //     AscendancyPassive_Illusion_T02_ShapeshiftSpeed PrefabGuid(-849582362)
        //     AscendancyPassive_Illusion_T01_SpellCooldown PrefabGuid(1380208342)
        //     AscendancyPassive_Frost_T01_ShieldEfficiency PrefabGuid(1401357351)
        //     AscendancyPassive_Frost_T02_AllResist PrefabGuid(1943795419)
        //     AscendancyPassive_Frost_T03_FrostMastery PrefabGuid(-1877359740)
        //     AscendancyPassive_Blood_T01_BloodMend PrefabGuid(254413498)
        //     AscendancyPassive_Blood_T02_BloodDrain PrefabGuid(762152014)
        //     AscendancyPassive_Blood_T03_LeechMastery PrefabGuid(980613835)
        //     AscendancyPassive_Unholy_T01_HealthRegen PrefabGuid(-2030466757)
        //     Buff_General_Haste_TurbulentVelocity PrefabGuid(415557996)
        //     Buff_InCombat PrefabGuid(581443919)
        //     Buff_CombatStance PrefabGuid(-952067173)
        //     AB_Frost_ColdSnap_Shield PrefabGuid(-557933420)
        //     AB_Frost_Shared_SpellMod_FrostWeapon_Buff PrefabGuid(-1510452092)
        //     AB_Vampire_VeilOfFrost_Buff PrefabGuid(-879243806)
        //     Buff_PhysCrit_Movement_After_Veil PrefabGuid(-1045175171)
        //     Buff_General_Haste_TurbulentVelocity PrefabGuid(415557996)
        //     Buff_InCombat PrefabGuid(581443919)

        // DamageTakenEvent from inspection#56 ---------------------------------------
        // damage taken from source with prefab guid: AB_Frost_Shared_SpellMod_FrostWeapon_Buff PrefabGuid(-1510452092)
        //   Buffs to apply on gameplay events (damageTaken.Source):
        //     stacks:1
        //     0:Frost_Vampire_Buff_Chill PrefabGuid(27300215)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     spellModSource:PrefabGuid(0)
        //      ----
        //   Buffs to apply on gameplay events (attacker):
        //     stacks:1
        //     0:Buff_CombatStance PrefabGuid(-952067173)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     spellModSource:PrefabGuid(0)
        //      ----
        //     stacks:1
        //     0:Buff_OutOfCombat PrefabGuid(897325455)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     spellModSource:PrefabGuid(0)
        //      ----
        //   attacker's buffs:
        //     AscendancyPassive_Illusion_T03_IllusionMastery PrefabGuid(522943404)
        //     SetBonus_Veil_PhysCrit_T09 PrefabGuid(-45464851)
        //     Buff_VBlood_Ability_Replace PrefabGuid(1171608023)
        //     Buff_VBlood_Ability_Replace PrefabGuid(1171608023)
        //     SpellPassive_Blood_T01_BloodSpray PrefabGuid(-1027845865)
        //     SpellPassive_Blood_T03_VBloodSlayer PrefabGuid(896859617)
        //     SpellPassive_Storm_T04_TurbulentVelocity PrefabGuid(-1148833103)
        //     SpellPassive_Illusion_T01_SpiritualInfusion PrefabGuid(-204224143)
        //     SpellPassive_Illusion_T02_FlowingSorcery PrefabGuid(-1979168975)
        //     Buff_VBlood_Ability_Replace PrefabGuid(1171608023)
        //     AscendancyPassive_Storm_T03_StormMastery PrefabGuid(-655574135)
        //     EquipBuff_Chest_Base PrefabGuid(1872694456)
        //     EquipBuff_Weapon_Reaper_Ability03 PrefabGuid(-244154805)
        //     AscendancyPassive_Chaos_T03_ChaosMastery PrefabGuid(-974227542)
        //     EquipBuff_Boots_Base PrefabGuid(-1465458722)
        //     EquipBuff_Legs_Base PrefabGuid(1971020070)
        //     EquipBuff_Gloves_Base PrefabGuid(541298575)
        //     EquipBuff_Bag_Base PrefabGuid(-783958722)
        //     SetBonus_MovementSpeed_T09 PrefabGuid(-252942383)
        //     SetBonus_VeilCooldownRecovery_T09 PrefabGuid(-1689852255)
        //     AscendancyPassive_Chaos_T02_UltimatePower PrefabGuid(-463084646)
        //     AscendancyPassive_Chaos_T01_VeilCooldown PrefabGuid(-1314793423)
        //     AscendancyPassive_Unholy_T02_FeedCooldown PrefabGuid(1178142107)
        //     AscendancyPassive_Storm_T02_MountSpeed PrefabGuid(399289260)
        //     AscendancyPassive_Storm_T01_AttackSpeed PrefabGuid(1805361793)
        //     AscendancyPassive_Unholy_T03_SkeletonMastery PrefabGuid(-392743276)
        //     AscendancyPassive_Illusion_T02_ShapeshiftSpeed PrefabGuid(-849582362)
        //     AscendancyPassive_Illusion_T01_SpellCooldown PrefabGuid(1380208342)
        //     AscendancyPassive_Frost_T01_ShieldEfficiency PrefabGuid(1401357351)
        //     AscendancyPassive_Frost_T02_AllResist PrefabGuid(1943795419)
        //     AscendancyPassive_Frost_T03_FrostMastery PrefabGuid(-1877359740)
        //     AscendancyPassive_Blood_T01_BloodMend PrefabGuid(254413498)
        //     AscendancyPassive_Blood_T02_BloodDrain PrefabGuid(762152014)
        //     AscendancyPassive_Blood_T03_LeechMastery PrefabGuid(980613835)
        //     AscendancyPassive_Unholy_T01_HealthRegen PrefabGuid(-2030466757)
        //     Buff_InCombat PrefabGuid(581443919)
        //     Buff_General_Haste_TurbulentVelocity PrefabGuid(415557996)
        //     Buff_CombatStance PrefabGuid(-952067173)
        //     AB_Frost_ColdSnap_Shield PrefabGuid(-557933420)
        //     Buff_PhysCrit_Movement_After_Veil PrefabGuid(-1045175171)
        //     AB_Vampire_VeilOfFrost_Shield PrefabGuid(1539416513)
        //   spell mods:
        //     count:1
        //     0:SpellMod_Shared_FrostWeapon PrefabGuid(1222918506)
        //     1:GUID Not Found 0
        //     2:GUID Not Found 0
        //     3:GUID Not Found 0
        //     4:GUID Not Found 0
        //     5:GUID Not Found 0
        //     6:GUID Not Found 0
        //     7:GUID Not Found 0
        #endregion


        // Noteworthy Buffs to lookout for:
        // SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack	-292495274
        // SpellMod_Shared_FrostWeapon	1222918506
        // AB_Frost_FrostWeapon_Buff	620130895
        // AB_Frost_Shared_SpellMod_FrostWeapon_Buff	-1510452092
        // Frost_Vampire_Buff_Chill	27300215
        // AB_Vampire_VeilOfFrost_AoE	569397628
        // AB_Vampire_VeilOfFrost_Buff	-879243806
        // AB_Vampire_VeilOfFrost_TriggerBonusEffects	-1688602321
    }

    public static string LookupPrefabName(PrefabGUID prefabGuid)
    {
        var prefabCollectionSystem = WorldUtil.Game.GetExistingSystemManaged<PrefabCollectionSystem>();
        var prefabLookupMap = prefabCollectionSystem._PrefabLookupMap;
        if (prefabLookupMap.GuidToEntityMap.ContainsKey(prefabGuid))
        {
            return $"{prefabLookupMap.GetName(prefabGuid)} PrefabGuid({prefabGuid.GuidHash})";
        }
        return $"GUID Not Found {prefabGuid._Value}";
    }
    
}