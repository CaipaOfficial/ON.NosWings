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

using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Mock
{
    public class EquipmentOptionDAO : SynchronizableBaseDAO<EquipmentOptionDTO>, IEquipmentOptionDAO
    {
        #region Methods

        public IEnumerable<EquipmentOptionDTO> GetOptionsByWearableInstanceId(Guid inventoryitemId)
        {
            throw new NotImplementedException();
        }

        public DeleteResult DeleteByWearableInstanceId(Guid wearableInstanceId)
        {
            throw new NotImplementedException();
        }

        #endregion

        public DeleteResult Delete(IEnumerable<Guid> id)
        {
            throw new NotImplementedException();
        }
    }
}