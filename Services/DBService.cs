using MatrixIdent.Models;
using MatrixIdent.Database;
using Microsoft.EntityFrameworkCore;
using log4net;
using MatrixIdent.Controllers;

namespace MatrixIdent.Services
{
    public class DBService
    {
        private IdentDbContext _context;
        private ILog log = LogManager.GetLogger(typeof(DBService));

        public DBService(ConfigService config)
        {
            try
            {
                _context = new IdentDbContext(config);
                _context.Database.Migrate();
                _context.Database.EnsureCreated();
            }
            catch (Exception e)
            {
                log.Error(e);
            }
            
        }

        public async Task<string> saveToken(AuthItem itm)
        {
            string strtoken = Crypt.md5(itm.access_token);
            itm.token = strtoken;
             _context.AuthItems.Add(itm);
             _context.SaveChanges();
            return strtoken;
        }

        public async Task<bool> checkToken(string token)
        {
            var item = await _context.AuthItems.FindAsync(token);
            return item != null ? true : false;
        }

        public async Task<AuthItem?> getAuthItem(string token)
        {
            return await _context.AuthItems.FindAsync(token);
        }

        public async Task<string?> getAccessToken(string token)
        {
            AuthItem? itm = await _context.AuthItems.FindAsync(token);
            if (itm!=null)
            {
                return itm.access_token;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> deleteToken(string token)
        {
            var item = await _context.AuthItems.FindAsync(token);            
            if (item == null) return false;

            _context.AuthItems.Remove(item);
             _context.SaveChanges();
            return true;
        }



        /*HASHITEMS*/

        public async Task<bool> deleteHash(string token)
        {
            var item = await _context.HashItems.FindAsync(token);
            if (item == null) return false;

            _context.HashItems.Remove(item);
             _context.SaveChanges();
            return true;
        }

        public async Task<HashItem?> getHash(string token)
        {
            return await _context.HashItems.FindAsync(token);
        }


        public async Task<bool> addOrUpdateHash(string token, string lookup_pepper)
        {
            HashItem? itm = await getHash(token);
            
            if (itm==null)
            {
                itm = new HashItem();
                itm.token = token;
                itm.lookup_pepper = lookup_pepper;
                _context.HashItems.Add(itm);
                int result = _context.SaveChanges();
                return result == 1;
            }
            else
            {
                itm.lookup_pepper = lookup_pepper;
                _context.Entry(itm).State = EntityState.Modified;                
                int result = _context.SaveChanges();
                return result == 1;
            }

        }

        /*email validation*/

        public async Task<bool> deleteEmailValidationItem(string email)
        {
            var item = await _context.EmailValidationItems.FindAsync(email);
            if (item == null) return false;

            _context.EmailValidationItems.Remove(item);
             _context.SaveChanges();
            return true;
        }

        public async Task<EmailValidationRequestItem?> getEmailValidationItem(string email)
        {
            return await _context.EmailValidationItems.FindAsync(email);
        }

        public async Task<EmailValidationRequestItem?> getEmailValidationItemByTokenSidSecret(string token, string client_secret, string sid)
        {
            var query = _context.EmailValidationItems.Where(p => p.client_secret == client_secret && p.sid == sid && p.token == token);

            var result = await query.ToArrayAsync();
            return result.Length == 1 ? result[0] : null;
        }

        public async Task<EmailValidationRequestItem?> getEmailValidationItemBySecretAndSid(string client_secret, string sid)
        {
            var query = _context.EmailValidationItems.Where(p => p.client_secret == client_secret && p.sid == sid);

            var result = await query.ToArrayAsync();
            return result.Length == 1 ? result[0] : null;
        }

        public async Task<bool> setTrueEmailValidationItem(string token, string client_secret, string sid, bool val)
        {
            EmailValidationRequestItem? itm = await getEmailValidationItemByTokenSidSecret(token, client_secret, sid);

            if (itm != null)
            {
                _context.Entry(itm).State = EntityState.Modified;             
                itm.success = val;
                int result =  _context.SaveChanges();
                return result == 1;
            }

            return false;
        }


        public async Task<bool> addOrUpdateEmailValidationItem(EmailValidationRequestItem item)
        {
            EmailValidationRequestItem? itm = await getEmailValidationItem(item.email);

            if (itm == null)
            {
                 _context.EmailValidationItems.Add(item);
                int result =  _context.SaveChanges();
                return result == 1;
            }
            else
            {
                _context.Entry(itm).State = EntityState.Modified;
                itm.client_secret = item.client_secret;
                itm.next_link = item.next_link;
                itm.send_attempt = item.send_attempt;
                itm.token = item.token;
                itm.sid = item.sid;
                itm.success = item.success;
                int result =  _context.SaveChanges();
                return result == 1;
            }
        }

        /*msisdn validation*/

        public async Task<bool> deleteMsisdnValidationItem(string email)
        {
            var item = await _context.MsisdnValidationItems.FindAsync(email);
            if (item == null) return false;

            _context.MsisdnValidationItems.Remove(item);
             _context.SaveChanges();
            return true;
        }

        public async Task<MsisdnValidationRequestItem?> getMsisdnValidationItem(string phonenumber)
        {
            return await _context.MsisdnValidationItems.FindAsync(phonenumber);
        }

        public async Task<MsisdnValidationRequestItem?> getMsisdnValidationItemByTokenSidSecret(string token, string client_secret, string sid)
        {
            var query = _context.MsisdnValidationItems.Where(p => p.client_secret == client_secret && p.sid == sid && p.token == token);

            var result = await query.ToArrayAsync();
            return result.Length == 1 ? result[0] : null;
        }

        public async Task<MsisdnValidationRequestItem?> getMsisdnValidationItemBySecretAndSid(string client_secret, string sid)
        {
            var query = _context.MsisdnValidationItems.Where(p => p.client_secret == client_secret && p.sid == sid);

            var result = await query.ToArrayAsync();
            return result.Length == 1 ? result[0] : null;
        }

        public async Task<bool> setTruePhonenumberValidationItem(string token, string client_secret, string sid, bool val)
        {
            MsisdnValidationRequestItem? itm = await getMsisdnValidationItemByTokenSidSecret(token, client_secret, sid);

            if (itm != null)
            {
                _context.Entry(itm).State = EntityState.Modified;
                itm.success = val;
                int result =  _context.SaveChanges();
                return result == 1;
            }

            return false;
        }

        public async Task<bool> addOrUpdateMsisdnValidationItem(MsisdnValidationRequestItem item)
        {
            MsisdnValidationRequestItem? itm = await getMsisdnValidationItem(item.phone_number);

            if (itm == null)
            {
                 _context.MsisdnValidationItems.Add(item);
                int result =  _context.SaveChanges();
                return result == 1;
            }
            else
            {
                _context.Entry(itm).State = EntityState.Modified;
                itm.client_secret = item.client_secret;
                itm.next_link = item.next_link;
                itm.country = item.country;
                itm.send_attempt = item.send_attempt;
                itm.token = item.token;
                itm.sid = item.sid;
                int result =  _context.SaveChanges();
                return result == 1;
            }
        }

        /*3PID ASSOZIATION*/

        public async Task<bool> delete3PIDItem(string address, string mxid, string medium )
        {
            var threepiditem = await get3PidItem(address, mxid, medium);
            if (threepiditem == null) return false;

            if (medium=="email")
            {
                deleteEmailValidationItem(address);

            }
            if (medium == "msisdn")
            {
                deleteMsisdnValidationItem(address);

            }

            _context.ThreePidResponseItems.Remove(threepiditem);
             _context.SaveChanges();
            return true;
        }


        public async Task<ThreePidResponseItem?> get3PidItem(string address, string mxid, string medium)
        {
            var query = _context.ThreePidResponseItems.Where(p => p.address == address && p.mxid== mxid && p.medium == medium);

            var result = await query.ToArrayAsync();
            return result.Length == 1 ? result[0] : null;
        }

        public async Task<ThreePidResponseItem[]> get3PidItems()
        {
            var query = _context.ThreePidResponseItems.Where(p => true);

            return await query.ToArrayAsync();            
        }


        public async Task<bool> addOrUpdate3PIDItem(ThreePidResponseItem item)
        {
            ThreePidResponseItem? itm = await get3PidItem(item.address, item.mxid, item.medium);

            if (itm == null)
            {
                 _context.ThreePidResponseItems.Add(item);
                int result =  _context.SaveChanges();
                return result == 1;
            }
            else
            {
                _context.Entry(itm).State = EntityState.Modified;
                itm.not_before = item.not_before;
                itm.not_after = item.not_after;
                itm.ts = item.ts;
                int result =  _context.SaveChanges();
                return result == 1;
            }
        }

        /*INVITATION*/

        public async Task<bool> deleteInvitation(string address, string room_id, string medium, string sender)
        {
            var invitationItem = await getInvitationItem(address, room_id, medium, sender);
            if (invitationItem == null) return false;
            _context.InvitationRequestItems.Remove(invitationItem[0]);
             _context.SaveChanges();
            return true;
        }


        public async Task<InvitationRequestItem[]?> getInvitationItem(string address, string room_id, string medium, string sender)
        {
            var query = _context.InvitationRequestItems.Where(p => p.address == address && p.room_id == room_id && p.medium == medium && p.sender == sender);

            return await query.ToArrayAsync();
        }

        public async Task<InvitationRequestItem[]?> getInvitationItem(string address)
        {
            var query = _context.InvitationRequestItems.Where(p => p.address == address);

            return await query.ToArrayAsync();
        }

        public async Task<InvitationRequestItem[]?> getInvitationItemByToken(string token)
        {
            var query = _context.InvitationRequestItems.Where(p => p.token == token);

            return await query.ToArrayAsync();
        }

        public async Task<bool> addOrUpdateInvitationItem(InvitationRequestItem item)
        {
            InvitationRequestItem[]? itm = await getInvitationItem(item.address, item.room_id, item.medium, item.sender);

            if (itm == null||itm.Length==0)
            {
                 _context.InvitationRequestItems.Add(item);
                int result =  _context.SaveChanges();
                return result == 1;
            }
            else
            {
                _context.Entry(itm[0]).State = EntityState.Modified;
                itm[0].room_alias = item.room_alias;
                itm[0].room_avatar_url = item.room_avatar_url;
                itm[0].room_join_rules = item.room_join_rules;
                itm[0].room_name = item.room_name;
                itm[0].room_type = item.room_type;
                itm[0].sender_avatar_url = item.sender_avatar_url;
                itm[0].sender_display_name = item.sender_display_name;
                itm[0].token = item.token;
                itm[0].key = item.key;

                int result =  _context.SaveChanges();
                return result == 1;
            }
        }

        /*Keys*/

        public async Task<bool> deleteKeyByIdentifier(string identifier)
        {
            var item = await _context.Keys.FindAsync(identifier);
            if (item == null) return false;

            _context.Keys.Remove(item);
             _context.SaveChanges();
            return true;

        }
        public async Task<bool> deleteKey(string public_key)
        {
            var key = await getKeyFromPublicKey(public_key);
            if (key == null) return false;
            _context.Keys.Remove(key);
             _context.SaveChanges();
            return true;
        }

        public async Task<Key?> getKeyFromPublicKey(string public_key)
        {
            var query = _context.Keys.Where(p => p.public_key == public_key);

            var result = await query.ToArrayAsync();
            return result.Length == 1 ? result[0] : null;
        }

        public async Task<Key?> getKeyFromPrivateKey(string private_key)
        {
            var query = _context.Keys.Where(p => p.private_key == private_key);

            var result = await query.ToArrayAsync();
            return result.Length == 1 ? result[0] : null;
        }


        public async Task<bool> addKey(Key k)
        {            
            if (await getKeyFromPrivateKey(k.private_key) == null)
            {               
                 _context.Keys.Add(k);
                int result =  _context.SaveChanges();
                return result == 1;
            }

            return true;
        }
    }
}
