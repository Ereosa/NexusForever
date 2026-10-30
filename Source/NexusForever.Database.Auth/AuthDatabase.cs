using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Auth.Model;
using NexusForever.Database.Configuration.Model;
using NLog;

namespace NexusForever.Database.Auth
{
    [Database(DatabaseType.Auth)]
    public class AuthDatabase : IDatabase
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private IConnectionString config;

        public void Initialise(IConnectionString connectionString)
        {
            config = connectionString;
        }

        public async Task Save(Action<AuthContext> action)
        {
            using var context = new AuthContext(config);
            action.Invoke(context);
            await context.SaveChangesAsync();
        }

        public void Migrate()
        {
            using var context = new AuthContext(config);

            List<string> migrations = context.Database.GetPendingMigrations().ToList();
            if (migrations.Count > 0)
            {
                log.Info($"Applying {migrations.Count} authentication database migration(s)...");
                foreach (string migration in migrations)
                    log.Info(migration);

                context.Database.Migrate();
            }
        }

        /// <summary>
        /// Selects an <see cref="AccountModel"/> asynchronously that matches the supplied email.
        /// </summary>
        public async Task<AccountModel> GetAccountByEmailAsync(string email)
        {
            using var context = new AuthContext(config);
            return await context.Account.SingleOrDefaultAsync(a => a.Email == email);
        }

        /// <summary>
        /// Selects an <see cref="AccountModel"/> asynchronously that matches the supplied email and game token.
        /// </summary>
        public async Task<AccountModel> GetAccountByGameTokenAsync(string email, string gameToken)
        {
            using var context = new AuthContext(config);
            return await context.Account
                .Include(a => a.AccountSuspension)
                .SingleOrDefaultAsync(a => a.Email == email && a.GameToken == gameToken);
        }

        /// <summary>
        /// Selects an <see cref="AccountModel"/> asynchronously that matches the supplied email and session key.
        /// </summary>
        public async Task<AccountModel> GetAccountBySessionKeyAsync(string email, string sessionKey)
        {
            using var context = new AuthContext(config);
            return await context.Account
                .AsSplitQuery()
                .Include(a => a.AccountCostumeUnlock)
                .Include(a => a.AccountCurrency)
                .Include(a => a.AccountGenericUnlock)
                .Include(a => a.AccountDailyLogin)
                .Include(a => a.AccountItem)
                .Include(a => a.AccountItemClaim)
                .Include(a => a.AccountItemCooldown)
                .Include(a => a.AccountKeybinding)
                .Include(a => a.AccountEntitlement)
                .Include(a => a.AccountPermission)
                .Include(a => a.AccountRole)
                .SingleOrDefaultAsync(a => a.Email == email && a.SessionKey == sessionKey);
        }

        /// <summary>
        /// Selects an <see cref="AccountModel"/> asynchronously that matches the supplied external reference type and value.
        /// </summary>
        public async Task<List<AccountModel>> GetAccountsByExternalReference(string referenceType, string referenceValue)
        {
            using var context = new AuthContext(config);
            return await context.Account
                .Include(a => a.AccountExternalReference)
                .Where(a => a.AccountExternalReference.Any(r => r.Type == referenceType && r.Value == referenceValue))
                .ToListAsync();
        }

        /// <summary>
        /// Returns if an account with the given username already exists.
        /// </summary>
        public bool AccountExists(string email)
        {
            using var context = new AuthContext(config);
            return context.Account.SingleOrDefault(a => a.Email == email) != null;
        }

