using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NewWorld : MonoBehaviour
{
    public Tilemap tilemapGround;
    public TileBase tile;
    public TileBase tile2;

    TileBase temptile;

    //public tilemap


    // Start is called before the first frame update
    void Start()
    {
        tilemapGround.SetTile(new Vector3Int(0, 0, 0), tile);
        //tilemap.SetTile(new Vector3Int(-2, 0, 0), tile2);
        //temptile = tilemap.GetTile(new Vector3Int(-2, 0, 0));
        //tilemap.FloodFill(new Vector3Int(0, 0, 0), tile);
        //tilemap.FloodFill(new Vector3Int(4, 1, 0), tile);
        //Vector3 pos = new Vector3(5.5f, 3.5f, 0f);
        Debug.Log(tilemapGround.GetTile(tilemapGround.WorldToCell(new Vector3(5.5f, -3.5f, 0))));

    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Space))
        //{
        //    tilemap.SetTile(new Vector3Int(0, 0, 0), temptile);
        //}
    }


}
