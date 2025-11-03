using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using Newtonsoft.Json;

public class Users 
{
    public string Name { get; set; }
    public string Email { get; set; }  // Thêm field Email
    public int Gold { get; set; }
    public int Diamond { get; set; }

    public Map MapInGame { get; set; }

    public Users() 
    { 
    }

    public Users(string name, int gold, int diamond, Map mapInGame)
    {
        Name = name;
        Gold = gold;
        Diamond = diamond;
        MapInGame = mapInGame;
    }
    
    public Users(string name, string email, int gold, int diamond, Map mapInGame)
    {
        Name = name;
        Email = email;
        Gold = gold;
        Diamond = diamond;
        MapInGame = mapInGame;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
