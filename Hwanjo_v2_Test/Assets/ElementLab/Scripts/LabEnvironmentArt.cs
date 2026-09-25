using UnityEngine;

namespace Hwanjo.ElementLab
{
    public static class LabEnvironmentArt
    {
        public static bool Platform(LabArtLibrary art,Transform parent,float width,float depth)
        {
            if(!art || art.Terrain==null || art.Terrain.Length<8)return false;
            const float tile=.8f;
            int cols=Mathf.CeilToInt(width/tile),rows=Mathf.CeilToInt(depth/tile);
            for(int y=0;y<rows;y++)for(int x=0;x<cols;x++)
            {
                float w=Mathf.Min(tile,width-x*tile),h=Mathf.Min(tile,depth-y*tile);
                int index=y==0?(x==0?0:x==cols-1?2:1):y==rows-1?(x==0?4:x==cols-1?6:5):3;
                var go=new GameObject("Stone tile "+index);go.transform.SetParent(parent,false);
                go.transform.localPosition=new Vector3(-width/2+x*tile+w/2,depth/2-y*tile-h/2,0);
                var sr=go.AddComponent<SpriteRenderer>();sr.sprite=art.Terrain[index];sr.sortingOrder=6;
                go.transform.localScale=new Vector3(w/sr.sprite.bounds.size.x,h/sr.sprite.bounds.size.y,1);
            }
            return true;
        }
    }
}
