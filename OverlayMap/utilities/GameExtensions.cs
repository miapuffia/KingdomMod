#if IL2CPP
using Il2CppSystem.Collections.Generic;

#else
using System.Collections.Generic;
#endif
using UnityEngine;

namespace KingdomMod
{
    public static class GameExtensions
    {
        public static T GetPayableOfType<T>() where T : Component
        {
            var payables = Managers.Inst.payables;
            if (!payables) return null;

            foreach (var obj in payables.
#if IL2CPP
                         AllPayables
#else
                         GetFieldOrPropertyValue<Payable[]>("AllPayables")
#endif
                    )
            {
                if (obj == null) continue;
                var comp = obj.GetComponent<T>();
                if (comp != null)
                    return comp;
            }

            return null;
        }

        public static List<T> GetPayablesOfType<T>() where T : Component
        {
            var result = new List<T>();
            var payables = Managers.Inst.payables;
            if (!payables) return result;

            foreach (var obj in payables.
#if IL2CPP
                         AllPayables
#else
                         GetFieldOrPropertyValue<Payable[]>("AllPayables")
#endif
                    )
            {
                if (obj == null) continue;
                var comp = obj.GetComponent<T>();
                if (comp != null)
                    result.Add(comp);
            }

            return result;
        }

        public static List<Payable> GetPayablesWithName(string name, bool contains = false) {
            var result = new List<Payable>();
            var payables = Managers.Inst.payables;
            if(!payables)
                return result;

            foreach(var obj in payables.
#if IL2CPP
                         AllPayables
#else
                         GetFieldOrPropertyValue<Payable[]>("AllPayables")
#endif
                    ) {
                if(obj == null)
                    continue;
                if(!contains && obj.name == name)
                    result.Add(obj);
                else if(contains && obj.name.Contains(name))
                    result.Add(obj);
            }

            return result;
        }

        public static T GetPayableBlockerOfType<T>() where T : Component
        {
            var payables = Managers.Inst.payables;
            if (!payables) return null;

            foreach (var obj in payables.GetFieldOrPropertyValue<List<PayableBlocker>>("_allBlockers"))
            {
                if (obj == null) continue;
                var comp = obj.GetComponent<T>();
                if (comp != null)
                    return comp;
            }

            return null;
        }

        public static List<T> FindObjectsWithTagOfType<T>(string tagName)
        {
            var list = new List<T>();
            foreach (var obj in GameObject.FindGameObjectsWithTag(tagName))
            {
                if (obj == null) continue;
                var comp = obj.GetComponent<T>();
                if (comp != null)
                    list.Add(comp);
            }

            return list;
        }

        public static List<Character> FindCharactersOfType<T>()
        {
            var list = new List<Character>();
            var kingdom = Managers.Inst.kingdom;
            if (kingdom == null) return list;
            foreach (var character in kingdom.GetFieldOrPropertyValue<HashSet<Character>>("_characters"))
            {
                if (character == null) continue;
                if (character.GetComponent<T>() != null)
                    list.Add(character);
            }

            return list;
        }

        public static int GetArcherCount(ArcherTypeEnum archerType)
        {
            var result = 0;
            foreach (var obj in Managers.Inst.kingdom.GetFieldOrPropertyValue<HashSet<Archer>>("_archers"))
            {
                if (archerType == ArcherTypeEnum.Free)
                {
                    if (!obj.inGuardSlot && !obj.isKnightSoldier)
                        result++;
                }
                else if (archerType == ArcherTypeEnum.GuardSlot)
                {
                    if (obj.inGuardSlot)
                        result++;
                }
                else if (archerType == ArcherTypeEnum.KnightSoldier)
                {
                    if (obj.isKnightSoldier)
                        result++;
                }
            }

            return result;
        }

        public static int GetKnightCount(bool needsArmor)
        {
            var knightCount = 0;
            foreach (var knight in Managers.Inst.kingdom.GetFieldOrPropertyValue<HashSet<Knight>>("_knights"))
            {
                if (knight.GetFieldOrPropertyValue<bool>("_needsArmor") == needsArmor)
                    knightCount++;
            }
            return knightCount;
        }

        public static Player GetLocalPlayer() {
            return Managers.Inst.kingdom.GetPlayer(NetworkBigBoss.HasWorldAuth ? 0 : 1);
        }
    }
}