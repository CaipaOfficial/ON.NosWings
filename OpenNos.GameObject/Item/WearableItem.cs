﻿/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Core;
using OpenNos.Domain;
using System;
using System.Diagnostics;

namespace OpenNos.GameObject
{
    public class WearableItem : Item
    {
        #region Methods

        public override void Use(ClientSession session, ref Inventory inventory, bool DelayUsed = false)
        {
            switch (Effect)
            {
                default:
                    short slot = inventory.Slot;
                    byte type = inventory.Type;
                    if (inventory == null) return;

                    Item iteminfo = ServerManager.GetItem(inventory.ItemInstance.ItemVNum);
                    if (iteminfo == null) return;

                    if (iteminfo.ItemValidTime > 0 && inventory.ItemInstance.IsUsed == false)
                    {
                        inventory.ItemInstance.ItemDeleteTime = DateTime.Now.AddSeconds(iteminfo.ItemValidTime);
                    }

                    if (iteminfo.EquipmentSlot == (byte)EquipmentType.Fairy)
                    {
                        if ((iteminfo.MaxElementRate == 70 || iteminfo.MaxElementRate == 80) && !DelayUsed && !inventory.ItemInstance.IsUsed && IsTradable)
                        {
                            session.Client.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{type}^{slot}^1 {Language.Instance.GetMessageFromKey("ASK_BIND")}");
                            return;
                        }
                        else
                        {
                            inventory.ItemInstance.IsUsed = true;
                        }
                    }
                    if ((iteminfo.EquipmentSlot == (byte)EquipmentType.CostumeHat || iteminfo.EquipmentSlot == (byte)EquipmentType.CostumeSuit || iteminfo.EquipmentSlot == (byte)EquipmentType.WeaponSkin) && !DelayUsed && !inventory.ItemInstance.IsUsed && IsTradable)
                    {
                        session.Client.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{type}^{slot}^1 {Language.Instance.GetMessageFromKey("ASK_BIND")}");
                        return;
                    }
                    else
                    {
                        inventory.ItemInstance.IsUsed = true;
                    }

                    double timeSpanSinceLastSpUsage = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds - session.Character.LastSp;

                    if (iteminfo.EquipmentSlot == (byte)EquipmentType.Sp && inventory.ItemInstance.Rare == -2)
                    {
                        session.Client.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_EQUIP_DESTROYED_SP"), 0));
                        return;
                    }

                    if (iteminfo.EquipmentSlot == (byte)EquipmentType.Sp && timeSpanSinceLastSpUsage <= session.Character.SpCooldown && session.Character.EquipmentList.LoadInventoryBySlotAndType((byte)EquipmentType.Sp, (byte)InventoryType.Sp) != null)
                    {
                        session.Client.SendPacket(session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"), session.Character.SpCooldown - (int)Math.Round(timeSpanSinceLastSpUsage)), 0));
                        return;
                    }
                    if ((iteminfo.ItemType != (byte)Domain.ItemType.Weapon
                         && iteminfo.ItemType != (byte)Domain.ItemType.Armor
                         && iteminfo.ItemType != (byte)Domain.ItemType.Fashion
                         && iteminfo.ItemType != (byte)Domain.ItemType.Jewelery
                         && iteminfo.ItemType != (byte)Domain.ItemType.Specialist)
                        || iteminfo.LevelMinimum > (iteminfo.IsHeroic ? session.Character.HeroLevel : session.Character.Level) || (iteminfo.Sex != 0 && iteminfo.Sex != session.Character.Gender + 1)
                        || ((iteminfo.ItemType != (byte)Domain.ItemType.Jewelery && iteminfo.EquipmentSlot != (byte)EquipmentType.Boots && iteminfo.EquipmentSlot != (byte)EquipmentType.Gloves) && ((iteminfo.Class >> session.Character.Class) & 1) != 1))
                    {
                        session.Client.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("BAD_EQUIPMENT"), 10));
                        return;
                    }

                    if (session.Character.UseSp
                        && iteminfo.EquipmentSlot == (byte)EquipmentType.Fairy
                        && iteminfo.Element != ServerManager.GetItem(
                            session.Character.EquipmentList.LoadBySlotAndType<SpecialistInstance>(
                                (byte)EquipmentType.Sp,
                                (byte)InventoryType.Equipment).ItemVNum).Element &&
                                iteminfo.Element != ServerManager.GetItem(
                            session.Character.EquipmentList.LoadBySlotAndType<SpecialistInstance>(
                                (byte)EquipmentType.Sp,
                                (byte)InventoryType.Equipment).ItemVNum).SecondaryElement)
                    {
                        session.Client.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_FAIRY"), 0));
                        return;
                    }

                    if (session.Character.UseSp && iteminfo.EquipmentSlot == (byte)EquipmentType.Sp)
                    {
                        session.Client.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SP_BLOCKED"), 10));
                        return;
                    }

                    if (session.Character.JobLevel < iteminfo.LevelJobMinimum)
                    {
                        session.Client.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("LOW_JOB_LVL"), 10));
                        return;
                    }

                    Inventory equip = session.Character.EquipmentList.LoadInventoryBySlotAndType(iteminfo.EquipmentSlot, (byte)InventoryType.Equipment);
                    if (iteminfo.EquipmentSlot == (byte)EquipmentType.Amulet)
                        session.Client.SendPacket(session.Character.GenerateEff(39));

                    if (equip == null)
                    {
                        session.Character.EquipmentList.AddToInventoryWithSlotAndType(inventory.ItemInstance as ItemInstance, (byte)InventoryType.Equipment, iteminfo.EquipmentSlot);
                        session.Client.SendPacket(session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0, 0));
                        session.Character.InventoryList.DeleteFromSlotAndType(inventory.Slot, inventory.Type);
                        session.Client.SendPacket(session.Character.GenerateStatChar());
                        session.CurrentMap?.Broadcast(session.Character.GenerateEq());
                        session.Client.SendPacket(session.Character.GenerateEquipment());
                        session.CurrentMap?.Broadcast(session.Character.GeneratePairy());
                    }
                    else
                    {
                        session.Character.InventoryList.DeleteFromSlotAndType(inventory.Slot, inventory.Type);
                        session.Character.EquipmentList.DeleteFromSlotAndType(equip.Slot, equip.Type);
                        session.Character.EquipmentList.AddToInventoryWithSlotAndType(inventory.ItemInstance as ItemInstance, (byte)InventoryType.Equipment, iteminfo.EquipmentSlot);
                        session.Character.InventoryList.AddToInventoryWithSlotAndType(equip.ItemInstance as ItemInstance, type, slot);

                        session.Client.SendPacket(session.Character.GenerateStatChar());
                        session.CurrentMap?.Broadcast(session.Character.GenerateEq());
                        session.Client.SendPacket(session.Character.GenerateEquipment());
                        session.CurrentMap?.Broadcast(session.Character.GeneratePairy());
                    }

                    if (iteminfo.EquipmentSlot == (byte)EquipmentType.Fairy)
                    {
                        WearableInstance fairy = session.Character.EquipmentList.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, (byte)InventoryType.Equipment);
                        session.Client.SendPacket(session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("FAIRYSTATS"), fairy.XP, ServersData.LoadFairyXpData((fairy.ElementRate + fairy.Item.ElementRate))), 10));
                    }
                    break;
            }
        }

        #endregion
    }
}