        /// <summary>
        /// Create a new account with the supplied email, salt and password verifier that is inserted into the database.
        /// </summary>
        public void CreateAccount(string email, string s, string v, uint role)
        {
            if (AccountExists(email))
                throw new InvalidOperationException($"Account with that username already exists.");

            CreateAccountAsync(email, s, v, role, []).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Account currencies a new account starts with: 500 Fortune Coins, 100,000 OmniBits and 100,000 Protobucks.
        /// </summary>
        private static readonly (byte CurrencyId, ulong Amount)[] StartingCurrencies = [(5, 500ul), (6, 100000ul), (11, 100000ul)];

        /// <summary>
        /// Create a new account with the supplied email, salt and password verifier and external references that is inserted into the database.
        /// </summary>
        public async Task CreateAccountAsync(string email, string s, string v, uint role, IEnumerable<(string referenceType, string referenceValue)> externalReferences)
        {
            using var context = new AuthContext(config);

            var model = new AccountModel
            {
                Email = email,
                S     = s,
                V     = v
            };

            model.AccountRole.Add(new AccountRoleModel
            {
                RoleId = role
            });

            // every account starts with what the first players got (web signup grants the same)
            foreach ((byte currencyId, ulong amount) in StartingCurrencies)
            {
                model.AccountCurrency.Add(new AccountCurrencyModel
                {
                    CurrencyId = currencyId,
                    Amount     = amount
                });
            }

            foreach ((string referenceType, string referenceValue) in externalReferences)
            {
                model.AccountExternalReference.Add(new AccountExternalReferenceModel
                {
                    Type  = referenceType,
                    Value = referenceValue,
                });
            }

            context.Account.Add(model);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Delete an existing account with the supplied email.
        /// </summary>
        public bool DeleteAccount(string email)
        {
            using var context = new AuthContext(config);
            AccountModel account = context.Account.SingleOrDefault(a => a.Email == email);
            if (account == null)
                return false;

            context.Account.Remove(account);
            return context.SaveChanges() > 0;
        }

        /// <summary>
        /// Update <see cref="AccountModel"/> with supplied game token asynchronously.
        /// </summary>
        public async Task UpdateAccountGameToken(uint accountId, string gameToken)
        {
            using var context = new AuthContext(config);
            EntityEntry<AccountModel> entity = context.Attach(new AccountModel
            {
                Id        = accountId,
                GameToken = gameToken
            });
            entity.Property(p => p.GameToken).IsModified = true;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update <see cref="AccountModel"/> with supplied session key asynchronously.
        /// </summary>
        public async Task UpdateAccountSessionKey(uint accountId, string sessionKey)
        {
            await using var context = new AuthContext(config);
            EntityEntry<AccountModel> entity = context.Attach(new AccountModel
            {
                Id         = accountId,
                SessionKey = sessionKey
            });
            entity.Property(p => p.SessionKey).IsModified = true;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update <see cref="AccountModel"/> with supplied salt and verifier asynchronously.
        /// </summary>
        public async Task UpdateAccountPasswordSaltAndVerifier(uint accountId, string s, string v)
        {
            await using var context = new AuthContext(config);
            EntityEntry<AccountModel> entity = context.Attach(new AccountModel
            {
                Id = accountId,
                S  = s,
                V  = v
            });

            entity.Property(p => p.S).IsModified = true;
            entity.Property(p => p.V).IsModified = true;

            await context.SaveChangesAsync();
        }

        public ImmutableList<ServerModel> GetServers()
        {
            using var context = new AuthContext(config);
            return context.Server
                .AsNoTracking()
                .ToImmutableList();
        }

        public ServerModel GetServer(ushort realmId)
        {
            using var context = new AuthContext(config);
            return context.Server
                .AsNoTracking()
                .SingleOrDefault(s => s.Id == realmId);
        }

        public ImmutableList<ServerMessageModel> GetServerMessages()
        {
            using var context = new AuthContext(config);
            return context.ServerMessage
                .AsNoTracking()
                .ToImmutableList();
        }

        public ImmutableList<PermissionModel> GetPermissions()
        {
            using var context = new AuthContext(config);
            return context.Permission
                .AsNoTracking()
                .ToImmutableList();
        }

        public ImmutableList<RoleModel> GetRoles()
        {
            using var context = new AuthContext(config);
            return context.Role
                .Include(r => r.RolePermission)
                .AsNoTracking()
                .ToImmutableList();
        }

        public void BanAccount(uint accountId, string reason, DateTime? endTime)
        {
            using var context = new AuthContext(config);
            context.AccountSuspension.Add(new AccountSuspensionModel
            {
                Id      = accountId,
                Reason  = reason,
                EndTime = endTime,
            });

            context.SaveChanges();
        }

        /// <summary>
        /// Insert a player bug report into the report website's player_report table, returns the new report id or 0 if it could not be saved.
        /// </summary>
        public async Task<uint> AddPlayerReportAsync(PlayerReportModel report)
        {
            try
            {
                using var context = new AuthContext(config);
                DbConnection connection = context.Database.GetDbConnection();
                await connection.OpenAsync();

                using DbCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO player_report (category, summary, description, zone, accountId, email, characterId, characterName, " +
                    "level, class, worldId, worldZoneId, locationX, locationY, locationZ, ip) VALUES (@category, @summary, @description, @zone, " +
                    "@accountId, @email, @characterId, @characterName, @level, @class, @worldId, @worldZoneId, @locationX, @locationY, @locationZ, @ip); " +
                    "SELECT LAST_INSERT_ID();";
                AddParameter(command, "@category", report.Category);
                AddParameter(command, "@summary", report.Summary);
                AddParameter(command, "@description", report.Description);
                AddParameter(command, "@zone", report.Zone);
                AddParameter(command, "@accountId", report.AccountId);
                AddParameter(command, "@email", report.Email);
                AddParameter(command, "@characterId", report.CharacterId);
                AddParameter(command, "@characterName", report.CharacterName);
                AddParameter(command, "@level", Math.Min(report.Level, byte.MaxValue));
                AddParameter(command, "@class", report.Class);
                AddParameter(command, "@worldId", report.WorldId);
                AddParameter(command, "@worldZoneId", report.WorldZoneId);
                AddParameter(command, "@locationX", report.LocationX);
                AddParameter(command, "@locationY", report.LocationY);
                AddParameter(command, "@locationZ", report.LocationZ);
                AddParameter(command, "@ip", report.Ip ?? "");

                return Convert.ToUInt32(await command.ExecuteScalarAsync());
            }
            catch (Exception exception)
            {
                log.Error(exception, $"Failed to save player report from account {report.AccountId}!");
                return 0u;
            }
        }

        private static void AddParameter(DbCommand command, string name, object value)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value         = value;
            command.Parameters.Add(parameter);
        }
    }
}
