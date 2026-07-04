using Server.Models;
using System;
using System.Collections.Generic;


public class AccountData
{
    public Account account;

    public Account accountCachedData;
}

public class CacheManager
{
    private static readonly Lazy<CacheManager> lazyInstance = new Lazy<CacheManager>(() => new CacheManager());
    public static CacheManager Instance => lazyInstance.Value;

    private Dictionary<int, AccountData> accounts;

    public void InitCache()
    {
        accounts = new Dictionary<int, AccountData>();
    }

 
    //Account
    public void AddAccountData(AccountData data)
    {
        accounts[data.account.Idaccount] = data;
    }
    public AccountData GetAccountData(int accountId)
    {
        accounts.TryGetValue(accountId, out var data);
        return data;
    }
    public Dictionary<int, AccountData> GetAllAccountData()
    {
        return accounts;
    }
    public void RemoveAccountData(int accountId)
    {
        accounts.Remove(accountId);
    }
    public bool IsAccountOnline(int accountId)
    {
        return accounts.ContainsKey(accountId);
    }
    public void ClearAccounts()
    {
        accounts.Clear();
    }
}