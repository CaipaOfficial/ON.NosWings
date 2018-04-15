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
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.DAL.EF.Base;
using OpenNos.DAL.EF.Entities;

namespace OpenNos.DAL.EF
{
    public class MailDAO : MappingBaseDao<Mail, MailDTO>, IMailDAO
    {
        #region Methods

        public DeleteResult DeleteById(long mailId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                var contextRef = context;
                return DeleteById(ref contextRef, mailId);
            }
        }

        public DeleteResult DeleteById(ref OpenNosContext context, long mailId)
        {
            try
            {
                Mail mail = context.Mail.First(i => i.MailId.Equals(mailId));

                if (mail == null)
                {
                    return DeleteResult.Deleted;
                }
                context.Mail.Remove(mail);

                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref MailDTO mail)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                var contextRef = context;
                return InsertOrUpdate(ref contextRef, ref mail);
            }
        }

        public SaveResult InsertOrUpdate(ref OpenNosContext context, ref MailDTO mail)
        {
            try
            {
                long mailId = mail.MailId;
                Mail entity = context.Mail.FirstOrDefault(c => c.MailId.Equals(mailId));

                if (entity == null)
                {
                    mail = Insert(mail, context);
                    return SaveResult.Inserted;
                }

                mail.MailId = entity.MailId;
                mail = Update(entity, mail, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<MailDTO> LoadByCharacterId(long characterId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (Mail mail in context.Mail.Where(s => (!s.IsSenderCopy && s.ReceiverId == characterId) || (s.IsSenderCopy && s.SenderId == characterId)))
                {
                    yield return _mapper.Map<MailDTO>(mail);
                }
            }
        }

        public IEnumerable<MailDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (Mail mail in context.Mail)
                {
                    yield return _mapper.Map<MailDTO>(mail);
                }
            }
        }

        public MailDTO LoadById(long mailId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<MailDTO>(context.Mail.FirstOrDefault(i => i.MailId.Equals(mailId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private MailDTO Insert(MailDTO mail, OpenNosContext context)
        {
            try
            {
                Mail entity = _mapper.Map<Mail>(mail);
                context.Mail.Add(entity);
                context.SaveChanges();
                return _mapper.Map<MailDTO>(entity);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private MailDTO Update(Mail entity, MailDTO respawn, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(respawn, entity);
                context.SaveChanges();
            }
            return _mapper.Map<MailDTO>(entity);
        }

        #endregion
    }
}