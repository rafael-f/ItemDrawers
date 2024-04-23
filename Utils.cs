using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace kg_ItemDrawers;

public static class Utils
{
    public static string Localize(this string s) => Localization.instance.Localize(s);

    public static int CustomCountItems(string prefab, int level)
    {
        int num = 0;

        var traverse = Traverse.Create(Player.m_localPlayer);
        Inventory m_inventory = traverse.Field("m_inventory").GetValue<Inventory>();

        var traverse2 = Traverse.Create(m_inventory);
        List<ItemDrop.ItemData> m_inventory2 = traverse2.Field("m_inventory").GetValue<List<ItemDrop.ItemData>>();

        foreach (ItemDrop.ItemData itemData in m_inventory2)
        {
            if (itemData.m_dropPrefab.name == prefab && level == itemData.m_quality)
            {
                num += itemData.m_stack;
            }
        }

        return num;
    }

    public static void CustomRemoveItems(string prefab, int amount, int level)
    {
        var traverse = Traverse.Create(Player.m_localPlayer);
        Inventory m_inventory = traverse.Field("m_inventory").GetValue<Inventory>();

        var traverse2 = Traverse.Create(m_inventory);
        List<ItemDrop.ItemData> m_inventory2 = traverse2.Field("m_inventory").GetValue<List<ItemDrop.ItemData>>();

        foreach (ItemDrop.ItemData itemData in m_inventory2)
        {
            if (itemData.m_dropPrefab.name == prefab && itemData.m_quality == level)
            {
                int num = Mathf.Min(itemData.m_stack, amount);
                itemData.m_stack -= num;
                amount -= num;
                if (amount <= 0)
                    break;
            }
        }

        m_inventory2.RemoveAll(x => x.m_stack <= 0);

        var traverse3 = Traverse.Create(m_inventory);
        traverse3.Method("Changed");
    }

    public static void InstantiateItem(GameObject prefab, int stack, int level)
    {
        Player p = Player.m_localPlayer;
        if (!p || !prefab) return;
        if (prefab.GetComponent<ItemDrop>() is not { } item) return;

        var traverse = Traverse.Create(p);
        Inventory m_inventory = traverse.Field("m_inventory").GetValue<Inventory>();

        if (item.m_itemData.m_shared.m_maxStackSize > 1)
        {
            while (stack > 0)
            {
                int addStack = Math.Min(stack, item.m_itemData.m_shared.m_maxStackSize);
                stack -= addStack;
                ItemDrop itemDrop = Object.Instantiate(prefab, p.transform.position + Vector3.up * 1.5f, Quaternion.identity).GetComponent<ItemDrop>();
                itemDrop.m_itemData.m_stack = addStack;
                itemDrop.m_itemData.m_durability = item.m_itemData.GetMaxDurability();

                var traverse2 = Traverse.Create(itemDrop);
                traverse2.Method("Save");

                if (m_inventory.CanAddItem(itemDrop.gameObject))
                {
                    m_inventory.AddItem(itemDrop.m_itemData);
                    ZNetScene.instance.Destroy(itemDrop.gameObject);
                }
            }
        }
        else
        {
            for (int i = 0; i < stack; ++i)
            {
                GameObject go = Object.Instantiate(prefab, p.transform.position + Vector3.up * 1.5f, Quaternion.identity);
                ItemDrop itemDrop = go.GetComponent<ItemDrop>();
                itemDrop.m_itemData.m_quality = level;
                itemDrop.m_itemData.m_durability = itemDrop.m_itemData.GetMaxDurability();

                var traverse2 = Traverse.Create(itemDrop);
                traverse2.Method("Save");

                if (m_inventory.CanAddItem(go))
                {
                    m_inventory.AddItem(itemDrop.m_itemData);
                    ZNetScene.instance.Destroy(go);
                }
            }
        }
    }

    public static void InstantiateAtPos(GameObject prefab, int stack, int level, Vector3 pos)
    {
        Player p = Player.m_localPlayer;
        if (!p || !prefab) return;
        ItemDrop item = prefab.GetComponent<ItemDrop>();
        while (stack > 0)
        {
            int addStack = Math.Min(stack, item.m_itemData.m_shared.m_maxStackSize);
            stack -= addStack;
            ItemDrop itemDrop = Object.Instantiate(prefab, pos, Quaternion.identity).GetComponent<ItemDrop>();
            itemDrop.m_itemData.m_stack = addStack;
            float durability = item.m_itemData.GetMaxDurability();
            itemDrop.m_itemData.m_durability = durability;

            var traverse2 = Traverse.Create(itemDrop);
            traverse2.Method("Save");
        }
    }
}