notes about how other mods do things

# Bloodcraft
https://github.com/mfoltz/Bloodcraft

## professions, changing stats of instanced equipment
this changes crafted gear attributes by a factor, depending on the user's profession level.
- max durability
- all stat lines
```
public static void ApplyPlayerEquipmentStats(ulong steamId, Entity equipmentEntity)
{
    IProfession handler = ProfessionFactory.GetProfession(equipmentEntity.GetPrefabGuid());

    int professionLevel = handler.GetProfessionData(steamId).Key;
    float scaledBonus = 0f;

    if (equipmentEntity.Has<Durability>())
    {
        scaledBonus = CalculateDurabilityBonus(professionLevel);

        equipmentEntity.With((ref Durability durability) =>
        {
            durability.MaxDurability *= scaledBonus;
            durability.Value = durability.MaxDurability;
        });
    }

    if (!ServerGameManager.TryGetBuffer<ModifyUnitStatBuff_DOTS>(equipmentEntity, out var buffer)) return;

    scaledBonus = CalculateStatBonus(handler, professionLevel);

    for (int i = 0; i < buffer.Length; i++)
    {
        ModifyUnitStatBuff_DOTS statBuff = buffer[i];
        if (statBuff.StatType.Equals(UnitStatType.InventorySlots)) continue;

        statBuff.Value *= scaledBonus;
        buffer[i] = statBuff;
    }
}

```

Core
```
public static ServerGameManager ServerGameManager => SystemService.ServerScriptMapper.GetServerGameManager();
public static SystemService SystemService { get; } = new(Server);
```

SystemService
```
ServerScriptMapper _serverScriptMapper;
public ServerScriptMapper ServerScriptMapper => _serverScriptMapper ??= GetSystem<ServerScriptMapper>();
```

## bleeding edge, changing stats of weapon abilities

Core
```
public static IReadOnlySet<WeaponType> BleedingEdge => _bleedingEdge;


if (BleedingEdge.Any())
{
    if (BleedingEdge.Contains(WeaponType.Slashers))
    {
        if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(Buffs.VargulfBleedBuff, out Entity bleedBuffPrefab))
        {
            bleedBuffPrefab.With((ref Buff buff) =>
            {
                buff.MaxStacks = BLEED_STACKS;
                buff.IncreaseStacks = true;
            });
        }
    }

    if (BleedingEdge.Contains(WeaponType.Crossbow) || BleedingEdge.Contains(WeaponType.Pistols))
    {
        ComponentType[] _projectileAllComponents =
        [
            ComponentType.ReadOnly(Il2CppType.Of<PrefabGUID>()),
            ComponentType.ReadOnly(Il2CppType.Of<Projectile>()),
            ComponentType.ReadOnly(Il2CppType.Of<LifeTime>()),
            ComponentType.ReadOnly(Il2CppType.Of<Velocity>())
        ];

        QueryDesc projectileQueryDesc = EntityManager.CreateQueryDesc(_projectileAllComponents, typeIndices: [0], options: EntityQueryOptions.IncludeAll);
        BleedingEdgePrimaryProjectileRoutine(projectileQueryDesc).Start();
    }
}


static IEnumerator BleedingEdgePrimaryProjectileRoutine(QueryDesc projectileQueryDesc)
{
    bool pistols = BleedingEdge.Contains(WeaponType.Pistols);
    bool crossbow = BleedingEdge.Contains(WeaponType.Crossbow);

    yield return QueryResultStreamAsync(
        projectileQueryDesc,
        stream =>
        {
            try
            {
                using (stream)
                {
                    foreach (QueryResult result in stream.GetResults())
                    {
                        Entity entity = result.Entity;
                        PrefabGUID prefabGuid = result.ResolveComponentData<PrefabGUID>();
                        string prefabName = prefabGuid.GetPrefabName();

                        if (pistols && IsWeaponPrimaryProjectile(prefabName, WeaponType.Pistols))
                        {
                            // Log.LogWarning($"[BleedingEdgePrimaryProjectileRoutine] - editing {prefabName}");
                            entity.With((ref Projectile projectile) =>
                            {
                                projectile.Range *= 1.25f;
                            });
                            entity.HasWith((ref LifeTime lifeTime) =>
                            {
                                lifeTime.Duration *= 1.25f;
                            });
                        }
                        else if (crossbow && IsWeaponPrimaryProjectile(prefabName, WeaponType.Crossbow))
                        {
                            // Log.LogWarning($"[BleedingEdgePrimaryProjectileRoutine] - editing {prefabName}");
                            entity.With((ref Projectile projectile) =>
                            {
                                projectile.Speed = 100f;
                            });
                        }
                        else
                        {
                            // Log.LogWarning($"[BleedingEdgePrimaryProjectileRoutine] - {prefabName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[BleedingEdgePrimaryProjectileRoutine] - {ex}");
            }
        }
    );
}

static bool IsWeaponPrimaryProjectile(string prefabName, WeaponType weaponType)
{
    return prefabName.ContainsAll([weaponType.ToString(), "Primary", "Projectile"]);
}

```


EntityQueries util https://github.com/mfoltz/Bloodcraft/blob/main/Utilities/EntityQueries.cs

Buff-related extensions for Entity: https://github.com/mfoltz/Bloodcraft/blob/main/Utilities/Buffs.cs

### patches
stat changes https://github.com/mfoltz/Bloodcraft/blob/main/Patches/StatChangeSystemPatch.cs#L55