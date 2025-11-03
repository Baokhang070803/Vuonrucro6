using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;



public enum TilemapState
{
    Ground,
    Grass,
    Forest,
    Sunflower,
    GrapeFruit,
    Pepper,
    Pumpkin,    // Thêm cho bí ngô
    Eggplant,   // Thêm cho cà tím
}

public class TilemapDetail 
{
    public int x { get; set; }
    public int y { get; set; }

    public TilemapState tilemapState { get; set; }

    public TilemapDetail()
    {
       
    }

    public TilemapDetail(int x, int y, TilemapState tilemapState)
    {
        this.x = x;
        this.y = y;
        this.tilemapState = tilemapState;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
